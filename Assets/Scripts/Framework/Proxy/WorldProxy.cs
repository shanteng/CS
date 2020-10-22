using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;


[Serializable]
public class BuildingData
{
    public static int MainCityID = 1;
    public enum BuildingStatus
    {
        INIT = 0,//等待创建的状态
        NORMAL,
        BUILD,
        UPGRADE,
    }

    public int _id;
    public string _key;
      
    public int _UpgradeSecs = 0;//下一个等级所需时间
    public VInt2 _cordinate = new VInt2();//左下角的坐标
    public List<VInt2> _occupyCordinates;//占领的全部地块坐标
  
    public int _level = 0;
    public BuildingStatus _status = BuildingStatus.INIT;//建筑的状态
    public long _expireTime;//建造或者升级的到期时间
    public int _durability;//当前耐久度

    public void Create(int id,int x,int z)
    {
        this._key =  UtilTools.GenerateUId();
        this._id = id;
        this._occupyCordinates = new List<VInt2>();
        this.SetCordinate(x, z);
    }

    public void SetStatus(BuildingStatus state)
    {
        this._status = state;
        this._expireTime = 0;
        _UpgradeSecs = 0;
        if (state == BuildingStatus.BUILD || state == BuildingStatus.UPGRADE)
        {
            BuildingUpgradeConfig nextConfig = BuildingUpgradeConfig.GetConfig(this._id, this._level + 1);
            _UpgradeSecs = nextConfig == null ? 0 : nextConfig.NeedTime;
            this._expireTime = UtilTools.GetExpireTime(_UpgradeSecs);
        }
    }

    public void SetLevel(int newLevel)
    {
        this._level = newLevel;
        BuildingUpgradeConfig configLevel = BuildingUpgradeConfig.GetConfig(this._id, this._level);
        if (configLevel != null)
            this._durability = configLevel.Durability;
    }

    public void SetCordinate(int x, int z)
    {
        this._cordinate.x = x;
        this._cordinate.y = z;
        this._occupyCordinates.Clear();
        BuildingConfig _config = BuildingConfig.Instance.GetData(this._id);

        for (int row = 0; row < _config.RowCount; ++row)
        {
            int curX = this._cordinate.x + row;
            for (int col = 0; col < _config.ColCount; ++col)
            {
                int curZ = this._cordinate.y + col;
                VInt2 corNow = new VInt2(curX, curZ);
                this._occupyCordinates.Add(corNow);
            }
        }
    }
}//end class

//时间戳回调管理
public class WorldProxy : BaseRemoteProxy
{
    public static WorldProxy _instance;

    public static WorldConfig _config;
    private int _World;
    private Dictionary<string, BuildingData> _datas = new Dictionary<string, BuildingData>();
    private List<string> _canOperateSpots = new List<string>();//我可以操作的地块

    public WorldProxy() : base(ProxyNameDefine.WORLD)
    {
        _instance = this;
    }

    public List<string> GetCanOperateSpots()
    {
        return this._canOperateSpots;
    }

    public bool IsBuildingByOverLevel(int id, int level)
    {
        foreach (BuildingData data in this._datas.Values)
        {
            if (data._id == id && data._level >= level)
                return true;
        }
        return false;
    }

    public bool IsBuildingOpen(int id)
    {
        BuildingConfig config = BuildingConfig.Instance.GetData(id);
        if (config.Condition == null || config.Condition.Length == 0)
            return true;

        bool isOverLevel = this.IsBuildingByOverLevel(config.Condition[0], config.Condition[1]);
        return isOverLevel;
    }

    public int GetBuildingCount(int id)
    {
        int count = 0;
        foreach (BuildingData data in this._datas.Values)
        {
            if (data._id == id)
                count++;
        }
        return count;
    }

