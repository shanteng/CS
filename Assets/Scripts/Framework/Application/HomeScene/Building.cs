using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Building : MonoBehaviour
    , IPointerClickHandler
    ,IDragHandler
    ,IBeginDragHandler
    ,IEndDragHandler
{
    public int _configid;//建筑对应的配置文件
    public string _key = "";
    public Vector3Int _cordinate = Vector3Int.zero;//左下角的坐标
    public List<Vector2Int> _occupyCordinates;//占领的全部地块坐标
    public PlaneBase _basePlane;

    [HideInInspector]
    public BuildingConfig _config;

    private float CosDegreeValue;
    private bool _isSelect = false;
    private ColorFlash _flash;
    private bool _isDrag = false;
 

    void Start()
    {
        this._flash = this.GetComponent<ColorFlash>();
        this._flash.Stop();
        _basePlane.gameObject.SetActive(false);
        CosDegreeValue = Mathf.Cos(HomeLandManager.Degree * Mathf.Deg2Rad);
    }

    public void Init()
    {
        this._config = BuildingConfig.Instance.GetData(this._configid);
        this._occupyCordinates = new List<Vector2Int>();
    }
    
    public void SetCordinate(int x, int z)
    {
        this._cordinate.x = x;
        this._cordinate.z = z;
        this._occupyCordinates.Clear();
        for (int row = 0; row < this._config.RowCount; ++row)
        {
            int curX = this._cordinate.x + row;
            for (int col = 0; col < this._config.ColCount; ++col)
            {
                int curZ = this._cordinate.z + col;
                Vector2Int corNow = new Vector2Int(curX, curZ);
                this._occupyCordinates.Add(corNow);
            }
        }
    }

    private Vector3 _beginPos;
    private Vector3 _screenSpace;
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (this._isSelect == false)
            return;
        this._isDrag = true;
        ViewControllerLocal.GetInstance().PressSpot(this._key);
        _screenSpace = Camera.main.WorldToScreenPoint(this.transform.position);
        _beginPos = this.transform.position;
        this._basePlane.gameObject.SetActive(true);
        this._basePlane.SetColorIndex(1);//正常颜色
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
      

        curPosition.x = Mathf.RoundToInt(curPosition.x);
        curPosition.z = Mathf.RoundToInt(curPosition.z);


        int maxX = HomeLandManager.ROW_COUNT - this._config.RowCount;
        int maxZ = HomeLandManager.COL_COUNT - this._config.ColCount;

        int newX = Mathf.Clamp((int)curPosition.x, 0, maxX);
        int newZ = Mathf.Clamp((int)curPosition.z, 0, maxZ);

        this.transform.position = new Vector3(newX,1.02f, newZ);
        bool canBuildHere = HomeLandManager.GetInstance().canBuildInSpot(this._key, newX, newZ,this._config.RowCount,this._config.ColCount);
        int index = canBuildHere ? 1 : 0;
        this._basePlane.SetColorIndex(index);

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (this._isSelect == false)
            return;

        this._isDrag = false;
        bool canBuildHere = HomeLandManager.GetInstance().canBuildInSpot(this._key, (int)this.transform.position.x, (int)this.transform.position.z, this._config.RowCount, this._config.ColCount);
        if (canBuildHere)
        {
            //结束拖拽，重新设置位置
            HomeLandManager.GetInstance().UnSelectOtherBuilding(this._key);//拖拽结束，取消勾选
            this.SetSelect(false);
            this._basePlane.gameObject.SetActive(false);
            ViewControllerLocal.GetInstance().PressSpot("");
            ViewControllerLocal.GetInstance().SetSelectSpot("");
            this.transform.position = new Vector3(this.transform.position.x, 1, this.transform.position.z);//恢复层级
            this.SetCordinate((int)this.transform.position.x, (int)this.transform.position.z);
            //通知LandManager
            HomeLandManager.GetInstance().RecordBuildOccupy(this._key,this._occupyCordinates);
            //MediatorUtil.SendNotification(NotiDefine.BUILDING_POSTION_CHANGE, this);
        }
    }

    //监听点击
    public void OnPointerClick(PointerEventData eventData)
    {
        if (this._isDrag)
            return;
        HomeLandManager.GetInstance().UnSelectOtherBuilding(this._key);
        this.SetSelect(!this._isSelect);
        ViewControllerLocal.GetInstance().SetSelectSpot(this._key);
    }

    public void SetSelect(bool isSelect)
    {
        this._isSelect = isSelect;
        this._flash.DoFlash(this._isSelect);
    }
}
