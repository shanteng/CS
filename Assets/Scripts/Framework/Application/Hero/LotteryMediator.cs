
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LotteryMediator : BaseWindowMediator<LotteryView>
{
    public LotteryMediator() : base(MediatorDefine.LOTTERY, WindowLayer.Window)
    {
        
    }

   
    protected override void InitListNotificationInterestsInner()
    {
        //m_HideNoHandleNotifations.Add(NotiDefine.SaveAwardPlayers);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.SaveAwardPlayers:
                {
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
        
    }
}//end class