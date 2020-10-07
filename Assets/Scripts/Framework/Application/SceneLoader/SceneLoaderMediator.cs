
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;

public class SceneLoaderMediator : BaseFullScreenWindowMediator<SceneLoaderView>
{
    private string _name;
    public SceneLoaderMediator() : base(MediatorDefine.SCENE_LOADER)
    {
        this._prefabName = "SceneLoaderPF";
    }

   
    protected override void InitListNotificationInterestsInner()
    {
        m_lInterestNotifications.Add(NotiDefine.DO_LOAD_SCENE);
        m_HideNoHandleNotifations.Add(NotiDefine.LOAD_SCENE_FINISH);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.DO_LOAD_SCENE:
                {
                    this._name = (string)notification.Body;
                    MediatorUtil.ShowMediator(MediatorDefine.SCENE_LOADER);
                    break;
                }
            case NotiDefine.LOAD_SCENE_FINISH:
                {
                    MediatorUtil.HideMediator(MediatorDefine.SCENE_LOADER);
                    break;
                }
        }
    }//end func

    protected override void InitViewComponent(GameObject view)
    {

    }

    protected override void DoInitializeInner()
    {
        this.m_view.LoadScene(this._name);
    }
}//end class