    public void GenerateAllBaseSpot(int world)
    {
        //从数据库取出数据
        this._World = world;
        this._canOperateSpots.Clear();
        WorldProxy._config = WorldConfig.Instance.GetData(this._World);
        GameIndex.ROW = WorldProxy._config.MaxRowCount;
        GameIndex.COL = WorldProxy._config.MaxColCount;


        string fileName = UtilTools.combine(SaveFileDefine.WorldSpot, world);
        string json = CloudDataTool.LoadFile(fileName);
        WorldCanOprateSpot spots = new WorldCanOprateSpot();
        spots.World = world;
        spots.Dpots = new List<string>();
        if (json.Equals(string.Empty) == false)
        {
            spots = Newtonsoft.Json.JsonConvert.DeserializeObject<WorldCanOprateSpot>(json);
        }
        else
        {
            int halfRow = WorldProxy._config.RowCount / 2;
            int halfCol = WorldProxy._config.ColCount / 2;
            for (int row = -halfRow; row <= halfRow; ++row)
            {
                int corX = row;
                for (int col = -halfCol; col <= halfCol; ++col)
                {
                    int corZ = col;
                    string key = UtilTools.combine(corX, "|", corZ);
                    spots.Dpots.Add(key);
                }
            }//end for
            this.DoSaveLocalSpots(spots);
        }//end else
        this._canOperateSpots.AddRange(spots.Dpots);
        this.SendNotification(NotiDefine.GenerateMySpotResp, this._canOperateSpots);
    }//end func

    public void DoSaveLocalSpots(WorldCanOprateSpot script)
    {
        //存储
        string fileName = UtilTools.combine(SaveFileDefine.WorldSpot, this._World);
        CloudDataTool.SaveFile(fileName, script);
    }

    public void GenerateAllBuilding(int world)
    {
        this._datas.Clear();
        string fileName = UtilTools.combine(SaveFileDefine.WorldBuiding, world);
        string json = CloudDataTool.LoadFile(fileName);
        WorldBuildings builds = new WorldBuildings();
        builds.World = world;
        if (json.Equals(string.Empty))
        {
            //构造一个主城
            BuildingData mainCity = new BuildingData();
            BuildingConfig config = BuildingConfig.Instance.GetData(BuildingData.MainCityID);
            int posx = (config.RowCount - 1) / 2;
            int posz = (config.ColCount - 1) / 2;
            mainCity.Create(BuildingData.MainCityID, -posx, -posz);
            mainCity.SetLevel(1);
            mainCity.SetStatus(BuildingData.BuildingStatus.NORMAL);
            this._datas.Add(mainCity._key, mainCity);
            this.DoSaveLocalBuildings();
        }
        else
        {
            builds = Newtonsoft.Json.JsonConvert.DeserializeObject<WorldBuildings>(json);
            foreach (BuildingData data in builds.Datas)
            {
                this._datas[data._key] = data;
                if (data._status == BuildingData.BuildingStatus.BUILD ||
                    data._status == BuildingData.BuildingStatus.UPGRADE)
                {
                    this.AddOneTimeListener(data);
                }
            }
        }
        ComputeEffects();
        this.SendNotification(NotiDefine.GenerateMyBuildingResp, this._datas);
    }

    public void DoSaveLocalBuildings()
    {
        //存储
        WorldBuildings script = new WorldBuildings();
        script.World = this._World;
        script.Datas = new List<BuildingData>();
        foreach (BuildingData data in this._datas.Values)
        {
            script.Datas.Add(data);
        }
        string fileName = UtilTools.combine(SaveFileDefine.WorldBuiding, this._World);
        CloudDataTool.SaveFile(fileName, script);
    }


    public Dictionary<string, BuildingData> GetAllBuilding()
    {
        return this._datas;
    }

    public BuildingData GetBuilding(string key)
    {
        BuildingData data = null;
        if (this._datas.TryGetValue(key, out data))
        {
            return data;
        }
        return null;
    }

