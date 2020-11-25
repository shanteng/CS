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

    public void EnterBattle(BattleData data)
    {
        this._data = data;
        this.SendNotification(NotiDefine.EnterBattleSuccess,data.Id);//加载场景，进行布阵
        //测试写死直接胜利
        //this.BattleEnd(true);
    }

    public void DoAttackAction()
    {
        BattlePlayer actionPlayer = this.GetActionPlayer();
        //计算所有被伤害的人
        int skillID = actionPlayer._AttackSkillID;
        int AttackDemage = actionPlayer.ComputeDemage(skillID);//根据技能和属性计算出本次的输出伤害
        List<PlayerBloodChangeData> attackPlayers = new List<PlayerBloodChangeData>();
        foreach (VInt2 attackPos in actionPlayer.SkillDemageCordinates)
        {
            BattlePlayer posPlayer = this.GetPlayerBy(attackPos);
            if (posPlayer == null || posPlayer.Status != PlayerStatus.Wait)
                continue;
            int teamvalue = actionPlayer.TeamID * posPlayer.TeamID;
            if (teamvalue > 0)
                continue;//相同队伍的不受伤害
            int loseBlood = posPlayer.TakeDemage(AttackDemage);
            PlayerBloodChangeData kv = new PlayerBloodChangeData();
            kv.TeamID = posPlayer.TeamID;
            kv.ChangeValue = -loseBlood;
            attackPlayers.Add(kv);
        }

        //播放完毕动画再去处理UI
        BattleController.Instance.TakeDeamge(attackPlayers);
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
                pl.Status = PlayerStatus.Wait;
                break;
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
        pl._AttackSkillID = 0;
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

    public void BattleEnd(bool isSuccess)
    {
        if (this.Data.Type == BattleType.AttackCity)
        {
            string groupid = (string)this.Data.Param;
            Group gp = TeamProxy._instance.GetGroup(groupid);
            if (gp == null) return;
            if (isSuccess)
                WorldProxy._instance.DoOwnCity(gp.TargetCityID);
            TeamProxy._instance.AttackCityEnd(groupid, isSuccess);
        }
        this.SendNotification(NotiDefine.BattleEndNoti);
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
