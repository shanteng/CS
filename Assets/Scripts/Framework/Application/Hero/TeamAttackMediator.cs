
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TeamAttackMediator : BaseWindowMediator<TeamAttackView>
{
    public TeamAttackMediator() : base(MediatorDefine.TEAM_ATTACK, WindowLayer.Window)
    {
        
    }

   
    protected override void InitListNotificationInterestsInner()
    {
        m_HideNoHandleNotifations.Add(NotiDefine.MoveToAttackCityResp);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.MoveToAttackCityResp:
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
        this.m_view.InitData((int)this.ShowData);
    }

    protected override void hideWindowInner() 
    {
        
    }
}//end class