    private BuildingEffectsData _Effects;
    private void ComputeEffects()
    {
        _Effects = new BuildingEffectsData();
        for (int i = (int)CareerDefine.Rider; i <= (int)CareerDefine.Count; ++i)
        {
            Dictionary<string, float> attrDic = new Dictionary<string, float>();
            attrDic[AttributeDefine.Attack] = 0;
            attrDic[AttributeDefine.Defense] = 0;
            attrDic[AttributeDefine.AtkSpeed] = 0;
            attrDic[AttributeDefine.Burst] = 0;
            _Effects.CareerAttrAdds[i] = attrDic;
        }

        _Effects.ElementAdds[ElementDefine.Fire] = 0;
        _Effects.ElementAdds[ElementDefine.Wind] = 0;
        _Effects.ElementAdds[ElementDefine.Water] = 0;

        _Effects.IncomeDic[ItemKey.gold] = 0;
        _Effects.IncomeDic[ItemKey.food] = 0;
        _Effects.IncomeDic[ItemKey.wood] = 0;
        _Effects.IncomeDic[ItemKey.metal] = 0;
        _Effects.IncomeDic[ItemKey.stone] = 0;


        Dictionary<string, float> dic = new Dictionary<string, float>();
        foreach (BuildingData data in this._datas.Values)
        {
            if (data._status == BuildingData.BuildingStatus.BUILD || data._status == BuildingData.BuildingStatus.INIT)
                continue;
            BuildingConfig config = BuildingConfig.Instance.GetData(data._id);
            BuildingUpgradeConfig configLevel = BuildingUpgradeConfig.GetConfig(data._id, data._level);
            this._Effects.PowerAdd += configLevel.Power;

            if (config.AddType.Equals(ValueAddType.AttributeAdd))
            {
                int curCareer = UtilTools.ParseInt(configLevel.AddValues[0]);
                Dictionary<string, float> attrDic = _Effects.CareerAttrAdds[curCareer];
                Dictionary<string, float> adds = AttributeData.InitAttributesBy(configLevel.AddValues[1]);
                foreach (string attr in adds.Keys)
                {
                    attrDic[attr] += adds[attr];
                }
            }//end if
            else if (config.AddType.Equals(ValueAddType.ElementAdd))
            {
                string curElement = configLevel.AddValues[0];
                float addValue = UtilTools.ParseFloat(configLevel.AddValues[1]);
                _Effects.ElementAdds[curElement] += addValue;
            }
            else if (config.AddType.Equals(ValueAddType.MarchSpeed))
            {
                float addValue = UtilTools.ParseFloat(configLevel.AddValues[0]);
                _Effects.MarchSpeedAdd += addValue;
            }
            else if (config.AddType.Equals(ValueAddType.HeroMaxBlood))
            {
                int addValue = UtilTools.ParseInt(configLevel.AddValues[0]);
                _Effects.MaxBloodAdd += addValue;
            }
            else if (config.AddType.Equals(ValueAddType.StoreLimit))
            {
                _Effects.ResLimitAdd += UtilTools.ParseInt(configLevel.AddValues[0]);
            }
            else if (config.AddType.Equals(ValueAddType.HourTax))
            {
                //税收
                CostData add = new CostData();
                add.Init(configLevel.AddValues[0]);
                _Effects.IncomeDic[add.id] += add.count;
            }
        }//end for

        //处理监听
        RoleProxy._instance.ComputeBuildingEffect();
        HeroProxy._instance.ComputeBuildingEffect();
    }

    public BuildingEffectsData GetBuildingEffects()
    {
        return this._Effects;
    }

    private void AddOneTimeListener(BuildingData data)
    {
        TimeCallData dataTime = new TimeCallData();
        dataTime._key = data._key;
        dataTime._notifaction = NotiDefine.BuildingExpireReachedNoti;
        dataTime.TimeStep = data._expireTime;
        dataTime._param = data._key;
        MediatorUtil.SendNotification(NotiDefine.AddTimestepCallback, dataTime);
    }

