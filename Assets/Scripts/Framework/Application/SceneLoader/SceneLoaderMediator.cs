
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;

public class SceneLoaderMediator : BaseFullScreenWindowMediator<SceneLoaderView>
{
    private string _name;
    private bool _isAddtive = false;
    public SceneLoaderMediator() : base(MediatorDefine.SCENE_LOADER)
    {
       
    }

   
    protected override void InitListNotificationInterestsInner()
    {
        m_lInterestNotifications.Add(NotiDefine.DoLoadScene);
        m_lInterestNotifications.Add(NotiDefine.DoLoadSceneAddtive);
        m_HideNoHandleNotifations.Add(NotiDefine.LoadSceneFinish);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.DoLoadScene:
                {
                    this._isAddtive = false;
                    this._name = (string)notification.Body;

                    MediatorUtil.ShowMediator(MediatorDefine.SCENE_LOADER);
                    break;
                }
            case NotiDefine.DoLoadSceneAddtive:
                {
                    this._isAddtive = true;
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
        this.m_view.LoadScene(this._name,this._isAddtive);
    }
}//end class