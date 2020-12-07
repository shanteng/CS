
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
using UnityEngine.EventSystems;

public enum WindowLayer
{
    Main = 0,
    FullScreen,
    Window,
    Popup,
    Tips,
    Sdk,
    Count

}
public class UIRoot : MonoBehaviour
{
    [HideInInspector]
    public Camera camera;
    public EventSystem _event;
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

    void LateUpdate()
    {
        MouseState.instance.update();
        if (MouseState.instance.isMouseUpRightNow())
        {
            //屏幕点击了
            if (PopupFactory.Instance.ClickHideWin != null)
            {
                var isInRange = IsMouseInGameObjectRange(PopupFactory.Instance.ClickHideWin);
                if(isInRange == false)
                    PopupFactory.Instance.HideSingle();
            }
        }
    }

    public bool IsMouseInGameObjectRange(GameObject obj)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        Vector2 size = rect.sizeDelta;
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, this.camera, out pos))
        {
            float xBorder = size.x / 2;
            float yBorder = size.y / 2;
            return pos.x >= -xBorder && pos.x <= xBorder && pos.y >= -yBorder && pos.y <= yBorder;
        }
        return false;
    }

    public void SetHomeSceneEnable(bool isEnable)
    {
      //  Camera.main.enabled = isEnable;
        ViewControllerLocal.GetInstance().gameObject.GetComponent<ViewControllerLocal>().enabled = isEnable;
        ViewControllerLocal.GetInstance().gameObject.GetComponent<PhysicsRaycaster>().enabled = isEnable;
        ViewControllerLocal.GetInstance().gameObject.GetComponent<ViewControllerLocal>().SetDoUpdateDrag(false);
        this.GetLayer(WindowLayer.Main).gameObject.SetActive(isEnable);
    }

    public GameObject InstantiateUIInCenter(GameObject obj, WindowLayer layer, bool NeedAnchor = true,bool NeedZDepth = true)
    {
        Transform parent = UIRoot.Intance.GetLayer(layer);
        GameObject view = GameObject.Instantiate(obj, parent, false);
        this.ShowUIInCenter(view, NeedAnchor, NeedZDepth);
        return view;
    }

    //pivot 0.5/0.5 
    public void AdjustUIInMouseInputPos(GameObject ui,WindowLayer layer)
    {
        var rectForm = ui.GetComponent<RectTransform>();
        Vector2 size = rectForm.sizeDelta;

        Vector3 clickpos = Vector3.zero;
#if UNITY_EDITOR
        clickpos = Input.mousePosition;
#else
        if (Input.touchCount > 0)
            clickpos = Input.GetTouch(0).position;
#endif

        RectTransform layerRect = this.GetLayer(layer).GetComponent<RectTransform>();

        Vector2 uiPos;
       RectTransformUtility.ScreenPointToLocalPointInRectangle(layerRect, clickpos, UIRoot.Intance.camera, out uiPos);

        float borderX = Screen.width / 2;
        float borderY = Screen.height / 2;

        float halfSizeX = size.x / 2;
        float halfSizeY = size.y / 2;

        uiPos.x += halfSizeX;
        uiPos.y += halfSizeY;

        float borderOffset = 10;//多偏移10像素，边界的时候


        if (uiPos.x + halfSizeX > borderX)
        {
            uiPos.x = borderX - halfSizeX- borderOffset;
        }
        else if (uiPos.x - halfSizeX < -borderX)
        {
            uiPos.x = borderX + halfSizeX+ borderOffset;
        }

        if (uiPos.y + halfSizeY > borderY)
        {
            uiPos.y = borderY - halfSizeY- borderOffset;
        }
        else if (uiPos.y - halfSizeY < -borderY)
        {
            uiPos.y = borderY + halfSizeY+ borderOffset;
        }
        rectForm.anchoredPosition = uiPos;
    }

    public void ShowUIInCenter(GameObject ui,bool setAnchorCenter,bool NeedZDepth = false)
    {
        var rectForm = ui.GetComponent<RectTransform>();
        if (setAnchorCenter)
        {
            var offmini = rectForm.offsetMin;
            var offmax = rectForm.offsetMax;
            rectForm.offsetMax = Vector2.zero;
            rectForm.offsetMin = Vector2.zero;
        }
       
        rectForm.localScale = Vector3.one;

        if (NeedZDepth)
        {
            rectForm.localPosition = new Vector3(0, 0, -1000);
        }
        else
        {
            rectForm.localPosition = Vector3.zero;
        }

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