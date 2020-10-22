
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

public enum WindowLayer
{
    Main = 0,
    FullScreen,
    Window,
    Popup,
    Tips,
    Sdk

}
public class UIRoot : MonoBehaviour, ISyncCallback
{
    [HideInInspector]
    public Camera camera;
    private List<Transform> _windowLayers;
    private AudioSource _audioSource;
    public static string CurFullWindow = string.Empty;

    public GameObject _SdkView;
    public UIScreenHideHandler _SdkClickHandler;

    public PlayerIdentityCore _sdkCore;
    public MainController _sdkController;
    public Material _UIGray;

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

    public static UIRoot Intance { get; private set; }
    void Awake()
    {
        this.camera = this.transform.parent.GetComponent<Camera>();
        Intance = this;
        this._audioSource = this.gameObject.AddComponent<AudioSource>();
        this._windowLayers = new List<Transform>();
        int count = this.transform.childCount;
        for (int i = 0; i < count; ++i)
        {
            Transform layer = this.transform.GetChild(i);
            layer.gameObject.SetActive(true);
            this._windowLayers.Add(layer);
        }
    }

    private void Start()
    {
        this._SdkView.SetActive(true);
        this._SdkClickHandler.AddListener(OnClickSdkBg);
    }

    public void ShowUIInCenter(GameObject ui, WindowLayer layer,bool setAnchorCenter)
    {
        Transform parent = this.GetLayer(layer);
        var rectForm = ui.GetComponent<RectTransform>();
        rectForm.SetParent(parent.transform);

        if (setAnchorCenter)
        {
            var offmini = rectForm.offsetMin;
            var offmax = rectForm.offsetMax;
            rectForm.offsetMax = offmax;
            rectForm.offsetMin = offmini;
        }
       

        rectForm.localScale = Vector3.one;
        rectForm.localPosition = Vector3.zero;

        ui.SetActive(true);
    }

    private void OnClickSdkBg()
    {
        if (GameIndex.InGame)
            this._SdkView.SetActive(false);
    }

    public Transform GetLayer(WindowLayer layer)
    {
        return this._windowLayers[(int)layer];
    }

    public void SetImageGray(Image img,bool isGray)
    {
        if (isGray == false)
            img.material = null;
        else
        {
            Material me = new Material(this._UIGray);
            img.material = me;
        }
    }

    public void OnLogin()
    {
        this._SdkView.gameObject.SetActive(false);
        MediatorUtil.ShowMediator(MediatorDefine.LOGIN);
        GameIndex.UID = _sdkCore.userInfo.userId;

        CloudSaveInitializer.AttachToGameObject(this.gameObject);
        characterInfo = CloudSave.OpenOrCreateDataset("CharacterInfo");
        characterInfo.SynchronizeOnConnectivityAsync(this);
    }

    public void OnLogout()
    {
        //返回登录
        MediatorUtil.SendNotification(NotiDefine.GAME_RESET);
        GameObject.Destroy(this.camera.gameObject);
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