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



    private Dictionary<string, List<Vector2Int>> _BuildingDic;
    private Dictionary<string,Building> _AllBuildings;
    private List<string> _SpotHasOccupys;//key x|z,bool value
    public Building _BuildPrefab;
    private Dictionary<string, SpotCube> _allSpotDic;//key格式x|y,存储当前所有的地块

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

    public void OnBuildingStateChanged(string key)
    {
        Building curBuild = this.GetBuilding(key);
        if (curBuild != null)
            curBuild.SetCurrentState();
    }

    public void OnRelocateResp(string key)
    {
        Building curBuild = this.GetBuilding(key);
        if (curBuild != null)
            curBuild.EndRelocate();
    }

    private Building GetBuilding(string key)
    {
        Building building;
        if (this._AllBuildings.TryGetValue(key, out building))
            return building;
        return null;
    }

    public void OnCreateResp(BuildingData data)
    {
        //创建一个
        Building building = GameObject.Instantiate<Building>(this._BuildPrefab, new Vector3(data._cordinate.x, 1, data._cordinate.y), Quaternion.identity, this.transform);
        building._data = data;
        building.name = UtilTools.combine(building._data._key + "|" + building._data._id);
        building.CreateCountDownUI(this._coundPrefabs);
        building.SetCurrentState();
        this._AllBuildings.Add(data._key,building);
        this.RecordBuildOccupy(building._data._key, building._data._occupyCordinates);
    }

    public void OnClickSpotCube(int x,int z)
    {
        HomeLandManager.GetInstance().UnSelectOtherBuilding("");
        this.Build(1, x, z);//测试
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

    public void UnSelectOtherBuilding(string key)
    {
        foreach (Building bd in this._AllBuildings.Values)
        {
            bool isKey = key.Equals(bd._data._key);
            if(isKey ==false)
                bd.EndRelocate();
        }
    }

    public void RecordBuildOccupy(string key, List<Vector2Int> occs)
    {
        List<Vector2Int> buildOccupy = null;
        if (this._BuildingDic.TryGetValue(key, out buildOccupy))
        {
            //先清楚之前的占地信息
            foreach (Vector2Int pos in buildOccupy)
            {
                string oldKey = UtilTools.combine(pos.x, "|", pos.y);
                this._SpotHasOccupys.Remove(oldKey);
            }
        }

        buildOccupy = new List<Vector2Int>();
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
