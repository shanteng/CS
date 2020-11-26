
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
        m_lInterestNotifications.Add(NotiDefine.GroupReachCityNoti);
        m_lInterestNotifications.Add(NotiDefine.GroupBackCityNoti);
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.PatrolFinishNoti:
            case NotiDefine.GroupReachCityNoti:
            case NotiDefine.GroupBackCityNoti:
                {
                    PopupFactory.Instance.HandleNoti(notification);
                    break;
                }
        }
    }//end 
}//end class