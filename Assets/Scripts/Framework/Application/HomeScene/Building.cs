using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Building : MonoBehaviour
    , IPointerClickHandler
    , IDragHandler
    , IBeginDragHandler
    , IEndDragHandler
{
    public BuildingData _data;//从Proxy取到的引用
    public PlaneBase _basePlane;
    private ColorFlash _flash;

    private float CosDegreeValue;
    private bool _isSelect = false;
    private bool _isDrag = false;

    private Dictionary<string, object> vo = new Dictionary<string, object>();

    void Start()
    {
        this._flash = this.GetComponent<ColorFlash>();
        this._flash.Stop();
        _basePlane.gameObject.SetActive(false);
        CosDegreeValue = Mathf.Cos(HomeLandManager.Degree * Mathf.Deg2Rad);
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
        this._isDrag = true;
        ViewControllerLocal.GetInstance().PressSpot(this._data._key);
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


        int maxX = HomeLandManager.ROW_COUNT - this._data._config.RowCount;
        int maxZ = HomeLandManager.COL_COUNT - this._data._config.ColCount;

        int newX = Mathf.Clamp((int)curPosition.x, 0, maxX);
        int newZ = Mathf.Clamp((int)curPosition.z, 0, maxZ);

        this.transform.position = new Vector3(newX, 1.02f, newZ);
        bool canBuildHere = HomeLandManager.GetInstance().canBuildInSpot(this._data._key, newX, newZ, this._data._config.RowCount, this._data._config.ColCount);
        int index = canBuildHere ? 1 : 0;
        this._basePlane.SetColorIndex(index);

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (this._isSelect == false)
            return;

        this._isDrag = false;
        bool canBuildHere = HomeLandManager.GetInstance().canBuildInSpot(this._data._key, (int)this.transform.position.x, (int)this.transform.position.z, this._data._config.RowCount, this._data._config.ColCount);
        if (canBuildHere)
        {
            //通知Proxy改变位置
            this.RelocateToProxy((int)this.transform.position.x, (int)this.transform.position.z);
        }
    }

    public void OnRelocateResp()
    {
        //结束拖拽，重新设置位置
        HomeLandManager.GetInstance().UnSelectOtherBuilding(this._data._key);//拖拽结束，取消勾选
        this.SetSelect(false);
        this._basePlane.gameObject.SetActive(false);
        ViewControllerLocal.GetInstance().PressSpot("");
        ViewControllerLocal.GetInstance().SetSelectSpot("");
        this.transform.position = new Vector3(this.transform.position.x, 1, this.transform.position.z);//恢复层级
        //通知LandManager
        HomeLandManager.GetInstance().RecordBuildOccupy(this._data._key, this._data._occupyCordinates);
    }

    //监听点击
    public void OnPointerClick(PointerEventData eventData)
    {
        if (this._isDrag)
            return;
        HomeLandManager.GetInstance().UnSelectOtherBuilding(this._data._key);
        this.SetSelect(!this._isSelect);
        ViewControllerLocal.GetInstance().SetSelectSpot(this._data._key);
    }

    public void SetSelect(bool isSelect)
    {
        this._isSelect = isSelect;
        this._flash.DoFlash(this._isSelect);
    }
}
