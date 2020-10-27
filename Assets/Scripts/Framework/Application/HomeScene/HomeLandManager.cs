using Newtonsoft.Json.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BuildingData;

public class HomeLandManager : MonoBehaviour
{
    public static float Degree = 45;

    public int World = 1;
    private WorldConfig _config;

    public MeshRenderer _HomePlane;
    public MeshRenderer _QuadCanBuild;

    public MyCity _cityPrefab;
    public BuildCanvas _BuildCanvas;

    private MyCity _myLandCity;

    public bool isTryBuild => this._TryBuildScript != null;
    private Building _TryBuildScript;
    private InfoCanvas _infoCanvas;
    private string _currentBuildKey = "";

    private static HomeLandManager instance;
    public static HomeLandManager GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        this._BuildCanvas.Hide();
    }

    public string CurrentKey => this._currentBuildKey;

    public bool canBuildInSpot(string key, int posX, int posZ, int rowCount, int colCount)
    {
        for (int row = 0; row < rowCount; ++row)
        {
            int curX = posX + row;
            for (int col = 0; col < colCount; ++col)
            {
                int curZ = posZ + col;
                bool canOp = this.CanOperateSpot(curX, curZ);
                if (canOp == false)
                    return false;

                string spotKey = UtilTools.combine(curX, "|", curZ);
                bool inOther = this._myLandCity.HasOccupy(spotKey);//是否占领了别人的格子
                bool isInMySpot = _myLandCity.isInMyOldRange(curX, curZ, key);//占领的是不本身我自己的格子
                if (inOther && isInMySpot == false)
                    return false;//不可以建造
            }
        }

        return true;
    }

    public bool CanOperateSpot(int x, int z)
    {
        string key = UtilTools.combine(x, "|", z);
        return WorldProxy._instance.GetCanOperateSpots().Contains(key);
    }



    public void OnCreateResp(BuildingData data)
    {
        if (this._TryBuildScript == null)
            return;
        //创建一个
        Building building = _TryBuildScript;
        building._data = data;
        building.name = building._data._key;
        building.SetCurrentState();
        this._myLandCity.AddOneBuilding(data._key, building);
        this._myLandCity.RecordBuildOccupy(building._data._key, building._data._occupyCordinates);
        this.SetCurrentSelectBuilding(data._key);
        this._TryBuildScript = null;
    }

    public void OnRelocateResp(string key)
    {
        Building curBuild = this._myLandCity.GetBuilding(key);
        if (curBuild != null)
        {
            _myLandCity.RecordBuildOccupy(curBuild._data._key, curBuild._data._occupyCordinates);
            this.ShowBuildingInfoCanvas(curBuild);
        }
    }

    public void OnBuildingStateChanged(string key)
    {
        Building curBuild = _myLandCity.GetBuilding(key);
        if (curBuild != null)
        {
            curBuild.SetCurrentState();
            if (key.Equals(this._currentBuildKey))
                this.ShowBuildingInfoCanvas(curBuild);
        }
    }

    public void OnRemoveBuild(string key)
    {
        this.HideInfoCanvas();
        Building curBuild = _myLandCity.GetBuilding(key);
        if (curBuild != null)
        {
            this._myLandCity.ClearBuildOccupy(key);
            GameObject.Destroy(curBuild.gameObject);
            curBuild = null;
        }
    }



    public void BuildInScreenCenterPos(int id)
    {
        Vector3 posScreen = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(posScreen);//摄像机发射射线到屏幕点。
        if (Physics.Raycast(ray, out hit))//射线发出并碰撞到外壳，那么手臂就应该朝向碰撞点
        {

            int xCenter = Mathf.RoundToInt(hit.point.x);
            int zCenter = Mathf.RoundToInt(hit.point.z);
            this.TryBuild(id, xCenter, zCenter);//测试
        }
    }


    public void OnClickSpotCube(int x, int z)
    {
        if (this.isTryBuild)
            return;
        this.SetCurrentSelectBuilding("");
    }



    public void TryBuild(int configid, int x, int z)
    {
        this.SetCurrentSelectBuilding("");
        this._myLandCity.SetOtherTransparent(this._currentBuildKey, true);

        BuildingConfig config = BuildingConfig.Instance.GetData(configid);
        Building prefab;
        if (config == null)
            return;
        Building building = this._myLandCity.InitOneBuild(configid, x, z);
        building._data = new BuildingData();
        building._data.Create(configid, x, z);
        building.SetCurrentState();
        building.SetSelect(true);
        building._flashBase.gameObject.SetActive(true);


        _BuildCanvas.transform.SetParent(building.transform);
        int zOff = config.RowCount - 2;
        _BuildCanvas.transform.localPosition = new Vector3(0, 0, zOff);

        _BuildCanvas.Show();
        _TryBuildScript = building;
        _TryBuildScript.SetCanDoState(x, z);

    }

    public void SetBuildCanvasState(bool canBuild)
    {
        this._BuildCanvas.SetState(canBuild);
    }

    public void ConfirmBuild(bool isBuild)
    {
        if (this._TryBuildScript == null)
            return;

        this._BuildCanvas.Hide();
        _BuildCanvas.transform.SetParent(this.transform);
        if (isBuild == false)
        {
            GameObject.Destroy(this._TryBuildScript.gameObject);
            this._TryBuildScript = null;
            return;
        }

        //通知Proxy建造一个Building
        int x = (int)_TryBuildScript.transform.position.x;
        int z = (int)_TryBuildScript.transform.position.z;

        BuildingConfig _config = BuildingConfig.Instance.GetData(_TryBuildScript._data._id);
        bool canBuild = this.canBuildInSpot("", x, z, _config.RowCount, _config.ColCount);
        if (canBuild == false)
            return;

        Dictionary<string, object> vo = new Dictionary<string, object>();
        vo["configid"] = _TryBuildScript._data._id;
        vo["x"] = x;
        vo["z"] = z;
        MediatorUtil.SendNotification(NotiDefine.CreateOneBuildingDo, vo);
    }

    public void HideInfoCanvas()
    {
        if (this._infoCanvas != null)
        {
            _infoCanvas.Hide();
        }
    }

    public void ShowBuildingInfoCanvas(Building bd)
    {
        if (bd == null)
        {
            this.HideInfoCanvas();
            return;
        }

        if (this._infoCanvas == null)
        {
            string name = MediatorUtil.GetName(MediatorDefine.MAIN);
            MainMediator mediator = ApplicationFacade.instance.RetrieveMediator(name) as MainMediator;
            this._infoCanvas = mediator.GetView()._InfoUI;
        }
        _infoCanvas.Show();
        _infoCanvas.SetBuildState(bd._data);
    }

    public void Build(int configid, int x, int z)
    {
        BuildingConfig config = BuildingConfig.Instance.GetData(configid);
        if (config == null)
            return;

        bool canBuild = this.canBuildInSpot("", x, z, config.RowCount, config.ColCount);
        if (canBuild == false)
            return;

        //通知Proxy建造一个Building
        Dictionary<string, object> vo = new Dictionary<string, object>();
        vo["configid"] = configid;
        vo["x"] = x;
        vo["z"] = z;

        MediatorUtil.SendNotification(NotiDefine.CreateOneBuildingDo, vo);
    }



    public void SetCurrentSelectBuilding(string key)
    {
        this._currentBuildKey = key;
        Building showBd = this._myLandCity.SetCurrentSelectBuilding(this._currentBuildKey, this.isTryBuild);
        this.ShowBuildingInfoCanvas(showBd);
    }

    private bool _isDraging;
    public void SetDraging(bool isDrag)
    {
        this._isDraging = isDrag;
        this._myLandCity.SetOtherTransparent(this._currentBuildKey,isDrag || this.isTryBuild);
    }

    public void SetQuadVisible(bool show)
    {
        //this._QuadCanBuild.gameObject.SetActive(show);
    }

    public bool IsDraging => _isDraging;



    public void InitScene()
    {
        this._config = WorldConfig.Instance.GetData(this.World);
        int row = this._config.MaxRowCount + 1;
        int col = this._config.MaxColCount + 1;
        ViewControllerLocal.GetInstance().InitBorder(row, col);

        this._HomePlane.transform.localScale = new Vector3(row, col, 1);
        this._HomePlane.material.SetVector("_MainTex_ST", new Vector4(row, col, 0, 0));
        this.GenerateMyCity();
        this.SetMySpotRange();
    }

    public void SetMySpotRange()
    {
        int range = WorldProxy._instance.GetBuildingEffects().BuildRange;
        this._QuadCanBuild.transform.localScale = new Vector3(range + 3, range + 3, 1);
        this._QuadCanBuild.material.SetVector("_MainTex_ST", new Vector4(range + 3, range + 3, 0, 0));
        this._myLandCity.SetRange(range, range);
    }


    private void GenerateMyCity()
    {
        WorldBuildings worldData = WorldProxy._instance.Data;
        this._myLandCity = GameObject.Instantiate<MyCity>(this._cityPrefab, new Vector3(worldData.StartCordinates.x, 0, worldData.StartCordinates.y), Quaternion.identity, this.transform);
        this._myLandCity.name = "MyCity";
        this._myLandCity.CreateMyCity();
        StartCoroutine(InComeCountDown());
    }



    IEnumerator InComeCountDown()
    {
        WaitForSeconds waitYield = new WaitForSeconds(60f);
        while (this.gameObject != null && this.gameObject.activeSelf)
        {
            this.UpdateIncome();
            yield return waitYield;
        }
    }

    public void UpdateIncome()
    {
        _myLandCity.UpdateIncome();
    }

}//end cloass
