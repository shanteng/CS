
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;

public class HomeSceneMediator : BaseNoWindowMediator
{
    HomeLandManager _LandManager;
    private bool _isHomeLoaded = false;
    public HomeSceneMediator() : base(MediatorDefine.HOME_SCENE)
    {
    }

    protected override void InitListNotificationInterestsInner()
    {
        m_lInterestNotifications.Add(NotiDefine.LOAD_SCENE_FINISH);
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.LOAD_SCENE_FINISH:
                {
                    string name = (string)notification.Body;
                    if (name.Equals(SceneDefine.Home))
                    {
                        this._isHomeLoaded = true;
                        this.InitScene();
                    }
                    break;
                }
        }
    }//end 

    void InitScene()
    {
        GameObject[] allObj = SceneManager.GetActiveScene().GetRootGameObjects();
        _LandManager = null;
        foreach (GameObject obj in allObj)
        {
            if (obj.name.Equals("LandManager"))
            {
                _LandManager = obj.GetComponent<HomeLandManager>();
                break;
            }
        }

        if (this._LandManager != null)
            this._LandManager.InitScene();
    }
}//end class