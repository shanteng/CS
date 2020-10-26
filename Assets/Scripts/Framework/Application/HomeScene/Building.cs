using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Building : MonoBehaviour
    , IPointerClickHandler
    , IDragHandler
    , IBeginDragHandler
    , IEndDragHandler
{
    public static float drag_offsety = 0.02f;
    public BuildingData _data;//从Proxy取到的引用
    [HideInInspector]
    public PlaneBase _flashBase;
    private Transform _occupyBase;
    private BoxCollider _collider;

    private Transform _BuildingTran; 
    private ColorFlash _flash;
    private CountDownCanvas _cdUI;
    private BuildingUI _Ui;

    private float CosDegreeValue;
    private bool _isSelect = false;
    private bool _isDrag = false;
    private string _AddType;
    private bool _AddIconShow = false;
    private string _AddAttr = "";

    public List<GameObject> _levelParts;
    private int _offsetDrag;

    private Dictionary<string, object> vo = new Dictionary<string, object>();

    void Awake()
    {
        _collider = this.transform.GetComponent<BoxCollider>();
        _flashBase = this.transform.Find("FlashBase").GetComponent<PlaneBase>();
        _occupyBase = this.transform.Find("OccupyBase");
        _BuildingTran = this.transform.Find("Building");
        this._flash = _BuildingTran.GetComponent<ColorFlash>();
        this._flash.SetOrignal();
        _flashBase.gameObject.SetActive(false);
        CosDegreeValue = Mathf.Cos(HomeLandManager.Degree * Mathf.Deg2Rad);
    }

    public void SetRowCol(int row, int col)
    {
        this._flashBase.SetSize(row, col);
        this._occupyBase.localScale = new Vector3(row, col, 1);
        float x = (float)(row - 1) / 2f;
        float z = (float)(col - 1) / 2f;
        Vector3 pos = new Vector3(x, 0.51f, z);
        Vector3 pos2 = new Vector3(x, 0.512f, z);
        this._occupyBase.transform.localPosition = pos;
        this._flashBase.transform.localPosition = pos2;
        this._offsetDrag = row - 1;
    }

    public void DoTransparent()
    {
        this._flash.DoTransparent();
    }

    public void CreateUI(BuildingUI prefabs,int id)
    {
        this._Ui = GameObject.Instantiate<BuildingUI>(prefabs, Vector3.zero, Quaternion.identity, this.transform);
        BuildingConfig config = BuildingConfig.Instance.GetData(id);
        this._AddType = config.AddType;
   
        this._Ui.SetUI(config);
        int offset = config.RowCount - 2;
        _Ui.transform.localPosition = new Vector3(0, 0, offset);
        //_Ui._CdUi.transform.Translate(new Vector3(0, -offset, 0), Space.Self);
        this._Ui._NameTxt.text = config.Name;
       
    }

    public void UpdateIncome()
    {
        if (this._AddType.Equals(ValueAddType.HourTax) && this._Ui != null)
        {
            BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(this._data._id, this._data._level);
            if (configLv != null)
            {
                CostData add = new CostData();
                add.Init(configLv.AddValues[0]);
                _AddIconShow =  this._Ui.UpdateIncome(add.id);
                this._AddAttr = add.id;
            }
        }
    }

    public void SetCurrentState()
    {
        if (this._data._status == BuildingData.BuildingStatus.INIT)
        {
            
        }
        else if (this._data._status == BuildingData.BuildingStatus.BUILD)
        {
            this._Ui.DoCountDown(_data._expireTime, this._data._UpgradeSecs);
        }
        else if (this._data._status == BuildingData.BuildingStatus.UPGRADE)
        {
            this._Ui.DoCountDown(_data._expireTime,this._data._UpgradeSecs);
        }
        else if (this._data._status == BuildingData.BuildingStatus.NORMAL)
        {
            this._Ui.HideCD();
        }

        this.UpdateIncome();

        
        //设置显示parts
        int level = this._data._level > 0 ? this._data._level : 1;
        BuildingUpgradeConfig configLevel = BuildingUpgradeConfig.GetConfig(this._data._id, level);
        int length = configLevel.Parts.Length;
        int count = this._levelParts.Count;
        for (int i = 0; i < count; ++i)
        {
            int partname = int.Parse(this._levelParts[i].name);
            bool isshow = configLevel.Parts.Contains(partname);
            this._levelParts[i].SetActive(isshow);
        }
    }

    public void RelocateToProxy(int x, int z)
    {
        vo.Clear();
        vo["key"] = this._data._key;
        vo["x"] = x;
        vo["z"] = z;
        MediatorUtil.SendNotification(NotiDefine.BuildingRelocateDo, vo);
    }

    private Vector3 _beginPos;
    private Vector3 _screenSpace;
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (this._isSelect == false)
            return;
        HomeLandManager.GetInstance().HideInfoCanvas();
        this._isDrag = true;
        HomeLandManager.GetInstance().SetDraging(true);
        HomeLandManager.GetInstance().SetQuadVisible(true);
        _screenSpace = Camera.main.WorldToScreenPoint(this.transform.position);
        _beginPos = this.transform.position;
        this._flashBase.gameObject.SetActive(true);
        this.SetCanDoState((int)this.transform.position.x, (int)this.transform.position.z);
    }

    public void SetCanDoState(int newX, int newZ)
    {
        BuildingConfig _config = BuildingConfig.Instance.GetData(this._data._id);
        bool canBuildHere = HomeLandManager.GetInstance().canBuildInSpot(this._data._key, newX, newZ, _config.RowCount, _config.ColCount);
        int index = canBuildHere ? 1 : 0;
        this._flashBase.SetColorIndex(index);

        if (this._data._status == BuildingData.BuildingStatus.INIT)
            HomeLandManager.GetInstance().SetBuildCanvasState(canBuildHere);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (this._isSelect == false)
            return;
        Vector3 curScreenSpace = new Vector3(eventData.position.x, eventData.position.y, _screenSpace.z);
        var curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace);
        Vector3 offset = curPosition - _beginPos;
        float yoffset = offset.y * this.CosDegreeValue;
        curPosition.x -= yoffset;
        curPosition.z += yoffset;
        //curPosition.x -= 2.5f;
        curPosition.z -= this._offsetDrag;
        curPosition.x = Mathf.RoundToInt(curPosition.x);
        curPosition.z = Mathf.RoundToInt(curPosition.z);

        this.transform.position = new Vector3(curPosition.x, drag_offsety, curPosition.z);
        this.SetCanDoState((int)curPosition.x, (int)curPosition.z);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (this._isSelect == false)
            return;

        this._isDrag = false;
        HomeLandManager.GetInstance().SetDraging(false);
        BuildingConfig _config = BuildingConfig.Instance.GetData(this._data._id);
        bool canBuildHere = HomeLandManager.GetInstance().canBuildInSpot(this._data._key, (int)this.transform.position.x, (int)this.transform.position.z, _config.RowCount, _config.ColCount);
        if (canBuildHere && this._data._status != BuildingData.BuildingStatus.INIT)
        {
            //通知Proxy改变位置
            this.RelocateToProxy((int)this.transform.position.x, (int)this.transform.position.z);
            HomeLandManager.GetInstance().SetQuadVisible(false);
        }
    }

    private void EndRelocate()
    {
        //结束拖拽，重新设置位置
        this._flashBase.gameObject.SetActive(false);
        this.transform.position = new Vector3(this._data._cordinate.x, 0, this._data._cordinate.y);//恢复层级
        HomeLandManager.GetInstance().SetQuadVisible(false);
    }

    public void SetSelect(bool isSelect)
    {
        this._isSelect = isSelect;
        this._flash.DoFlash(this._isSelect);
      
        if (isSelect == false)
            this.EndRelocate();
    }

    //监听点击
    public void OnPointerClick(PointerEventData eventData)
    {
        //拖拽或者当前正在建造升级中不允许点击
        if (this._isDrag || this._data._status == BuildingData.BuildingStatus.INIT)
            return;

        if (HomeLandManager.GetInstance().isTryBuild)
            return;


        if (this._AddIconShow)
        {
            BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(this._data._id, this._data._level);
            MediatorUtil.SendNotification(NotiDefine.AcceptHourAwardDo, this._AddAttr);
            return;
        }

        if (this._isSelect)
        {
            //取消选中，并且返回初始位置
            HomeLandManager.GetInstance().SetCurrentSelectBuilding("");
            return;
        }

        //选中当前地块
        HomeLandManager.GetInstance().SetCurrentSelectBuilding(this._data._key);
       
    }

   
}
