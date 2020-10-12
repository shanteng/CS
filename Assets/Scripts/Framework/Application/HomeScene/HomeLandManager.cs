using Newtonsoft.Json.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeLandManager : MonoBehaviour
{
    public static float Degree = 45;

    public static int COL_COUNT = 50;
    public static int ROW_COUNT = 50;
    public List<SpotCube> _spotPrefabs;
    public CountDownCanvas _coundPrefabs;
    public BuildCanvas _buildPrefabs;
    public InfoCanvas _infoPrefabs;

    public List<Building> _BuildPrefabs;
    private Dictionary<string, Building> _BuildPrefabDic;


    private Dictionary<string, List<Vector2Int>> _BuildingDic;
    private Dictionary<string,Building> _AllBuildings;
    private List<string> _SpotHasOccupys;//key x|z,bool value
    private Dictionary<string, SpotCube> _allSpotDic;//key格式x|y,存储当前所有的地块
    public bool isTryBuild => this._TryBuildScript != null;
    private Building _TryBuildScript;
    private BuildCanvas _BuildCanvas;
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
        this._AllBuildings = new Dictionary<string, Building>();
        this._SpotHasOccupys = new List<string>();
        this._BuildingDic = new Dictionary<string, List<Vector2Int>>();
        this._allSpotDic = new Dictionary<string, SpotCube>();
        this.GenerateAllBaseSpot();
    }

    public bool canBuildInSpot(string key, int posX, int posZ,int rowCount,int colCount)
    {
        for (int row = 0; row < rowCount; ++row)
        {
            int curX = posX + row;
            for (int col = 0; col < colCount; ++col)
            {
                int curZ = posZ + col;
                string spotKey = UtilTools.combine(curX, "|", curZ);
                bool inOther = _SpotHasOccupys.Contains(spotKey);//是否占领了别人的格子
                bool isInMySpot = this.isInMyOldRange(curX, curZ, key);//占领的是不本身我自己的格子
                if (inOther && isInMySpot == false)
                    return false;//不可以建造
            }
        }

        return true;
    }

    private bool isInMyOldRange(int x, int z, string key)
    {
        List<Vector2Int> buildOccupy = null;
        if (this._BuildingDic.TryGetValue(key, out buildOccupy) == false)
            return false;//我本身还没有建造

        foreach (Vector2Int pos in buildOccupy)
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
        building.CreateUI(this._coundPrefabs);
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
        if (this._AllBuildings.TryGetValue(key, out building))
            return building;
        return null;
    }

   

    public void OnClickSpotCube(int x,int z)
    {
        if (this.isTryBuild)
            return;
        this.SetCurrentSelectBuilding("");
        this.TryBuild(1, x, z);//测试
    }

  
    public void TryBuild(int configid, int x, int z)
    {
        this.SetCurrentSelectBuilding("");
        BuildingConfig config = BuildingConfig.Instance.GetData(configid);
        Building prefab;
        if (config == null || this._BuildPrefabDic.TryGetValue(config.Prefab, out prefab) == false)
            return;
        Building building = GameObject.Instantiate<Building>(prefab, new Vector3(x, 1, z), Quaternion.identity, this.transform);
        building._data = new BuildingData();
        building._data.Create(configid, x, z);
        building.SetCurrentState();
        building.SetSelect(true);
        building._basePlane.gameObject.SetActive(true);
        
        if (this._BuildCanvas == null)
            this._BuildCanvas = GameObject.Instantiate<BuildCanvas>(this._buildPrefabs, Vector3.zero, Quaternion.identity, this.transform);
        

        _BuildCanvas.transform.SetParent(building.transform);
        _BuildCanvas.transform.localPosition = Vector3.zero;
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

        bool canBuild = this.canBuildInSpot("", x, z, _TryBuildScript._data._config.RowCount, _TryBuildScript._data._config.ColCount);
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
            _infoCanvas.transform.SetParent(this.transform);
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
            this._infoCanvas = GameObject.Instantiate<InfoCanvas>(this._infoPrefabs, Vector3.zero, Quaternion.identity, this.transform);
        _infoCanvas.Show();
        _infoCanvas.transform.SetParent(bd.transform);
        _infoCanvas.transform.localPosition = Vector3.zero;
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

    public  bool IsDraging => _isDraging;

    public void ClearBuildOccupy(string key)
    {
        List<Vector2Int> buildOccupy = null;
        if (this._BuildingDic.TryGetValue(key, out buildOccupy))
        {
            foreach (Vector2Int pos in buildOccupy)
            {
                string oldKey = UtilTools.combine(pos.x, "|", pos.y);
                this._SpotHasOccupys.Remove(oldKey);
            }
        }
    }

    public void RecordBuildOccupy(string key, List<Vector2Int> occs)
    {
        this.ClearBuildOccupy(key);
 
        List<Vector2Int> buildOccupy = new List<Vector2Int>();
        buildOccupy.AddRange(occs);
        this._BuildingDic[key] = buildOccupy;
        foreach (Vector2Int pos in buildOccupy)
        {
            string curKey = UtilTools.combine(pos.x, "|", pos.y);
            this._SpotHasOccupys.Add(curKey);
        }
    }

    public void InitScene()
    {
        
    }

    private void GenerateAllBaseSpot()
    {
        for (int row = 0; row < ROW_COUNT; ++row)
        {
            int corX = row;
            int start = row % 2;
            for (int col = 0; col < COL_COUNT; ++col)
            {
                int curIndex = (start + 1 +col) % 2;
                SpotCube prefab = this._spotPrefabs[curIndex];
                int corZ = col;
                SpotCube baseSpot = GameObject.Instantiate<SpotCube>(prefab, new Vector3(corX, 0, corZ), Quaternion.identity, this.transform);
                baseSpot.SetCordinate(corX, corZ);
                string key = UtilTools.combine(corX, "|", corZ);
                baseSpot.name = key;
                this._allSpotDic[key] = baseSpot;
            }
        }
    }


}