    public void Create(Dictionary<string, object> vo)
    {
        int id = (int)vo["configid"];
        int x = (int)vo["x"];
        int z = (int)vo["z"];

        BuildingData data = new BuildingData();
        data.Create(id, x, z);
        data.SetLevel(0);
        data.SetStatus(BuildingData.BuildingStatus.BUILD);
        this._datas.Add(data._key, data);

        //通知时间中心添加一个监听
        this.AddOneTimeListener(data);
       
        //通知Land创建完成
        MediatorUtil.SendNotification(NotiDefine.CreateOneBuildingResp, data);
        this.DoSaveLocalBuildings();
    }

    public void Upgrade(string key)
    {
        BuildingData data = this.GetBuilding(key);
        BuildingConfig _config = BuildingConfig.Instance.GetData(data._id);
        if (data == null || data._status != BuildingData.BuildingStatus.NORMAL || data._level >= _config.MaxLevel)
            return;
        data.SetStatus(BuildingData.BuildingStatus.UPGRADE);
        //通知时间中心添加一个监听
        this.AddOneTimeListener(data);
        MediatorUtil.SendNotification(NotiDefine.BuildingStatusChanged, key);
        this.DoSaveLocalBuildings();
    }

    public void SpeedUpUpgrade(string key)
    {
        BuildingData data = this.GetBuilding(key);
        if (data == null || data._status == BuildingData.BuildingStatus.NORMAL || data._status == BuildingData.BuildingStatus.BUILD)
            return;
        //时间中心去掉
        MediatorUtil.SendNotification(NotiDefine.RemoveTimestepCallback, key);

        data.SetStatus(BuildingData.BuildingStatus.NORMAL);
        data.SetLevel(data._level + 1);
        ComputeEffects();//计算影响
        this.DoSaveLocalBuildings();
        MediatorUtil.SendNotification(NotiDefine.BuildingStatusChanged, key);
    }
    public void CancelUpgrade(string key)
    {
        BuildingData data = this.GetBuilding(key);
        if (data == null || data._status == BuildingData.BuildingStatus.NORMAL)
            return;
        //时间中心去掉
        MediatorUtil.SendNotification(NotiDefine.RemoveTimestepCallback, key);

        if (data._status == BuildingData.BuildingStatus.BUILD)
        {
            //删除取消
            this._datas.Remove(key);
            MediatorUtil.SendNotification(NotiDefine.BuildingRemoveNoti, key);
        }
        else if (data._status == BuildingData.BuildingStatus.UPGRADE)
        {
            //删除取消
            data.SetStatus(BuildingData.BuildingStatus.NORMAL);
            MediatorUtil.SendNotification(NotiDefine.BuildingStatusChanged, key);
        }
        this.DoSaveLocalBuildings();
    }

    public void Relocate(Dictionary<string, object> vo)
    {
        string key = (string)vo["key"];
        int x = (int)vo["x"];
        int z = (int)vo["z"];
        BuildingData data = this.GetBuilding(key);
        if (data != null)
        {
            data.SetCordinate(x, z);
            MediatorUtil.SendNotification(NotiDefine.BuildingRelocateResp, key);
        }
        this.DoSaveLocalBuildings();
    }

    public void OnBuildExpireFinsih(string key)
    {
        BuildingData data = this.GetBuilding(key);
        if (data == null)
            return;
        if (data._status == BuildingData.BuildingStatus.UPGRADE ||
            data._status == BuildingData.BuildingStatus.BUILD)
        {
            data.SetLevel(data._level + 1);//升级完成
            ComputeEffects();//计算影响
        }
        
        data.SetStatus(BuildingData.BuildingStatus.NORMAL);
        this.SendNotification(NotiDefine.BuildingStatusChanged, key);
        this.DoSaveLocalBuildings();
    }


}//end class
