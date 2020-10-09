
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;

public class HomeLandMediator : BaseNoWindowMediator
{
    HomeLandManager _LandManager;
    private bool _isHomeLoaded = false;
    public HomeLandMediator() : base(MediatorDefine.HOME_LAND)
    {
    }

    protected override void InitListNotificationInterestsInner()
    {
        m_lInterestNotifications.Add(NotiDefine.LoadSceneFinish);
        m_lInterestNotifications.Add(NotiDefine.CreateOneBuildingResp);
        m_lInterestNotifications.Add(NotiDefine.BuildingRelocateResp);
        m_lInterestNotifications.Add(NotiDefine.BuildingStatusChanged);
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.LoadSceneFinish:
                {
                    string name = (string)notification.Body;
                    if (name.Equals(SceneDefine.Home))
                    {
                        this._isHomeLoaded = true;
                        this.InitScene();
                    }
                    break;
                }
            case NotiDefine.CreateOneBuildingResp:
                {
                    BuildingData data = (BuildingData)notification.Body;
                    this._LandManager.OnCreateResp(data);
                    break;
                }
            case NotiDefine.BuildingRelocateResp:
                {
                    string key = (string)notification.Body;
                    this._LandManager.OnRelocateResp(key);
                    break;
                }
            case NotiDefine.BuildingStatusChanged:
                {
                    string key = (string)notification.Body;
                    this._LandManager.OnBuildingStateChanged(key);
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