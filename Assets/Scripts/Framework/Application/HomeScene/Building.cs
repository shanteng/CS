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


    private float CosDegreeValue;
    private List<MeshRenderer> _allRenders;
    private BuildingConfig _config;
    private bool _isSelect = false;
   

    void Start()
    {
        _allRenders = new List<MeshRenderer>();
        int count = this.transform.childCount;
        for (int i = 0; i < count; ++i)
        {
            MeshRenderer render = this.transform.GetChild(i).GetComponent<MeshRenderer>();
            if (render == null)
                continue;
            _allRenders.Add(render);
        }
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
        ViewControllerLocal.GetInstance().PressSpot(this.gameObject.name);
        _screenSpace = Camera.main.WorldToScreenPoint(this.transform.position);
        _beginPos = this.transform.position;
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
        curPosition.y = 1;

      


        curPosition.x = Mathf.RoundToInt(curPosition.x);
        curPosition.z = Mathf.RoundToInt(curPosition.z);


        int maxX = HomeLandManager.ROW_COUNT - this._config.RowCount;
        int maxZ = HomeLandManager.COL_COUNT - this._config.ColCount;

        int newX = Mathf.Clamp((int)curPosition.x, 0, maxX);
        int newZ = Mathf.Clamp((int)curPosition.z, 0, maxZ);

        Debug.LogError("newX--------:" + newX);
        Debug.LogError("newZ--------:" + newZ);


        this.transform.position = new Vector3(newX,1, newZ);
    
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (this._isSelect == false)
            return;
        //结束拖拽，重新设置位置
        this.SetSelect(false);
        ViewControllerLocal.GetInstance().PressSpot("");
        ViewControllerLocal.GetInstance().SetSelectSpot("");
        this.SetCordinate((int)this.transform.position.x, (int)this.transform.position.z);
        //通知LandManager
        MediatorUtil.SendNotification(NotiDefine.BUILDING_POSTION_CHANGE, this);
    }

    //监听点击
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.LogError("OnPointerClick" + this.gameObject.name);
        ViewControllerLocal.GetInstance().SetSelectSpot(this.gameObject.name);
        this.SetSelect(true);
    }

    private void SetSelect(bool isSelect)
    {
        this._isSelect = isSelect;
        foreach (MeshRenderer render in this._allRenders)
        {
            render.material.color = isSelect ? Color.yellow : Color.white;
        }
    }
}
