
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
        //_HideNoHandleNotifations.Add(NotiDefine.TEST_CALLBACK_NOTI);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.TEST_CALLBACK_NOTI:
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

    }

}//end class