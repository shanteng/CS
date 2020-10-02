
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;

public class LoginMediator : BaseFullScreenWindowMediator<LoginView>
{
    public LoginMediator() : base(MediatorDefine.LOGIN)
    {
        this._prefabName = "LoginPF";
        this.DestroyWhenHide = true;
    }

    protected override void InitViewComponent(GameObject view)
    {

    }

    protected override void InitListNotificationInterestsInner()
    {
        m_lInterestNotifications.Add(NotiDefine.LOAD_MAIN_SCENE_FINISH);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.LOAD_MAIN_SCENE_FINISH:
                {
                    if (this.windowVisible == false)
                        return;
                    MediatorUtil.hideMediator(MediatorDefine.LOGIN);
                    break;
                }
        }
    }//end func
}//end class