
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;

public class DataCenterMediator : BaseNoWindowMediator
{
    public DataCenterMediator() : base(MediatorDefine.DATA_CENTER)
    {
    }

    protected override void InitListNotificationInterestsInner()
    {
        m_lInterestNotifications.Add(NotiDefine.PatrolFinishNoti);
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.PatrolFinishNoti:
                {
                    PopupFactory.Instance.HandleNoti(notification);
                    break;
                }
        }
    }//end 
}//end class