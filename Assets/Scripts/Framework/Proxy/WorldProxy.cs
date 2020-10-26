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
    
    private List<string> _canOperateSpots = new List<string>();//我可以操作的地块
    private WorldBuildings _WorldData;
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
        foreach (BuildingData data in this._WorldData.Datas)
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
        foreach (BuildingData data in this._WorldData.Datas)
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
        this.GenerateAllBuilding(world);
        this.SendNotification(NotiDefine.GenerateMySpotResp);
    }//end func

    private void UpdateCanBuildSpot()
    {
        this._canOperateSpots.Clear();
        int halfRange = this._Effects.BuildRange / 2;
        for (int row = -halfRange; row <= halfRange; ++row)
        {
            int corX = row;
            for (int col = -halfRange; col <= halfRange; ++col)
            {
                int corZ = col;
                string key = UtilTools.combine(corX, "|", corZ);
                this._canOperateSpots.Add(key);
            }
        }//end for
    }


    public void GenerateAllBuilding(int world)
    {
        string fileName = UtilTools.combine(SaveFileDefine.WorldBuiding, world);
        string json = CloudDataTool.LoadFile(fileName);
        this._WorldData = new WorldBuildings();
        _WorldData.World = world;
        if (json.Equals(string.Empty))
        {
            //构造一个主城
            BuildingData mainCity = new BuildingData();
            BuildingConfig config = BuildingConfig.Instance.GetData(BuildingData.MainCityID);
            int posx = (config.RowCount - 1) / 2;
            int posz = (config.ColCount - 1) / 2;
            _WorldData.StartCordinates = new VInt2();
            _WorldData.StartCordinates.x =0;
            _WorldData.StartCordinates.y = 0;
            mainCity.Create(BuildingData.MainCityID,-posx, -posz);
            mainCity.SetLevel(1);
            mainCity.SetStatus(BuildingData.BuildingStatus.NORMAL);
            this._WorldData.Datas = new List<BuildingData>();
            this._WorldData.Datas.Add(mainCity);
            _WorldData.MainCityKey = mainCity._key;
            this.DoSaveWorldDatas();
        }
        else
        {
            _WorldData = Newtonsoft.Json.JsonConvert.DeserializeObject<WorldBuildings>(json);
            foreach (BuildingData data in _WorldData.Datas)
            {
                if (data._status == BuildingData.BuildingStatus.BUILD ||
                    data._status == BuildingData.BuildingStatus.UPGRADE)
                {
                    this.AddOneTimeListener(data);
                }
            }
        }
        ComputeEffects();
    }

    public void DoSaveWorldDatas()
    {
        //存储
        string fileName = UtilTools.combine(SaveFileDefine.WorldBuiding, this._World);
        CloudDataTool.SaveFile(fileName, _WorldData);
    }


    public List<BuildingData> GetAllBuilding()
    {
        return this._WorldData.Datas;
    }

    public WorldBuildings Data => this._WorldData;

     
    public BuildingData GetBuilding(string key)
    {
        int count = this._WorldData.Datas.Count;
        for (int i = 0; i < count; ++i)
        {
            if (this._WorldData.Datas[i]._key.Equals(key))
                return this._WorldData.Datas[i];
        }
        return null;
    }

    private BuildingEffectsData _Effects;
    private void ComputeEffects()
    {
        if (this._Effects == null)
            _Effects = new BuildingEffectsData();
        int oldRange = this._Effects.BuildRange;
        this._Effects.Reset();

        Dictionary<string, float> dic = new Dictionary<string, float>();
        foreach (BuildingData data in this._WorldData.Datas)
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
                string[] list = configLevel.AddValues[0].Split(':');
                string curElement = list[0];
                float addValue = UtilTools.ParseFloat(list[1]);
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
            else if (config.AddType.Equals(ValueAddType.BuildRange))
            {
                this._Effects.BuildRange = UtilTools.ParseInt(configLevel.AddValues[0]);
            }
        }//end for

        //处理监听
        RoleProxy._instance.ComputeBuildingEffect();
        HeroProxy._instance.ComputeBuildingEffect();
        //存储可以建筑的范围
        this.UpdateCanBuildSpot();
        if (oldRange != this._Effects.BuildRange && oldRange > 0)
            this.SendNotification(NotiDefine.HomeRangeChanged);
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
        this._WorldData.Datas.Add(data);
  
        //通知时间中心添加一个监听
        this.AddOneTimeListener(data);
       
        //通知Land创建完成
        MediatorUtil.SendNotification(NotiDefine.CreateOneBuildingResp, data);
        this.DoSaveWorldDatas();
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
        this.DoSaveWorldDatas();
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
        this.DoSaveWorldDatas();
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
            this._WorldData.Datas.Remove(data);
            MediatorUtil.SendNotification(NotiDefine.BuildingRemoveNoti, key);
        }
        else if (data._status == BuildingData.BuildingStatus.UPGRADE)
        {
            //删除取消
            data.SetStatus(BuildingData.BuildingStatus.NORMAL);
            MediatorUtil.SendNotification(NotiDefine.BuildingStatusChanged, key);
        }
        this.DoSaveWorldDatas();
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
        this.DoSaveWorldDatas();
    }

    public void OnBuildExpireFinsih(string key)
    {
        BuildingData data = this.GetBuilding(key);
        if (data == null)
            return;

        BuildingConfig config = BuildingConfig.Instance.GetData(data._id);
        if (data._level == config.MaxLevel)
            return;

        if (data._status == BuildingData.BuildingStatus.UPGRADE ||
        data._status == BuildingData.BuildingStatus.BUILD)
        {
            data.SetLevel(data._level + 1);//升级完成
            ComputeEffects();//计算影响
        }
        
        data.SetStatus(BuildingData.BuildingStatus.NORMAL);
        this.SendNotification(NotiDefine.BuildingStatusChanged, key);
        this.DoSaveWorldDatas();
    }
}//end class
