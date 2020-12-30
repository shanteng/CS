
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class QuestionMediator : BaseWindowMediator<QuestionView>
{
    public QuestionMediator() : base(MediatorDefine.QUESTION, WindowLayer.Window)
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
        this.m_view.SetRandomQuestion((int)this.ShowData);
    }

    protected override void hideWindowInner() 
    {
        HeroProxy._instance.TalkToHero((int)this.ShowData, this.m_view.Correct);
    }

}//end class