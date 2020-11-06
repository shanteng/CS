
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
        m_HideNoHandleNotifations.Add(NotiDefine.FavorLevelUpNoti);
        m_HideNoHandleNotifations.Add(NotiDefine.RecruitHeroResp);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.GetHeroRefreshResp:
            case NotiDefine.HeroTavernRefresh:
                {
                    int cityid = (int)notification.Body;
                    if(cityid == this.m_view.City)
                        this.m_view.SetList();
                    break;
                }
            case NotiDefine.RecruitHeroResp:
                {
                    this.m_view.SetList();
                    break;
                }
            case NotiDefine.FavorLevelUpNoti:
                {
                    this.m_view.OnFavorUp((int)notification.Body);
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