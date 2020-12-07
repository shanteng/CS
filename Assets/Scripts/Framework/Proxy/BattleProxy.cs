using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;


public class BattleProxy : BaseRemoteProxy
{
    private BattleData _data;
    public BattleData Data => this._data;
    public static BattleProxy _instance;
    public BattleProxy() : base(ProxyNameDefine.BATTLE)
    {
        _instance = this;
    }

    public bool EnterBattle(BattleData data)
    {
        if(data.Players.Count == 0)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.NoTeamCanFight);
            return false;
        }
        this._data = data;
        this.SendNotification(NotiDefine.EnterBattleSuccess,data.Id);//加载场景，进行布阵
        return true;
        //测试写死直接胜利
        //this.BattleEnd(true);
    }

    public List<AiStep> DoAi(BattlePlayer player)
    {
        List<AiStep> AiSteps = new List<AiStep>();
        //判断攻击范围内是否有可攻击的敌方
        //先判断普通攻击范围

        int myMp = (int)player.Attributes[AttributeDefine.Mp];
        List<SkillAiStep> skillSteps = new List<SkillAiStep>();

        SkillAiStep attackStep;
        this.PlayerSkillAutoReleaseInPostion(player, 0,out attackStep);
        if (attackStep._Postion != null)
        {
            skillSteps.Add(attackStep);
        }

        foreach (BattleSkill skill in player._SkillDatas.Values)
        {
            SkillConfig config = SkillConfig.Instance.GetData(skill.ID);
            if (config.ReleaseTerm.Equals(SkillReleaseTerm.Manual) == false)
                continue;

            if (myMp < config.MpCost)
                continue;
            SkillAiStep skillAttackStep;
            this.PlayerSkillAutoReleaseInPostion(player,skill.ID,out skillAttackStep);
            if (skillAttackStep._Postion != null)
            {
                skillSteps.Add(skillAttackStep);
            }
        }

        //先测试自动释放范围内的Ai
        if (skillSteps.Count > 0)
        {
            //临时用第0个先测试
            AiStep step = new AiStep();
            step._Step = AiStepType.ReleaseSkill;
            step._Postion = new VInt2(skillSteps[0]._Postion.x, skillSteps[0]._Postion.y);
            step._SkillID = skillSteps[0]._SkillID;
            AiSteps.Add(step);
        }

        //攻击结束后，直接停止等待
        AiStep stepEnd = new AiStep();
        stepEnd._Step = AiStepType.End;
        AiSteps.Add(stepEnd);

        return AiSteps;
    }

    public void PlayerSkillAutoReleaseInPostion(BattlePlayer player,int skillid,out SkillAiStep attackStep)
    {
        int actionTeamID = player.TeamID;
        attackStep = new SkillAiStep();
        attackStep._SkillID = skillid;
        if (skillid == 0)
        {
            attackStep.MainEffectTypeID = 0;
        }
        else
        {
            SkillConfig config = SkillConfig.Instance.GetData(skillid);
            attackStep.MainEffectTypeID = config.EffectIDs[0];
        }

        SkillEffectConfig typeConfig = SkillEffectConfig.Instance.GetData(attackStep.MainEffectTypeID);

        player.ComputeSkillFightRange(skillid);
        foreach (VInt2 pos in player.SkillFightRangeCordinates)
        {
            //计算伤害范围内是否有可攻击的敌人
            player.ComputeSkillDemageRange(pos, skillid);
            VInt2 hasPlayerPos = null;
            foreach (VInt2 attackPos in player.SkillDemageCordinates)
            {
                int validPlayerId = this.GetValidPlayerInPostion(attackPos.x, attackPos.y);
                if (validPlayerId == 0)
                    continue;
                bool isSameTeam = actionTeamID * validPlayerId > 0;
                bool isMySelf = validPlayerId == actionTeamID;
                bool isSelfOther = isSameTeam && isMySelf == false;
                bool isEnemy = isSameTeam == false;
                if (typeConfig.Target.Equals(SkillEffectTarget.Enemy) && isEnemy == false)
                    continue;

                if (typeConfig.Target.Equals(SkillEffectTarget.Self) && isMySelf == false)
                    continue;

                if (typeConfig.Target.Equals(SkillEffectTarget.Self_All) && isSameTeam == false)
                    continue;

                if (typeConfig.Target.Equals(SkillEffectTarget.Self_Other) && (isSameTeam == false || isMySelf))
                    continue;
                //可以释放了
                hasPlayerPos = attackPos;
                break;
            }//end foreach

            if (hasPlayerPos != null)
            {
                attackStep._Postion = new VInt2(hasPlayerPos.x, hasPlayerPos.y);
                break;
            }
        }//end foreach
    }

    public int GetValidPlayerInPostion(int x, int y)
    {
        foreach (BattlePlayer pl in this.Data.Players.Values)
        {
            bool isValid = this.IsValidPlayer(pl);
            if (isValid == false)
                continue;
            if (pl.Postion.x == x && pl.Postion.y == y)
                return pl.TeamID;
        }
        return 0;
    }

    public bool IsValidPlayer(BattlePlayer pl)
    {
        return pl.Status == PlayerStatus.Wait || pl.Status == PlayerStatus.Action || pl.Status == PlayerStatus.ActionFinished;
    }

    public List<PlayerEffectChangeData> ManualReleaseAction()
    {
        BattlePlayer actionPlayer = this.GetActionPlayer();
        int attackerTeamID = actionPlayer.TeamID;
        int skillID = actionPlayer._AttackSkillID;
        List<PlayerEffectChangeData> effectPlayers = actionPlayer.ReleaseSkill(skillID, _data);

        if (skillID == 0)
        {
            //连携判断
            //释放被动技能判断
            List<PlayerEffectChangeData> players = actionPlayer.ReleaseAfterAttackSKill(this.Data);
            effectPlayers.AddRange(players);
        }

        //判断战斗结束
        bool hasAliveOpppTeamID = false;
        foreach (BattlePlayer pl in this.Data.Players.Values)
        {
            int teamvalue = actionPlayer.TeamID * pl.TeamID;
            if (teamvalue < 0 && pl.Attributes[AttributeDefine.Blood] > 0)
            {
                hasAliveOpppTeamID = true;
                break;
            }
        }

        if (hasAliveOpppTeamID == false)
        {
            //判断战斗结束，胜利和失败
            this.DoEndBattleResult(attackerTeamID > 0);
        }

        return effectPlayers;
    }

    public void DoEndBattleResult(bool IsWin)
    {
        this._data.IsGameOver = true;
        this._data.IsWin = IsWin;
        this._data.Status = BattleStatus.End;
        BattleController.Instance.StopAi();
        //进行战斗数据结算
        //根据上阵伤害，平均分配战斗經驗

        //城市攻占
        if (this._data.Type == BattleType.AttackCity)
        {
            int TargetCityID = (int)BattleProxy._instance.Data.Param;
            if(this._data.IsWin)
                WorldProxy._instance.DoOwnCity(TargetCityID);
            TeamProxy._instance.AttackCityEnd(TargetCityID, this._data);
        }
    }

    public BattlePlayer GetPlayerBy(VInt2 pos)
    {
        foreach (BattlePlayer pl in this.Data.Players.Values)
        {
            if (pl.Postion.x == pos.x && pl.Postion.y == pos.y)
                return pl;
        }
        return null;
    }

    public void DoNextRound()
    {
        this._data.Status = BattleStatus.Judge;
    

        foreach (BattlePlayer pl in this._data.Players.Values)
        {
            if (pl.Status == PlayerStatus.Action || pl.Status == PlayerStatus.ActionFinished)
            {
                //清除当前玩家身上Buff的Duration
                pl.Status = PlayerStatus.Wait;
                pl.EndOneRoundBuff();
                break;
            }
        }

        this._data.Round++;
        this.SendNotification(NotiDefine.BattleStateChangeNoti, this._data.Status);
    }



    public void OnPlayerStartAction(int teamid)
    {
        List<PlayerEffectChangeData> allPlayerEffect = new List<PlayerEffectChangeData>();
        this._data.Status = BattleStatus.Action;
        BattlePlayer actionPlayer = this._data.Players[teamid];

        //行动玩家恢复一次Mp
        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.BattleMpRecoverForOneRound);
        int RecoverValue = cfgconst.IntValues[0];
        float curMp = actionPlayer.Attributes[AttributeDefine.Mp] + RecoverValue;
        float OrMp = actionPlayer.Attributes[AttributeDefine.OrignalMp];
        if (curMp <= OrMp)
            actionPlayer.Attributes[AttributeDefine.Mp] = curMp;
        else
            actionPlayer.Attributes[AttributeDefine.Mp] = OrMp;

        actionPlayer.Status = PlayerStatus.Action;
        actionPlayer.HasDoRoundActionFinish = false;

        //释放被动技能判断
        List<PlayerEffectChangeData> players = actionPlayer.ReleaseSelfRoundSKill(this.Data);
        allPlayerEffect.AddRange(players);
       
        //执行自带buff当前回合效果
        Dictionary<string, BattleEffectShowData> selfChanges = actionPlayer.DoSelfRoundBuffEffect(this.Data);
        if (selfChanges.Count > 0)
        {
            PlayerEffectChangeData actionTeamEffect = null;
            foreach (PlayerEffectChangeData effect in allPlayerEffect)
            {
                if (effect.TeamID == teamid)
                {
                    actionTeamEffect = effect;
                    break;
                }
            }//end foreach

            if (actionTeamEffect == null)
            {
                actionTeamEffect = new PlayerEffectChangeData();
                actionTeamEffect.TeamID = teamid;
                allPlayerEffect.Add(actionTeamEffect);
            }

            foreach (BattleEffectShowData cur in selfChanges.Values)
            {
                actionTeamEffect.ChangeShowDatas[cur.Type] = cur;
            }
        }

        BattleController.Instance.EffectPlayerResponseToSkill(allPlayerEffect, null);
        this._data.Round++;
        this.SendNotification(NotiDefine.BattleStateChangeNoti,this._data.Status);
    }

    public void OnPlayerActionFinishded(int teamid)
    {
        BattlePlayer pl = this._data.Players[teamid];
        pl.Status = PlayerStatus.ActionFinished;
        pl.ActionMoveCordinates = null;
        pl.SkillFightRangeCordinates = null;
        pl.SkillFightRangeCordinatesStrList = null;
        pl.SkillDemageCordinates = null;
        pl._AttackSkillID = -1;
    }

    public void StartFight()
    {
        bool hasUp = false;
        foreach (BattlePlayer pl in this.Data.Players.Values)
        {
            if (pl.BornIndex > 0 && pl.TeamID > 0)
            {
                hasUp = true;
                break;
            }
        }

        if (!hasUp)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.NoUpPlayer);
            return;
        }

        this.SendNotification(NotiDefine.PreBattleStartNoti);

        //每个参战玩家战前被动技能判断和状态设置
        foreach (BattlePlayer pl in this.Data.Players.Values)
        {
            if (pl.BornIndex == 0)
                continue;
            pl.Status = PlayerStatus.Wait;
        }

        List<PlayerEffectChangeData> allPlayerEffect = new List<PlayerEffectChangeData>();
        foreach (BattlePlayer pl in this.Data.Players.Values)
        {
            if (pl.BornIndex == 0)
                continue;
            List<PlayerEffectChangeData> effects =  pl.ReleaseBeforeStartSKill(this.Data);
            allPlayerEffect.AddRange(effects);
        }
        BattleController.Instance.EffectPlayerResponseToSkill(allPlayerEffect,this.OnPreStartSkillEnd);
       
    }

    public void OnPreStartSkillEnd()
    {
        this._data.Status = BattleStatus.Judge;
        this._data.Round = 1;
        this.SendNotification(NotiDefine.BattleStartNoti);
    }

    public bool IsSpotOccupy(int x, int z, int teamid)
    {
        foreach (BattlePlayer player in this.Data.Players.Values)
        {
            if (player.TeamID == teamid || player.Status != PlayerStatus.Wait)
                continue;
            if(player.Postion.x == x && player.Postion.y == z)
                return true;
        }
        return false;
    }

    public BattlePlayer GetPlayer(int teamid)
    {
        BattlePlayer player = this.Data.Players[teamid];
        return player;
    }

    public BattlePlayer GetActionPlayer()
    {
        foreach (BattlePlayer pl in this.Data.Players.Values)
        {
            if (pl.Status == PlayerStatus.Action)
                return pl;
        }
        return null;
    }

    private Dictionary<int, Vector3> _bornPosDic;
    public void SaveBornPostion(Dictionary<int, Vector3> bornPos)
    {
        this._bornPosDic = bornPos;
        foreach (BattlePlayer pl in this._data.Players.Values)
        {
            if (pl.TeamID > 0)
                continue;
            pl.Postion = new VInt2((int)this._bornPosDic[pl.BornIndex].x,(int)this._bornPosDic[pl.BornIndex].z);
        }
    }

    public void SetMyPlayerBorn(int teamid, int bornIndex)
    {
        BattlePlayer oldPlayer = null;
        foreach (BattlePlayer p in this.Data.Players.Values)
        {
            if (p.BornIndex == bornIndex)
            {
                oldPlayer = p;
                break;
            }
        }

        BattlePlayer curPlayer = this.GetPlayer(teamid);
        if (oldPlayer != null)
        {
            oldPlayer.BornIndex = curPlayer.BornIndex;
            if (oldPlayer.BornIndex > 0)
                oldPlayer.Postion = new VInt2((int)this._bornPosDic[oldPlayer.BornIndex].x, (int)this._bornPosDic[oldPlayer.BornIndex].z);
            else
                oldPlayer.Postion = new VInt2();
        }
           
        curPlayer.BornIndex = bornIndex;
        curPlayer.Postion = new VInt2((int)this._bornPosDic[curPlayer.BornIndex].x, (int)this._bornPosDic[curPlayer.BornIndex].z);
        this.SendNotification(NotiDefine.BattleBornUpdate);
    }

    public void UnMySetPlayerBorn(int teamid)
    {
        BattlePlayer curPlayer = this.GetPlayer(teamid);
        if (curPlayer.BornIndex == 0)
            return;
        curPlayer.BornIndex = 0;
        curPlayer.Postion = new VInt2();
        this.SendNotification(NotiDefine.BattleBornUpdate);
    }

    public void SetPlayerPostion(int teamid,int x,int z)
    {
        BattlePlayer curPlayer = this.GetPlayer(teamid);
        curPlayer.Postion.x = x;
        curPlayer.Postion.y = z;
    }

}//end class
