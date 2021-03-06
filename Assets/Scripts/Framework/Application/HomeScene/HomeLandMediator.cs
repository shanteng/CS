﻿
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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
        m_lInterestNotifications.Add(NotiDefine.GAME_RESET);

        m_lInterestNotifications.Add(NotiDefine.WINDOW_HAS_SHOW);

        m_lInterestNotifications.Add(NotiDefine.CreateOneBuildingResp);
        m_lInterestNotifications.Add(NotiDefine.BuildingRelocateResp);
        m_lInterestNotifications.Add(NotiDefine.BuildingStatusChanged);
        m_lInterestNotifications.Add(NotiDefine.ConfirmBuild);
        m_lInterestNotifications.Add(NotiDefine.BuildingRemoveNoti);
        m_lInterestNotifications.Add(NotiDefine.TryBuildBuilding);
        m_lInterestNotifications.Add(NotiDefine.HomeRangeChanged);
        m_lInterestNotifications.Add(NotiDefine.LandVisibleChanged);


        m_lInterestNotifications.Add(NotiDefine.AcceptHourAwardResp);
        m_lInterestNotifications.Add(NotiDefine.GO_TO_SELEC_BUILDING_BY_ID);

        m_lInterestNotifications.Add(NotiDefine.PathAddNoti);
        m_lInterestNotifications.Add(NotiDefine.PathRemoveNoti);
        m_lInterestNotifications.Add(NotiDefine.NewCitysVisbleNoti);
        m_lInterestNotifications.Add(NotiDefine.DoOwnCityResp);
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.GAME_RESET:
                {
                    this._isHomeLoaded = false;
                    GameIndex.InWorld = false;
                    break;
                }
            case NotiDefine.LoadSceneFinish:
                {
                    string name = (string)notification.Body;
                    if (name.Equals(SceneDefine.Home))
                    {
                        this.InitScene();
                    }
                    else
                    {
                        this._isHomeLoaded = false;
                    }
                    break;
                }
            case NotiDefine.LandVisibleChanged:
                {
                    if (this._isHomeLoaded)
                    {
                        Dictionary<string, VInt2> pos = (Dictionary<string, VInt2>)notification.Body;
                        this._LandManager.GenerateVisibleSpot(pos);
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
                    if (this._isHomeLoaded)
                    {
                        string key = (string)notification.Body;
                        this._LandManager.OnBuildingStateChanged(key);
                    }
                    break;
                }
            case NotiDefine.HomeRangeChanged:
                {
                    if (this._isHomeLoaded)
                    {
                        int city = (int)notification.Body;
                        if (city == 0)
                            this._LandManager.SetMainCityRange();
                    }
                    break;
                }
            case NotiDefine.ConfirmBuild:
                {
                    bool isconfirm = (bool)notification.Body;
                    this._LandManager.ConfirmBuild(isconfirm);
                    break;
                }
            case NotiDefine.BuildingRemoveNoti:
                {
                    if (this._isHomeLoaded)
                    {
                        string key = (string)notification.Body;
                        this._LandManager.OnRemoveBuild(key);
                    }
                    break;
                }
            case NotiDefine.TryBuildBuilding:
                {
                    int id = (int)notification.Body;
                    this._LandManager.BuildInScreenCenterPos(id);
                    break;
                }
            case NotiDefine.AcceptHourAwardResp:
                {
                    if (this._isHomeLoaded)
                    {
                        this._LandManager.UpdateIncome();
                    }
                    break;
                }
            case NotiDefine.GO_TO_SELEC_BUILDING_BY_ID:
                {
                    if (this._isHomeLoaded)
                    {
                        int id = (int)notification.Body;
                        this._LandManager.GotoSelectBuildingBy(id);
                    }
                    break;
                }
            case NotiDefine.WINDOW_HAS_SHOW:
                {
                    if (this._isHomeLoaded)
                    {
                        this._LandManager.SetCurrentSelectBuilding("");
                        this._LandManager.HideSelectSpot();
                    }
                    break;
                }
            case NotiDefine.PathAddNoti:
                {
                    if (this._isHomeLoaded)
                    {
                        PathData data = (PathData)notification.Body;
                        this._LandManager.AddOnePath(data);
                    }
                    break;
                }
            case NotiDefine.PathRemoveNoti:
                {
                    if (this._isHomeLoaded)
                    {
                        string ids = (string)notification.Body;
                        this._LandManager.RemoveOnePath(ids);
                    }
                    break;
                }
            case NotiDefine.NewCitysVisbleNoti:
                {
                    if (this._isHomeLoaded)
                    {
                        List<int> ids = (List<int>)notification.Body;
                        this._LandManager.NewCitysVisible(ids);
                    }
                    break;
                }
            case NotiDefine.DoOwnCityResp:
                {
                    if (this._isHomeLoaded)
                    {
                        int cityid= (int)notification.Body;
                        this._LandManager.SetCityOwn(cityid);
                    }
                    break;
                }
        }
    }//end 

  

    void InitScene()
    {
        GameObject[] allObj = SceneManager.GetSceneByName(SceneDefine.Home).GetRootGameObjects();
        _LandManager = null;
        foreach (GameObject obj in allObj)
        {
            if (obj.name.Equals("LandManager"))
            {
                _LandManager = obj.GetComponent<HomeLandManager>();
                break;
            }
        }

        MediatorUtil.ShowMediator(MediatorDefine.MAIN);
        if (this._LandManager != null)
            this._LandManager.InitScene();

        if (GameIndex.InBattle)
        {
            //从战斗场景退出的
            if (BattleProxy._instance.Data.Type == BattleType.AttackCity)
            {
                int TargetCityID = (int)BattleProxy._instance.Data.Param;
                VInt2 cityPost = WorldProxy._instance.GetCityCordinate(TargetCityID);
                ViewControllerLocal.GetInstance().DirectGoTo(cityPost);
            }
        }

        this._isHomeLoaded = true;
        GameIndex.InWorld = true;
        GameIndex.InBattle = false;
    }
}//end class