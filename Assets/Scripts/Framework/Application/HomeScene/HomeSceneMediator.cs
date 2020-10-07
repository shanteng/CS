
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;

public class HomeSceneMediator : BaseNoWindowMediator
{
    public HomeSceneMediator() : base(MediatorDefine.HOME_SCENE)
    {
    }

    protected override void InitListNotificationInterestsInner()
    {
        m_lInterestNotifications.Add(NotiDefine.MVC_STARTED);
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.MVC_STARTED:
                {
                    
                    break;
                }
        }
    }//end 
}//end class