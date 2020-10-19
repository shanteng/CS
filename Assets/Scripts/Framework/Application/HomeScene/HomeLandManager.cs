using Newtonsoft.Json.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BuildingData;

public class HomeLandManager : MonoBehaviour
{
    public static float Degree = 45;

    private WorldConfig _config;
    public int World = 1;
    public MeshRenderer _HomePlane;
    public MeshRenderer _QuadCanBuild;

    public List<SpotCube> _spotPrefabs;
    public BuildingUI _uiPrefabs;
    public BuildCanvas _BuildCanvas;

    public List<Building> _BuildPrefabs;
    private Dictionary<string, Building> _BuildPrefabDic;


    private Dictionary<string, List<VInt2>> _BuildingDic;
    private Dictionary<string,Building> _AllBuildings;
    private List<string> _SpotHasOccupys;//key x|z,bool value

    private List<string> _canOperateSpots = new List<string>();//key x|z
   // private Dictionary<string, SpotCube> _allSpotDic;//key格式x|y,存储当前所有的地块
    public bool isTryBuild => this._TryBuildScript != null;
    private Building _TryBuildScript;
    private InfoCanvas _infoCanvas;

    private static HomeLandManager instance;
    public static HomeLandManager GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
        this._BuildPrefabDic = new Dictionary<string, Building>();
        foreach (Building bd in this._BuildPrefabs)
        {
            this._BuildPrefabDic[bd.name] = bd;
        }
        
    }


    void Start()
    {
        this._BuildCanvas.Hide();
      //  this._QuadCanBuild.gameObject.SetActive(false);
    }


    public bool canBuildInSpot(string key, int posX, int posZ,int rowCount,int colCount)
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
                bool inOther = _SpotHasOccupys.Contains(spotKey);//是否占领了别人的格子
                bool isInMySpot = this.isInMyOldRange(curX, curZ, key);//占领的是不本身我自己的格子
                if (inOther && isInMySpot == false)
                    return false;//不可以建造
            }
        }

        return true;
    }

    public bool CanOperateSpot(int x, int z)
    {
        string key = UtilTools.combine(x, "|", z);
        return this._canOperateSpots.Contains(key);
    }

    private bool isInMyOldRange(int x, int z, string key)
    {
        List<VInt2> buildOccupy = null;
        if (this._BuildingDic.TryGetValue(key, out buildOccupy) == false)
            return false;//我本身还没有建造

        foreach (VInt2 pos in buildOccupy)
        {
            if (pos.x == x && pos.y == z)
                return true;
        }

        return false;
    }


    public void OnCreateResp(BuildingData data)
    {
        if (this._TryBuildScript == null)
            return;
        //创建一个
        Building building = _TryBuildScript;
        building._data = data;
        building.name = UtilTools.combine(building._data._key + "|" + building._data._id);
        building.SetCurrentState();
        this._AllBuildings.Add(data._key, building);
        this.RecordBuildOccupy(building._data._key, building._data._occupyCordinates);
        this.SetCurrentSelectBuilding(data._key);
        this._TryBuildScript = null;
    }

    public void OnRelocateResp(string key)
    {
        Building curBuild = this.GetBuilding(key);
        if (curBuild != null)
        {
            this.RecordBuildOccupy(curBuild._data._key, curBuild._data._occupyCordinates);
            this.ShowBuildingInfoCanvas(curBuild);
        }
    }

    public void OnBuildingStateChanged(string key)
    {
        Building curBuild = this.GetBuilding(key);
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
        Building curBuild = this.GetBuilding(key);
        if (curBuild != null)
        {
            this.ClearBuildOccupy(key);
            _AllBuildings.Remove(key);
            GameObject.Destroy(curBuild.gameObject);
            curBuild = null;
        }
    }


    private Building GetBuilding(string key)
    {
        Building building;
        if (this._AllBuildings != null && this._AllBuildings.TryGetValue(key, out building))
            return building;
        return null;
    }

    public void BuildInScreenCenterPos(int id)
    {
       // Vector3 screenSpace = Camera.main.WorldToScreenPoint(new Vector3(0, 0, 0));
        Vector3 posScreen = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(posScreen);//摄像机发射射线到屏幕点。
        if (Physics.Raycast(ray, out hit))//射线发出并碰撞到外壳，那么手臂就应该朝向碰撞点
        {
            
            int xCenter = Mathf.RoundToInt(hit.point.x);
            int zCenter = Mathf.RoundToInt(hit.point.z);
            this.TryBuild(id, xCenter, zCenter);//测试
        }

        // Vector3 posScreenWorld = Camera.main.ScreenToWorldPoint(posScreen);


       
    }
   

    public void OnClickSpotCube(int x,int z)
    {
        if (this.isTryBuild)
            return;
        this.SetCurrentSelectBuilding("");
    }

    private Building InitOneBuild(int configid, int x, int z)
    {
        BuildingConfig config = BuildingConfig.Instance.GetData(configid);
        Building prefab;
        if (config == null || this._BuildPrefabDic.TryGetValue(config.Prefab, out prefab) == false)
            return null;
        Building building = GameObject.Instantiate<Building>(prefab, new Vector3(x, Building.drag_offsety, z), Quaternion.identity, this.transform);
        building.CreateUI(this._uiPrefabs,configid);
        building.SetRowCol(config.RowCount, config.ColCount);
        return building;
    }
  
    public void TryBuild(int configid, int x, int z)
    {
        this.SetCurrentSelectBuilding("");
        BuildingConfig config = BuildingConfig.Instance.GetData(configid);
        Building prefab;
        if (config == null || this._BuildPrefabDic.TryGetValue(config.Prefab, out prefab) == false)
            return;
        Building building = this.InitOneBuild(configid,x,z);
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

    public void Build(int configid,int x, int z)
    {
        BuildingConfig config = BuildingConfig.Instance.GetData(configid);
        if (config == null)
            return;

        bool canBuild = this.canBuildInSpot("", x, z,config.RowCount,config.ColCount);
        if (canBuild == false)
            return;

        //通知Proxy建造一个Building
        Dictionary<string, object> vo = new Dictionary<string, object>();
        vo["configid"] = configid;
        vo["x"] = x;
        vo["z"] = z;

        MediatorUtil.SendNotification(NotiDefine.CreateOneBuildingDo, vo);
    }

    private string _currentBuildKey = "";
    public void SetCurrentSelectBuilding(string key)
    {
        this._currentBuildKey = key;
        Building showBd = null;
        foreach (Building bd in this._AllBuildings.Values)
        {
            bool isCur = key.Equals(bd._data._key);
            bd.SetSelect(isCur);
            if(isCur)
                showBd = bd;
        }

        this.ShowBuildingInfoCanvas(showBd);
    }

    private bool _isDraging;
    public void SetDraging(bool isd)
    {
        this._isDraging = isd;
        
    }

    public void SetQuadVisible(bool show)
    {
        //this._QuadCanBuild.gameObject.SetActive(show);
    }

    public  bool IsDraging => _isDraging;

    public void ClearBuildOccupy(string key)
    {
        List<VInt2> buildOccupy = null;
        if (this._BuildingDic.TryGetValue(key, out buildOccupy))
        {
            foreach (VInt2 pos in buildOccupy)
            {
                string oldKey = UtilTools.combine(pos.x, "|", pos.y);
                this._SpotHasOccupys.Remove(oldKey);
            }
        }
    }

    public void RecordBuildOccupy(string key, List<VInt2> occs)
    {
        this.ClearBuildOccupy(key);
 
        List<VInt2> buildOccupy = new List<VInt2>();
        buildOccupy.AddRange(occs);
        this._BuildingDic[key] = buildOccupy;
        foreach (VInt2 pos in buildOccupy)
        {
            string curKey = UtilTools.combine(pos.x, "|", pos.y);
            this._SpotHasOccupys.Add(curKey);
        }
    }

    public void InitScene()
    {
        this._config = WorldConfig.Instance.GetData(this.World);
        int row = this._config.MaxRowCount + 1;
        int col = this._config.MaxColCount + 1;
        this._HomePlane.transform.localScale = new Vector3(row, col, 1);
        this._HomePlane.material.SetVector("_MainTex_ST", new Vector4(row, col, 0, 0));


        row = this._config.RowCount + 1;
        col = this._config.ColCount + 1;
        this._QuadCanBuild.transform.localScale = new Vector3(row, col, 1);
        this._QuadCanBuild.material.SetVector("_MainTex_ST", new Vector4(row, col, 0, 0));


        MediatorUtil.SendNotification(NotiDefine.GenerateMySpotDo,this.World);
        MediatorUtil.SendNotification(NotiDefine.GenerateMyBuildingDo,this.World);

        ViewControllerLocal.GetInstance().InitBorder(this._config.RowCount, this._config.ColCount);
    }

    private void CreateOneBuilding(BuildingData data)
    {
        //创建一个
        Building building   = this.InitOneBuild(data._id, data._cordinate.x, data._cordinate.y);
        building._data = data;
        building.name = UtilTools.combine(building._data._key + "|" + building._data._id);
        building.SetCurrentState();
        this._AllBuildings.Add(data._key, building);
        this.RecordBuildOccupy(building._data._key, building._data._occupyCordinates);
    }

    public void OnGenerateMySpotEnd(List<string> mySpots)
    {
        this._canOperateSpots = mySpots;
    }

    public void OnGenerateAllBuildingEnd(Dictionary<string, BuildingData> builds)
    {
        this._SpotHasOccupys = new List<string>();
        this._AllBuildings = new Dictionary<string, Building>();
        this._BuildingDic = new Dictionary<string, List<VInt2>>();
        foreach (BuildingData data in builds.Values)
        {
            this.CreateOneBuilding(data);
        }

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
        foreach (Building bd in this._AllBuildings.Values)
        {
            bd.UpdateIncome();
        }
    }

}//end cloass
