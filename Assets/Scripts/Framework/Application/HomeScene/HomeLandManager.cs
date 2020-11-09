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

    private Dictionary<int, MyCity> _OwnCityDic = new Dictionary<int, MyCity>();
  

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
        MyCity city = this.GetOwnCity(0);

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
        MyCity city = this.GetOwnCity(0);

        if (this._TryBuildScript == null)
            return;
        //创建一个
        Building building = _TryBuildScript;
        building._data = data;
        building.name = building._data._key;
        building.SetCurrentState();
        city.AddOneBuilding(data._key, building);
        city.RecordBuildOccupy(building._data._key, building._data._occupyCordinates);
        this.SetCurrentSelectBuilding(data._key);
        this._TryBuildScript = null;
    }

    public void OnRelocateResp(string key)
    {
        MyCity city = this.GetOwnCity(0);

        Building curBuild = city.GetBuilding(key);
        if (curBuild != null)
        {
            city.RecordBuildOccupy(curBuild._data._key, curBuild._data._occupyCordinates);
            this.SetCurrentSelectBuilding("");
           //this.ShowBuildingInfoCanvas(curBuild);
        }
    }

    public void OnBuildingStateChanged(string key)
    {
        MyCity city = this.GetOwnCity(0);
        Building curBuild = city.GetBuilding(key);
        if (curBuild != null)
        {
            curBuild.SetCurrentState();
            if (key.Equals(this._currentBuildKey))
                this.ShowBuildingInfoCanvas(curBuild);
        }
    }

    public void OnRemoveBuild(string key)
    {
        MyCity city = this.GetOwnCity(0);

        this.HideInfoCanvas();
        Building curBuild = city.GetBuilding(key);
        if (curBuild != null)
        {
            city.ClearBuildOccupy(key);
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


 

    public MyCity GetOwnCity(int cityid)
    {
        MyCity city;
        this._OwnCityDic.TryGetValue(cityid, out city);
        return city;
    }


    public void TryBuild(int configid, int x, int z)
    {
        MyCity city = this.GetOwnCity(0);
        this.SetCurrentSelectBuilding("");
        city.SetOtherTransparent(this._currentBuildKey, true);

        BuildingConfig config = BuildingConfig.Instance.GetData(configid);
        if (config == null)
            return;
        Building building = city.InitOneBuild(configid, x, z);
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
        MyCity city = this.GetOwnCity(0);
        this._currentBuildKey = key;
        Building showBd = city.SetCurrentSelectBuilding(this._currentBuildKey, this.isTryBuild);
        this.ShowBuildingInfoCanvas(showBd);
    }

    private bool _isDraging;
    public void SetDraging(bool isDrag)
    {
        MyCity city = this.GetOwnCity(0);
        this._isDraging = isDrag;
        city.SetOtherTransparent(this._currentBuildKey,isDrag || this.isTryBuild);
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

    public void OnClickSpot(Vector3 pos)
    {
        if (this.isTryBuild)
            return;
      
       
        int x = Mathf.RoundToInt(pos.x);
        int z = Mathf.RoundToInt(pos.z);

        VInt2 gamePos = UtilTools.WorldToGameCordinate(x, z);
        if (gamePos.x < 0 || gamePos.x > GameIndex.ROW || gamePos.y < 0 || gamePos.y > GameIndex.COL)
        {
            return;
        }

        this._SelectSpotTrans.JudegeShow(x, z);
        this.SetCurrentSelectBuilding("");
        Dictionary<string, object> vo = new Dictionary<string, object>();
        vo["city"] = 0;
        vo["x"] = x;
        vo["z"] = z;
        WorldProxy._instance.DoPatrol(vo);

        /*   BuildingData bd = WorldProxy._instance.GetBuildingInRange(x, z);

           if (bd != null && bd._city == 0)
           {
              // MyCity city = this.GetOwnCity(bd._city);
             //  city.OnClickBuilding(bd._key);
           }
           else if (bd != null && bd._city > 0)
           {
               //占领的城市
           }
          */
    }

    public void InitScene()
    {
        this._config = WorldConfig.Instance.GetData(this.World);
        int row = this._config.MaxRowCount + 1;
        int col = this._config.MaxColCount + 1;
        ViewControllerLocal.GetInstance().InitBorder(row, col);

        this._HomePlane.transform.localScale = new Vector3(row, col, 1);
        this._HomePlane.material.SetVector("_MainTex_ST", new Vector4(row, col, 0, 0));
        this._HomePlane.GetComponent<SpotCube>().AddEvent(this.OnClickSpot);

        this.GenerateMyCity();
        this.SetMainCityRange();

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
        MyCity city = this.GetOwnCity(0);
        int range = WorldProxy._instance.GetBuildingEffects(0).BuildRange;
        this._QuadCanBuild.transform.localScale = new Vector3(range, range, 1);
        this._QuadCanBuild.material.SetVector("_MainTex_ST", new Vector4(range, range, 0, 0));
        city.SetRange(range);
    }

    private Dictionary<string, PathModel> _pathModels = new Dictionary<string, PathModel>();
    public void AddOnePath(PathData path)
    {
        PathModel PathModel = GameObject.Instantiate<PathModel>(this._pathModelPrefab, Vector3.zero, Quaternion.identity, this.transform);
        PathModel.name = UtilTools.combine("path-", path.ID);
        PathModel.DoPath(path);
        _pathModels.Add(path.ID, PathModel);
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
        this._OwnCityDic.Clear();
        WorldBuildings worldData = WorldProxy._instance.Data;
        MyCity city = GameObject.Instantiate<MyCity>(this._cityPrefab, new Vector3(worldData.StartCordinates.x, 0, worldData.StartCordinates.y), Quaternion.identity, this.transform);
        city.name = "MyCity";
        city.CreateMyCity();
        this._OwnCityDic.Add(0, city);
        StartCoroutine(InComeCountDown());
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
        MyCity city = this.GetOwnCity(0);
        city.UpdateIncome();
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
        ViewControllerLocal.GetInstance().TryGoto(data._cordinate,this.OnGotoEnd,key);
       
    }

    public void OnGotoEnd(object param)
    {
        string key = (string)param;
        this.SetCurrentSelectBuilding(key);
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



}//end cloass
