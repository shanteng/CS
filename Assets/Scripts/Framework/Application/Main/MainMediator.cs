﻿
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class MainMediator : BaseWindowMediator<MainView>
{
    public MainMediator() : base(MediatorDefine.MAIN, WindowLayer.Main)
    {

    }


    protected override void InitListNotificationInterestsInner()
    {
        this.m_HideNoHandleNotifations.Add(NotiDefine.NumberValueHasUpdated);
        this.m_HideNoHandleNotifations.Add(NotiDefine.ResLimitHasUpdated);
        this.m_HideNoHandleNotifations.Add(NotiDefine.JudgeIncome);
        this.m_HideNoHandleNotifations.Add(NotiDefine.RoleLvExpHasUpdated);
        this.m_HideNoHandleNotifations.Add(NotiDefine.CordinateChange);
        this.m_HideNoHandleNotifations.Add(NotiDefine.PowerChanged);
        this.m_HideNoHandleNotifations.Add(NotiDefine.PathAddNoti);
        this.m_HideNoHandleNotifations.Add(NotiDefine.PathRemoveNoti);
        this.m_HideNoHandleNotifations.Add(NotiDefine.NewLogNoti);
        this.m_HideNoHandleNotifations.Add(NotiDefine.JudegeNewLog);
        this.m_HideNoHandleNotifations.Add(NotiDefine.EnterBattleSuccess);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.NumberValueHasUpdated:
            case NotiDefine.ResLimitHasUpdated:
                {
                    this.m_view.UpdateIncome();
                    break;
                }
            case NotiDefine.JudgeIncome:
                {
                    this.m_view.JudgeIncome();
                    break;
                }
            case NotiDefine.RoleLvExpHasUpdated:
                {
                    this.m_view.SetLevelExp();
                    break;
                }
            case NotiDefine.CordinateChange:
                {
                    this.m_view._mapUI.SetCordinate();
                    break;
                }
            case NotiDefine.PowerChanged:
                {
                    this.m_view.SetPower();
                    break;
                }
            case NotiDefine.PathAddNoti:
            case NotiDefine.PathRemoveNoti:
                {
                    this.m_view.SetPathState();
                    break;
                }
            case NotiDefine.NewLogNoti:
                {
                    this.m_view.SetLogNew(true);
                    break;
                }
            case NotiDefine.JudegeNewLog:
                {
                    this.m_view.SetLogNew();
                    break;
                }
            case NotiDefine.EnterBattleSuccess:
                {
                    this.HideWindow();
                    break;
                }
        }
    }//end func

    protected override void InitViewComponent(GameObject view)
    {

    }

    protected override void DoInitializeInner()
    {
        this.m_view.UpdateIncome();
        this.m_view.SetName();
        this.m_view.SetLevelExp();
        this.m_view._mapUI.SetCordinate();
        this.m_view.SetPower();
        this.m_view.SetPathState();
        this.m_view.SetLogNew();
    }

}//end class