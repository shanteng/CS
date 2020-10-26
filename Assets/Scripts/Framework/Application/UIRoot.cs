
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
public class UIRoot : MonoBehaviour
{
    [HideInInspector]
    public Camera camera;
    private List<Transform> _windowLayers;
    private AudioSource _audioSource;
    public static string CurFullWindow = string.Empty;

    public Material _UIGray;

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

    void Start()
    {
        GameObject obj = ResourcesManager.Instance.LoadUIRes("SdkView");
        this.InstantiateUIInCenter(obj,WindowLayer.Sdk);
    }

    public GameObject InstantiateUIInCenter(GameObject obj, WindowLayer layer)
    {
        Transform parent = UIRoot.Intance.GetLayer(layer);
        GameObject view = GameObject.Instantiate(obj, parent, false);
        this.ShowUIInCenter(view, WindowLayer.Sdk, true);
        return view;
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
}