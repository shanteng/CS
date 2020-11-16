
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class GameLogMediator : BaseWindowMediator<GameLogView>
{
    public GameLogMediator() : base(MediatorDefine.GAME_LOG, WindowLayer.Window)
    {
        
    }

   
    protected override void InitListNotificationInterestsInner()
    {
        m_HideNoHandleNotifations.Add(NotiDefine.NewLogNoti);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.NewLogNoti:
                {
                    LogData data = (LogData)notification.Body;
                    this.m_view.AddOne(data);
                    break;
                }
        }
    }//end func

    protected override void InitViewComponent(GameObject view)
    {
        MediatorUtil.SendNotification(NotiDefine.JudegeNewLog);
    }

    protected override void DoInitializeInner()
    {
        this.m_view.DoShow();
        this.m_view.SetList();
    }

    protected override void hideWindowInner()
    {
        MediatorUtil.SendNotification(NotiDefine.JudegeNewLog);
    }

}//end class