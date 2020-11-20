
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TeamMediator : BaseWindowMediator<TeamView>
{
    public TeamMediator() : base(MediatorDefine.TEAM, WindowLayer.Window)
    {
        
    }

   
    protected override void InitListNotificationInterestsInner()
    {
        m_HideNoHandleNotifations.Add(NotiDefine.SetTeamHeroResp);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.SetTeamHeroResp:
                {
                    this.m_view.UpdateTeamList();
                    break;
                }
        }
    }//end func

    protected override void InitViewComponent(GameObject view)
    {

    }

    protected override void DoInitializeInner()
    {
        this.m_view.SetList((int)this.ShowData);
    }

    protected override void hideWindowInner() 
    {
        
    }

}//end class