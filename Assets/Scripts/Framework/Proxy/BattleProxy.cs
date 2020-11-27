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

    public void DoPlayerAttackAction()
    {
        BattlePlayer actionPlayer = this.GetActionPlayer();
        int attackerTeamID = actionPlayer.TeamID;
        List<PlayerEffectChangeData> effectPlayers = new List<PlayerEffectChangeData>();

        int skillID = actionPlayer._AttackSkillID;
        if (skillID == 0)
        {
            //计算普通攻击伤害的人
            int AttackDemage = actionPlayer.ComputeDemage(skillID);//根据技能和属性计算出本次的输出伤害
            foreach (VInt2 attackPos in actionPlayer.SkillDemageCordinates)
            {
                BattlePlayer posPlayer = this.GetPlayerBy(attackPos);
                if (posPlayer == null || posPlayer.Status != PlayerStatus.Wait)
                    continue;
                int teamvalue = actionPlayer.TeamID * posPlayer.TeamID;
                if (teamvalue > 0)
                    continue;//相同队伍的不受伤害
                int loseBlood = posPlayer.TakeDemage(AttackDemage);
                PlayerEffectChangeData kv = new PlayerEffectChangeData();
                kv.TeamID = posPlayer.TeamID;
                kv.ChangeValue = -loseBlood;
                effectPlayers.Add(kv);
            }
        }
        else
        {
            //技能效果判断,扣除Mp
            SkillConfig cfg = SkillConfig.Instance.GetData(skillID);
            float curMp = actionPlayer.Attributes[AttributeDefine.Mp] - cfg.MpCost;
            actionPlayer.Attributes[AttributeDefine.Mp] = curMp < 0 ? 0 : curMp;
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

        //播放完毕动画再去处理UI
        BattleController.Instance.ResponseToSkillBehaviour(effectPlayers);
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
        
        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.BattleMpRecoverForOneRound);
        int RecoverValue = cfgconst.IntValues[0];

        foreach (BattlePlayer pl in this._data.Players.Values)
        {
            if (pl.Status == PlayerStatus.Action || pl.Status == PlayerStatus.ActionFinished)
            {
                pl.Status = PlayerStatus.Wait;
            }

            //所有玩家恢复一次Mp
            if (pl.Status != PlayerStatus.Dead)
            {
                float curMp = pl.Attributes[AttributeDefine.Mp] + RecoverValue;
                float OrMp = pl.Attributes[AttributeDefine.OrignalMp];
                if (curMp <= OrMp)
                    pl.Attributes[AttributeDefine.Mp] = curMp;
                else
                    pl.Attributes[AttributeDefine.Mp] = OrMp;
            }
        }

        this._data.Round++;
        this.SendNotification(NotiDefine.BattleStateChangeNoti, this._data.Status);
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

    public void OnTeamBegin(int teamid)
    {
        this._data.Status = BattleStatus.Action;
        foreach (BattlePlayer pl in this._data.Players.Values)
        {
            if (pl.TeamID == teamid)
            {
                pl.Status = PlayerStatus.Action;
                pl.HasDoRoundActionFinish = false;
                break;
            }
        }
        this._data.Round++;
        this.SendNotification(NotiDefine.BattleStateChangeNoti,this._data.Status);
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

        foreach (BattlePlayer pl in this.Data.Players.Values)
        {
            pl.Status = PlayerStatus.Wait;
        }

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
