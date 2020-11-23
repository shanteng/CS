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

}//end class
