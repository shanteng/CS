
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.PlayerIdentity;
using UnityEngine.PlayerIdentity.UI;
using UnityEngine.PlayerIdentity.Management;
using UnityEngine.CloudSave;
using Debug = UnityEngine.Debug;
using UnityEngine.SceneManagement;


public class SdkView : MonoBehaviour
    , ISyncCallback
{
    public PlayerIdentityCore _sdkCore;

    private UIScreenHideHandler _SdkClickHandler;
    private PanelController _panelController;
    private SignInPanel _signPanel;
    private AccountPanel _accoutPanel;

    //云存储
    public IDataset characterInfo { get; private set; }
    private CloudSave _cloudSave;
    public CloudSave CloudSave
    {
        get
        {
            if (_cloudSave == null)
            {
                _cloudSave = new CloudSave(PlayerIdentityManager.Current);
            }
            return _cloudSave;
        }
       
    }

    public static SdkView Intance { get; private set; }
    void Awake()
    {
        this._SdkClickHandler = this.transform.Find("ClickHide").GetComponent<UIScreenHideHandler>();
        this._panelController = this.transform.Find("UserAuthPrefab/Main Canvas/Panels").GetComponent<PanelController>();
        this._signPanel = this.transform.Find("UserAuthPrefab/Main Canvas/Panels/Login/Sign In Panel").GetComponent<SignInPanel>();
        this._accoutPanel = this.transform.Find("UserAuthPrefab/Main Canvas/Panels/Account/Account Panel").GetComponent<AccountPanel>();
        Intance = this;
    }

    private void Start()
    {
        this._SdkClickHandler.AddListener(HideSdk);
        this.ShowSdk();
    }

    public void HideSdk()
    {
      //  if (GameIndex.InGame)
      this.gameObject.SetActive(false);
    }

    public void ShowSdk()
    {
        this.gameObject.SetActive(true);
        if (GameIndex.UID.Equals("") == false)
        {
            this._panelController.OpenPanel(this._accoutPanel);
        }
        else
        {
            this._panelController.OpenPanel(this._signPanel);
        }
    }


    public void OnLogin()
    {
        this.HideSdk();
        MediatorUtil.ShowMediator(MediatorDefine.LOGIN);
        GameIndex.UID = PlayerIdentityManager.Current.userId;

        CloudSaveInitializer.AttachToGameObject(this.gameObject);
        characterInfo = CloudSave.OpenOrCreateDataset("CharacterInfo");
        characterInfo.SynchronizeOnConnectivityAsync(this);
    }

    public void OnLogout()
    {
        //返回登录
        GameIndex.UID = "";
        MediatorUtil.SendNotification(NotiDefine.GAME_RESET);
        GameObject.Destroy(this.GetComponent<Camera>().gameObject);
        SceneManager.LoadScene(SceneDefine.GameIndex);
    }

    public void WipeOut()
    {
       // CloudSave.WipeOut();
       List<Record> list = (List<Record>)this.characterInfo.GetAllRecords();
        int count = list.Count;
        for (int i = 0; i < count; ++i)
        {
            characterInfo.Put(list[i].Key, "");
        }
        characterInfo.SynchronizeOnConnectivityAsync(this);
    }

    public void SaveToCloud(string jsonName,string jsonStr)
    {
        characterInfo.Put(jsonName, jsonStr);
        characterInfo.SynchronizeOnConnectivityAsync(this);
    }

    public string LoadCloudData(string jsonName)
    {
        return characterInfo.Get(jsonName);
    }

    //ISyncCallback
    public bool OnConflict(IDataset dataset, IList<SyncConflict> conflicts)
    {
        return true;
    }

    public void OnError(IDataset dataset, DatasetSyncException syncEx)
    {
        Debug.Log("Sync failed for dataset : " + dataset.Name);
    }

    public void OnSuccess(IDataset dataset)
    {
        Debug.Log("Successfully synced for dataset: " + dataset.Name);
    }
}