
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class CreateMediator : BaseFullScreenWindowMediator<CreateView>
{
    public CreateMediator() : base(MediatorDefine.CREATE)
    {
        
    }

    protected override void InitListNotificationInterestsInner()
    {
        m_HideNoHandleNotifations.Add(NotiDefine.CreateRoleResp);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.CreateRoleResp:
                {
                    this.HideWindow();
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

}//end class