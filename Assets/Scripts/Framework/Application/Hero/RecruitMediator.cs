
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class RecruitMediator : BaseWindowMediator<RecruitView>
{
    public RecruitMediator() : base(MediatorDefine.RECRUIT, WindowLayer.FullScreen)
    {
        
    }

   
    protected override void InitListNotificationInterestsInner()
    {
        m_HideNoHandleNotifations.Add(NotiDefine.GetHeroRefreshResp);
        m_HideNoHandleNotifations.Add(NotiDefine.HeroTavernRefresh);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.GetHeroRefreshResp:
            case NotiDefine.HeroTavernRefresh:
                {
                    this.m_view.SetList();
                    break;
                }
        }
    }//end func

    protected override void InitViewComponent(GameObject view)
    {

    }

    protected override void DoInitializeInner()
    {
        this.m_view.SetCity((int)this.ShowData);
    }

}//end class