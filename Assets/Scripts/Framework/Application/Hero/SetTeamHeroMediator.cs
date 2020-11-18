
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SetTeamHeroMediator : BaseWindowMediator<SetTeamHeroView>
{
    public SetTeamHeroMediator() : base(MediatorDefine.SET_TEAM_HERO, WindowLayer.Window)
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
                    MediatorUtil.HideMediator(MediatorDefine.SET_TEAM_HERO);
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