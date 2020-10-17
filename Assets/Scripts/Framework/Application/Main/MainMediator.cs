
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
        this.m_HideNoHandleNotifations.Add(NotiDefine.NumberValueHasUpdated);
        this.m_HideNoHandleNotifations.Add(NotiDefine.IncomeHasUpdated);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.NumberValueHasUpdated:
            case NotiDefine.IncomeHasUpdated:
                {
                    this.m_view.UpdateIncome();
                    break;
                }
        }
    }//end func

    protected override void InitViewComponent(GameObject view)
    {

    }

    protected override void DoInitializeInner()
    {
        this.m_view.UpdateIncome();
        this.m_view.SetName();
    }

}//end class