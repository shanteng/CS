using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.UIElements;
using UnityEngine.UI;
using Toggle = UnityEngine.UI.Toggle;

public class BuildingManager : MonoBehaviour
{
    public Camera _uiCamera;
    public GameObject _RangeObj;
    public int _Money = 500;//当前剩余金额

    public ToggleGroup _group;
    public List<Toggle> _toggleList;
    public Text _moneyTxt;
    private Animator _moneyAni;

    public List<TurretData> _TurretDataList;

    private TurretData _selectData = null;

    private static BuildingManager instance;
    private Vector3 _radiusPos = new Vector3(0, 1, 0);

    private MapCube _clickedMapCube;

    private Animator _upgradeAnimator;
    public Transform _UpgradeCanvas;
    public UnityEngine.UI.Button _btnUpgrade;
    public UnityEngine.UI.Button _btnDestory;
    public static BuildingManager GetInstance()
    {
        return instance;
    }
    void Awake()
    {
        this._upgradeAnimator = this._UpgradeCanvas.GetComponent<Animator>();
        this._RangeObj.SetActive(false);
        instance = this;
        this._moneyAni = this._moneyTxt.gameObject.GetComponent<Animator>();
        for (int i = 0; i < _toggleList.Count; i++)
        {
            Toggle toggle = _toggleList[i];
            toggle.isOn = false;
            toggle.onValueChanged.AddListener(OnTabChange);
        }//end for
        this.ChangeMoney(0);
        this._btnUpgrade.onClick.AddListener(this.OnUpgradeClick);
        this._btnDestory.onClick.AddListener(this.OnDestoryClick);
        this._UpgradeCanvas.gameObject.SetActive(false);
    }

    private void OnUpgradeClick()
    {
        if (this._Money < this._selectData._costUpgrade)
        {
            this._moneyAni.SetTrigger("Money");
            return;
        }
        this.ChangeMoney(this._selectData._costUpgrade);
        if (this._clickedMapCube != null)
        {
            _clickedMapCube.UpgradeTurrent(this._selectData);
        }
        StartCoroutine(HideUpgrade());
    }

    private void OnDestoryClick()
    {
        if (this._clickedMapCube != null)
        {
            _clickedMapCube.DestoryTurret();
        }
        StartCoroutine(HideUpgrade());
    }

  
    private void ShowUpgrade(Turret turret)
    {
        this._UpgradeCanvas.gameObject.SetActive(false);
        this._UpgradeCanvas.gameObject.SetActive(true);

        Vector3 pos = Camera.main.WorldToScreenPoint(turret.transform.position);
        Vector2 uiPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(this.GetComponent<RectTransform>(), pos, _uiCamera, out uiPos))
        {
            this._UpgradeCanvas.localPosition = uiPos;
        }

        this._btnUpgrade.interactable = !turret._isMaxLevel;
    }

    IEnumerator HideUpgrade()
    {
        this._upgradeAnimator.SetTrigger("Hide");
        yield return new  WaitForSeconds(0.5f);
        this._UpgradeCanvas.gameObject.SetActive(false);
        this._clickedMapCube = null;
    }


    public void ShowRange(Transform parentTran,float radius)
    {
        this._RangeObj.transform.SetParent(parentTran);
        this._RangeObj.SetActive(true);
        this._RangeObj.transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
        this._RangeObj.transform.localPosition = _radiusPos; 
    }

    public void HideRange()
    {
        this._RangeObj.SetActive(false);
        this._RangeObj.transform.SetParent(this.transform);
    }



    private void ChangeMoney(int change = 0)
    {
        this._Money -= change;
        this._moneyTxt.text = "$" + this._Money.ToString();
    }

    private void OnTabChange(bool isSelect)
    {
        if (isSelect == false)
            return;
        int count = this._toggleList.Count;
        for (int i = 0; i < count; ++i)
        {
            Toggle toggle = _toggleList[i];
            if (toggle.isOn)
            {
                this._selectData = this._TurretDataList[i];
                break;
            }
        }//end for
    }

    // Update is called once per frame
    void Update()
    {
        //射线检测Cube是否被点击
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject() == false)
        {
            this.OnClickCube();
        }
    }//end 

    private void OnClickCube()
    {
        if (this._selectData == null)
            return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool isCollider = Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("MapCube"));
        if (isCollider)
        {
          
            GameObject hitCube = hit.collider.gameObject;
            this.judgeBuild(hitCube);
        }
    }

    private void judgeBuild(GameObject hitCube)
    {
        MapCube script = hitCube.GetComponent<MapCube>();
        if (script != null && script.HasTurret() == false)
        {
            //创建炮台
            this.CreateTurret(script);
        }
        else
        {
            //升级或者拆除炮台
            if (this._clickedMapCube == null || this._clickedMapCube.gameObject != hitCube.gameObject)
            {
                this._clickedMapCube = script;
                this.ShowUpgrade(_clickedMapCube.getCurrentTurret());
            }
            else if (this._clickedMapCube != null && this._clickedMapCube.gameObject == hitCube.gameObject)
            {
                StartCoroutine(HideUpgrade());
            }
        }
        
    }

    private void CreateTurret(MapCube scrpite)
    {
        if (this._Money < this._selectData._cost)
        {
            this._moneyAni.SetTrigger("Money");
            return;
        }
        this.ChangeMoney(this._selectData._cost);
        scrpite.BuildTurret(this._selectData);
    }

}//end class
