
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
        m_lInterestNotifications.Add(NotiDefine.DoLoadScene);
        m_HideNoHandleNotifations.Add(NotiDefine.LoadSceneFinish);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.DoLoadScene:
                {
                    this._name = (string)notification.Body;
                    MediatorUtil.ShowMediator(MediatorDefine.SCENE_LOADER);
                    break;
                }
            case NotiDefine.LoadSceneFinish:
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