﻿
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class ArmyMediator : BaseWindowMediator<ArmyView>
{
    public ArmyMediator() : base(MediatorDefine.ARMY, WindowLayer.Window)
    {
        
    }

   
    protected override void InitListNotificationInterestsInner()
    {
        m_HideNoHandleNotifations.Add(NotiDefine.ArmyStateChange);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.ArmyStateChange:
                {
                    Army army = (Army)notification.Body;
                    if (army.CityID == this.m_view.City)
                    {
                        ArmyConfig config = ArmyConfig.Instance.GetData(army.Id);
                        this.m_view.UpdateState(config.Career);
                        this.m_view.UpdateToggle();
                    }
                    break;
                }
        }
    }//end func

    protected override void InitViewComponent(GameObject view)
    {

    }

    protected override void DoInitializeInner()
    {
        this.m_view.SetList((VInt2)this.ShowData);
    }

}//end class