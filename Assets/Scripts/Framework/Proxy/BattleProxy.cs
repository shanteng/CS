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

    public void OnTeamBegin(int teamid)
    {
        this._data.Status = BattleStatus.Action;
        foreach (BattlePlayer pl in this._data.Players.Values)
        {
            if (pl.TeamID == teamid)
            {
                pl.Status = PlayerStatus.Action;
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

    public BattlePlayer GetPlayer(int teamid)
    {
        BattlePlayer player = this.Data.Players[teamid];
        return player;
    }

    private Dictionary<int, Vector3> _bornPosDic;
    public void SaveBornPostion(Dictionary<int, Vector3> bornPos)
    {
        this._bornPosDic = bornPos;
        foreach (BattlePlayer pl in this._data.Players.Values)
        {
            if (pl.TeamID > 0)
                continue;
            pl.Postion = this._bornPosDic[pl.BornIndex];
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
                oldPlayer.Postion = this._bornPosDic[oldPlayer.BornIndex];
            else
                oldPlayer.Postion = Vector3.zero;
        }
           
        curPlayer.BornIndex = bornIndex;
        curPlayer.Postion = this._bornPosDic[curPlayer.BornIndex];
        this.SendNotification(NotiDefine.BattleBornUpdate);
    }

    public void UnMySetPlayerBorn(int teamid)
    {
        BattlePlayer curPlayer = this.GetPlayer(teamid);
        if (curPlayer.BornIndex == 0)
            return;
        curPlayer.BornIndex = 0;
        curPlayer.Postion = Vector3.zero;
        this.SendNotification(NotiDefine.BattleBornUpdate);
    }

}//end class
