
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
        this.m_HideNoHandleNotifations.Add(NotiDefine.ResLimitHasUpdated);
        this.m_HideNoHandleNotifations.Add(NotiDefine.RoleLvExpHasUpdated);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.NumberValueHasUpdated:
            case NotiDefine.IncomeHasUpdated:
            case NotiDefine.ResLimitHasUpdated:
                {
                    this.m_view.UpdateIncome();
                    break;
                }
            case NotiDefine.RoleLvExpHasUpdated:
                {
                    this.m_view.SetLevelExp();
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
        this.m_view.SetLevelExp();
    }

}//end class