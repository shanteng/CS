
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class HeroMediator : BaseWindowMediator<HeroView>
{
    public HeroMediator() : base(MediatorDefine.HERO, WindowLayer.FullScreen)
    {
        
    }

   
    protected override void InitListNotificationInterestsInner()
    {
        //m_HideNoHandleNotifations.Add(NotiDefine.GetHeroRefreshResp);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.GetHeroRefreshResp:
                {
                    //this.m_view.SetList();
                    break;
                }
        }
    }//end func

    protected override void InitViewComponent(GameObject view)
    {

    }

    protected override void DoInitializeInner()
    {
        this.m_view.InitData();
    }

    protected override void hideWindowInner() 
    {
       SceneManager.UnloadSceneAsync(SceneDefine.Hero);
        UIRoot.Intance.SetHomeSceneEnable(true);
    }

}//end class