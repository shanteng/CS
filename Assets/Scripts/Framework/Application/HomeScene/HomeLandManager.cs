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

    public ClickSpot _SelectSpotTrans;
    public Transform _VisibleRoot;
    private Dictionary<string, VisibleSpot> _VisibleSpots = new Dictionary<string, VisibleSpot>();
    public MeshRenderer _HomePlane;
    public MeshRenderer _QuadCanBuild;

    public PathModel _pathModelPrefab;
    public VisibleSpot _VisibleSpotPrefab;
    public MyCity _cityPrefab;
    public BuildCanvas _BuildCanvas;

    private MyCity _MyCity;
    private Dictionary<int, City> _ShowCitys = new Dictionary<int, City>();
  

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
        this.HideSelectSpot();
    }

    void Start()
    {
        this._BuildCanvas.Hide();
        this._QuadCanBuild.gameObject.SetActive(true);
        this._HomePlane.gameObject.SetActive(true);
    }

    public string CurrentKey => this._currentBuildKey;

    public bool canBuildInSpot(string key, int posX, int posZ, int rowCount, int colCount)
    {
        MyCity city = _MyCity;

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
                bool inOther = city.HasOccupy(spotKey);//是否占领了别人的格子
                bool isInMySpot = city.isInMyOldRange(curX, curZ, key);//占领的是不本身我自己的格子
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
        _MyCity.AddOneBuilding(data._key, building);
        _MyCity.RecordBuildOccupy(building._data._key, building._data._occupyCordinates);
        this.SetCurrentSelectBuilding(data._key);
        this._TryBuildScript = null;
    }

    public void OnRelocateResp(string key)
    {
        Building curBuild = _MyCity.GetBuilding(key);
        if (curBuild != null)
        {
            _MyCity.RecordBuildOccupy(curBuild._data._key, curBuild._data._occupyCordinates);
            this.SetCurrentSelectBuilding("");
           //this.ShowBuildingInfoCanvas(curBuild);
        }
    }

    public void OnBuildingStateChanged(string key)
    {
        Building curBuild = _MyCity.GetBuilding(key);
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
        Building curBuild = _MyCity.GetBuilding(key);
        if (curBuild != null)
        {
            _MyCity.ClearBuildOccupy(key);
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


    public void TryBuild(int configid, int x, int z)
    {
        this.SetCurrentSelectBuilding("");
        _MyCity.SetOtherTransparent(this._currentBuildKey, true);

        BuildingConfig config = BuildingConfig.Instance.GetData(configid);
        if (config == null)
            return;
        Building building = _MyCity.InitOneBuild(configid, x, z);
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
            return;
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



    public void SetCurrentSelectBuilding(string key,bool isHideInfoCanvas = true)
    {
        this._currentBuildKey = key;
        Building showBd = _MyCity.SetCurrentSelectBuilding(this._currentBuildKey, this.isTryBuild);
        if (showBd == null && isHideInfoCanvas)
        {
            this.HideInfoCanvas();
        }
        else
        {
            this.ShowBuildingInfoCanvas(showBd);
        }
       
    }

    private bool _isDraging;
    public void SetDraging(bool isDrag)
    {
        this._isDraging = isDrag;
        _MyCity.SetOtherTransparent(this._currentBuildKey,isDrag || this.isTryBuild);
    }

    public void SetQuadVisible(bool show)
    {
        //this._QuadCanBuild.gameObject.SetActive(show);
    }

    public bool IsDraging => _isDraging;

    public void HideSelectSpot()
    {
        this._SelectSpotTrans.Hide();
    }

    public void OnClickNpcCity(int city)
    {
        if (this.isTryBuild)
            return;
        bool isVisible = this._infoCanvas.gameObject.activeSelf;
        bool isShow = isVisible == false || (this._infoCanvas.City != city);
       
        if (isShow)
        {
            CityConfig config = CityConfig.Instance.GetData(city);
            this._SelectSpotTrans.ShowInPostion(config.Position[0], config.Position[1]);
            this._SelectSpotTrans.transform.localScale = new Vector3(config.Range[0], config.Range[0], 1);
            _infoCanvas.Show();
            _infoCanvas.SetNpcCity(city);
        }
        else
        {
            this.HideSelectSpot();
            this.HideInfoCanvas();
        }

        this.SetCurrentSelectBuilding("", !isShow);
    }
   
    public void OnClickSpot(Vector3 pos)
    {
        if (this.isTryBuild)
            return;
      
        int x = Mathf.RoundToInt(pos.x);
        int z = Mathf.RoundToInt(pos.z);

        VInt2 gamePos = UtilTools.WorldToGameCordinate(x, z);
        if (gamePos.x < 0 || gamePos.x > GameIndex.ROW || gamePos.y < 0 || gamePos.y > GameIndex.COL)
            return;

        bool isShow =  this._SelectSpotTrans.JudegeSpotShow(x, z);
        if (isShow)
        {
            this._SelectSpotTrans.transform.localScale = Vector3.one;
            _infoCanvas.Show();
            _infoCanvas.SetEmptySpot(x, z);
        }
        else
        {
            this.HideInfoCanvas();
        }
        this.SetCurrentSelectBuilding("",!isShow);
    }

    public void InitScene()
    {
        this._config = WorldConfig.Instance.GetData(this.World);
        int row = this._config.MaxRowCount + 1;
        int col = this._config.MaxColCount + 1;
        ViewControllerLocal.GetInstance().InitBorder(row, col);

        this._HomePlane.transform.localScale = new Vector3(row, col, 1);
        this._HomePlane.material.SetVector("_MainTex_ST", new Vector4(1, 1, 0, 0));
        this._HomePlane.GetComponent<SpotCube>().AddEvent(this.OnClickSpot);

        this.GenerateMyCity();
        this.SetMainCityRange();
        this.GenerateNpcCity();

        //可视化范围
        int count = this._VisibleSpots.Count;
       foreach(VisibleSpot spot in this._VisibleSpots.Values)
        {
            GameObject.Destroy(spot.gameObject);
        }
        _VisibleSpots.Clear();
        Dictionary<string, VInt2> visibleSpots = WorldProxy._instance.GetVisibleSpots();
        this.GenerateVisibleSpot(visibleSpots);
        this.GenerateAllPath();

        string name = MediatorUtil.GetName(MediatorDefine.MAIN);
        MainMediator mediator = ApplicationFacade.instance.RetrieveMediator(name) as MainMediator;
        this._infoCanvas = mediator.GetView()._InfoUI;

    }

    private void GenerateAllPath()
    {
        foreach (PathModel mod in this._pathModels.Values)
        {
            GameObject.Destroy(mod.gameObject);
        }
        this._pathModels.Clear();

        Dictionary<string, PathData> dic = PathProxy._instance.AllPaths;
        foreach (PathData data in dic.Values)
        {
            this.AddOnePath(data);
        }
    }

    public void SetMainCityRange()
    {
        int range = WorldProxy._instance.GetBuildingEffects(0).BuildRange;
        this._QuadCanBuild.transform.localScale = new Vector3(range, range, 1);
        this._QuadCanBuild.material.SetVector("_MainTex_ST", new Vector4(range, range, 0, 0));
        _MyCity.SetRange(range);
    }

    private Dictionary<string, PathModel> _pathModels = new Dictionary<string, PathModel>();
    public void AddOnePath(PathData path)
    {
        PathModel PathModel = GameObject.Instantiate<PathModel>(this._pathModelPrefab, Vector3.zero, Quaternion.identity, this.transform);
        PathModel.name = UtilTools.combine("path-", path.ID);
        PathModel.DoPath(path);
        _pathModels.Add(path.ID, PathModel);
    }

    private string _selectPathId = "";
    public bool GoToPathCurrentPostion(string id)
    {
        bool isSelect = id.Equals(this._selectPathId) == false;
        foreach (PathModel model in this._pathModels.Values)
        {
            if (model.ID.Equals(id))
            {
                model.SetSelect(isSelect);
                if (isSelect)
                {
                    Vector3 pos = model._modelRoot.transform.position;
                    ViewControllerLocal.GetInstance().TryGotoWorldPostion(pos);
                }
            }
            else
                model.SetSelect(false);
        }

        this._selectPathId = isSelect ? id : "";
        return isSelect;
    }

    public void RemoveOnePath(string pathid)
    {
        PathModel model;
        if (this._pathModels.TryGetValue(pathid, out model))
        {
            this._pathModels.Remove(pathid);
            GameObject.Destroy(model.gameObject);
        }
    }


    private void GenerateMyCity()
    {
        WorldBuildings worldData = WorldProxy._instance.Data;
        _MyCity = GameObject.Instantiate<MyCity>(this._cityPrefab, new Vector3(worldData.StartCordinates.x, 0, worldData.StartCordinates.y), Quaternion.identity, this.transform);
        _MyCity.name = "MyCity";
        _MyCity.CreateMyCity();
        StartCoroutine(InComeCountDown());
    }

    private void GenerateNpcCity()
    {
        foreach (City city in this._ShowCitys.Values)
        {
            GameObject.Destroy(city.gameObject);
        }
        this._ShowCitys.Clear();

        Dictionary<int, CityData> dic = WorldProxy._instance.AllCitys;
        foreach (CityData data in dic.Values)
        {
            if (data.Visible)
                this.AddOneNpcShowCity(data.ID);
        }
    }

    public void NewCitysVisible(List<int> news)
    {
        foreach (int city in news)
        {
            this.AddOneNpcShowCity(city);
        }
    }

    public void SetCityOwn(int cityid)
    {
        foreach (City city in this._ShowCitys.Values)
        {
            if (city.ID == cityid)
            {
                city.UpdateOwn();
                break;
            }
        }
    }

    public void AddOneNpcShowCity(int cityid)
    {
        if (this._ShowCitys.ContainsKey(cityid) || cityid == 0)
            return;
        CityConfig config = CityConfig.Instance.GetData(cityid);
        GameObject prefab = ResourcesManager.Instance.LoadCityRes(config.Model);
        GameObject obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, this.transform);
        City building = obj.GetComponent<City>();
        building.transform.localPosition = new Vector3(config.Position[0], 0, config.Position[1]);
        building.gameObject.name = UtilTools.combine("City", cityid);
        building.SetCity(cityid);
        
        this._ShowCitys.Add(cityid, building);
    }

    public void GenerateVisibleSpot(Dictionary<string, VInt2> list)
    {
        foreach (string key in list.Keys)
        {
            if (this._VisibleSpots.ContainsKey(key))
                continue;
            VisibleSpot spot = GameObject.Instantiate<VisibleSpot>(this._VisibleSpotPrefab, new Vector3(list[key].x, 0, list[key].y), Quaternion.identity, this._VisibleRoot);
            Vector3 pos = spot.transform.localPosition;
            pos.y = 0;
            spot.transform.localPosition = pos;
            spot.transform.localEulerAngles = new Vector3(90, 0, 0);
            spot.name = key;

        //    float zValue = 0.5f;
       //     float wValue = 0.5f;

       //     int x = list[key].x;
      //      int z = list[key].y;

       /*     bool isLeftVisible = WorldProxy._instance.IsSpotVisible(x-1,z);
            bool isRightVisible = WorldProxy._instance.IsSpotVisible(x + 1, z);
            if (isLeftVisible == false)
                zValue = 1f;
            else if (isRightVisible == false)
                zValue = 0f;

            bool isTopVisble = WorldProxy._instance.IsSpotVisible(x, z+1);
            bool isBottomVible = WorldProxy._instance.IsSpotVisible(x, z-1);
            if (isTopVisble == false)
                wValue = 0f;
            else if (isBottomVible == false)
                wValue = 1f;

            spot.GetComponent<MeshRenderer>().material.SetVector("_size", new Vector4(1, 1, zValue, wValue));
       */
            this._VisibleSpots.Add(key,spot);
        }
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
        _MyCity.UpdateIncome();
        MediatorUtil.SendNotification(NotiDefine.JudgeIncome);
    }

    private string _gotoKey;
    public void GotoSelectBuilding(string key)
    {
        this.SetCurrentSelectBuilding("");
        BuildingData data = WorldProxy._instance.GetBuilding(key);
        if (data == null)
            return;
        this._gotoKey = key;
        ViewControllerLocal.GetInstance().TryGoto(data._cordinate);
    }


    public void GotoSelectBuildingBy(int id)
    {
        BuildingData data = WorldProxy._instance.GetTopLevelBuilding(id);
        if (data == null)
        {
            BuildingConfig configNeed = BuildingConfig.Instance.GetData(id);
            PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.GoToBuild, configNeed.Name));
            MediatorUtil.ShowMediator(MediatorDefine.BUILD_CENTER);
            return;
        }
        this.GotoSelectBuilding(data._key);
    }

}//end class