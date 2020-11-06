using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class MyCity : MonoBehaviour
{
    public BuildingUI _uiPrefabs;
    public CityRange _Wall;


    
    private Dictionary<string, Building> _AllBuildings;
    private List<string> _SpotHasOccupys;//key x|z,bool value
    private Dictionary<string, List<VInt2>> _BuildingDic;

    private void Awake()
    {
       
    }

    public void UpdateIncome()
    {
        foreach (Building bd in this._AllBuildings.Values)
        {
            bd.UpdateIncome();
        }
    }

    public void SetOtherTransparent(string key,bool isDrag)
    {
        foreach (Building bd in _AllBuildings.Values)
        {
            bool isCur = key.Equals(bd._data._key);
            if(isCur == false)
                bd.DoTransparent(isDrag);
        }
    }

    public Building SetCurrentSelectBuilding(string key,bool isTryBuild)
    {
        Building showBd = null;
        foreach (Building bd in _AllBuildings.Values)
        {
            bool isCur = key.Equals(bd._data._key);
           // if ((key.Equals("") == false && isCur == false) || isTryBuild)
         //       bd.DoTransparent();
        //    else
                bd.SetSelect(isCur);

            if (isCur)
                showBd = bd;
        }
        return showBd;
    }

    public bool HasOccupy(string spotKey)
    {
        return this._SpotHasOccupys.Contains(spotKey);
    }

    public void AddOneBuilding(string key, Building bd)
    {
        this._AllBuildings.Add(key, bd);
    }

    public void CreateMyCity()
    {
        this._SpotHasOccupys = new List<string>();
        this._AllBuildings = new Dictionary<string, Building>();
        this._BuildingDic = new Dictionary<string, List<VInt2>>();
        List<BuildingData> datas = WorldProxy._instance.GetCityBuildings(0);
        foreach (BuildingData data in datas)
        {
            this.CreateOneBuilding(data);
        }
    }

    private void CreateOneBuilding(BuildingData data)
    {
        //创建一个
        Building building = this.InitOneBuild(data._id, data._cordinate.x, data._cordinate.y);
        building._data = data;
        building.name = building._data._key;
        building.SetCurrentState();
        this._AllBuildings.Add(data._key, building);
        this.RecordBuildOccupy(building._data._key, building._data._occupyCordinates);
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

    public bool isInMyOldRange(int x, int z, string key)
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

    public Building GetBuilding(string key)
    {
        Building building;
        if (this._AllBuildings != null && this._AllBuildings.TryGetValue(key, out building))
            return building;
        return null;
    }


    public Building InitOneBuild(int configid, int x, int z)
    {
        BuildingConfig config = BuildingConfig.Instance.GetData(configid);
        GameObject prefab =  ResourcesManager.Instance.LoadBuildingRes(config.Prefab);
        GameObject obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, this.transform);
        Building building = obj.GetComponent<Building>();
        building.transform.localPosition = new Vector3(x, Building.drag_offsety, z);
        building.CreateUI(this._uiPrefabs, configid);
        building.SetRowCol(config.RowCount, config.ColCount);
        return building;
    }

    public void SetRange(int range)
    {
        this._Wall.SetRange(range);
    }

}
