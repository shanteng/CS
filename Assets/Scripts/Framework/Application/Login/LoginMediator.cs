
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
    }

   

    protected override void InitListNotificationInterestsInner()
    {
        //m_HideNoHandleNotifations.Add(NotiDefine.LOAD_HOME_SCENE_FINISH);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
           /*( case NotiDefine.LOAD_HOME_SCENE_FINISH:
                {
                    MediatorUtil.hideMediator(MediatorDefine.LOGIN);
                    break;
                }
           */
        }
    }//end func

    protected override void InitViewComponent(GameObject view)
    {

    }

    protected override void DoInitializeInner()
    {

    }

}//end class