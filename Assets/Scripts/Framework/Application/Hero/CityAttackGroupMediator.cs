
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CityAttackGroupMediator : BaseWindowMediator<CityAttackGroupView>
{
    public CityAttackGroupMediator() : base(MediatorDefine.ATTACK_CITY_GROUP, WindowLayer.Window)
    {
        
    }

   
    protected override void InitListNotificationInterestsInner()
    {
        m_HideNoHandleNotifations.Add(NotiDefine.GroupReachCityNoti);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.GroupReachCityNoti:
                {
                    this.m_view.UpdateState();
                    break;
                }
        }
    }//end func

    protected override void InitViewComponent(GameObject view)
    {

    }

    protected override void DoInitializeInner()
    {
        this.m_view.SetData((int)this.ShowData);
    }

    protected override void hideWindowInner() 
    {
        
    }

}//end class