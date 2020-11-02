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
    public static int GateID = 23;
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
        int halfRange = this._Effects.BuildRange / 2;//必须为基数的格子数
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

            //构造一个城门
            BuildingData wallData = new BuildingData();
            config = BuildingConfig.Instance.GetData(BuildingData.GateID);
            BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(config.ID, 1);
            int range = UtilTools.ParseInt(configLv.AddValues[0]);
            posx = range / 2 + 1;
            posz = -1;
            wallData.Create(BuildingData.GateID, posx, posz);
            wallData.SetLevel(1);
            wallData.SetStatus(BuildingData.BuildingStatus.NORMAL);
            this._WorldData.Datas.Add(wallData);

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

    public VInt2 GetBuildingMaxAndLimitCount(int id)
    {
        VInt2 kv = new VInt2();
        BuildingConfig config = BuildingConfig.Instance.GetData(id);
        int len = config.Condition.Length;
        for (int i = 0; i < len; ++i)
        {
            string[] firstlist = config.Condition[i].Split('|');
            int bdid = UtilTools.ParseInt(firstlist[0]);
            int level = UtilTools.ParseInt(firstlist[1]);
            int bdCount = UtilTools.ParseInt(firstlist[2]);
            BuildingData data = this.GetTopLevelBuilding(bdid);
            if (data != null && data._level >= level)
            {
                kv.x = bdCount;
            }
            
            if (bdCount > kv.y)
            {
                kv.y = bdCount;
            }
        }
        return kv;
    }

    public VInt2 GetBuildingNextOpenCondition(int id)
    {
        VInt2 needkv = new VInt2();
        VInt2 kv = this.GetBuildingMaxAndLimitCount(id);
        BuildingConfig config = BuildingConfig.Instance.GetData(id);
        int max = kv.x;
        int len = config.Condition.Length;
        for (int i = 0; i < len; ++i)
        {
            string[] firstlist = config.Condition[i].Split('|');
            int bdid = UtilTools.ParseInt(firstlist[0]);
            int level = UtilTools.ParseInt(firstlist[1]);
            int bdCount = UtilTools.ParseInt(firstlist[2]);
            if (bdCount > max)
            {
                needkv.x = bdid;
                needkv.y = level;
                break;
            }
        }//end for
        return needkv;
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

    public bool IsUpgradeConditonStaisfy(string key)
    {
        BuildingData bd = WorldProxy._instance.GetBuilding(key);
        BuildingUpgradeConfig configNext = BuildingUpgradeConfig.GetConfig(bd._id, bd._level+1);
        if (configNext == null)
            return false;
        if (configNext.Condition.Length == 0)
            return true;
        BuildingData bdNeed = WorldProxy._instance.GetTopLevelBuilding(configNext.Condition[0]);
        return bdNeed != null && bdNeed._level >= configNext.Condition[1];
    }

    public BuildingData GetFirstBuilding(int id)
    {
        int count = this._WorldData.Datas.Count;
        for (int i = 0; i < count; ++i)
        {
            if (this._WorldData.Datas[i]._id.Equals(id))
                return this._WorldData.Datas[i];
        }
        return null;
    }

    public BuildingData GetTopLevelBuilding(int id)
    {
        BuildingData data = null;
        int curLevel = 0;
        int count = this._WorldData.Datas.Count;
        for (int i = 0; i < count; ++i)
        {
            if (this._WorldData.Datas[i]._id.Equals(id) && this._WorldData.Datas[i]._level > curLevel)
            {
                data = this._WorldData.Datas[i];
                curLevel = data._level;
            }
        }
        return data;
    }

    private BuildingEffectsData _Effects;
    private void ComputeEffects()
    {
        if (this._Effects == null)
            _Effects = new BuildingEffectsData();
        int oldRange = this._Effects.BuildRange;
        this._Effects.Reset();

        int oldRecruit = this._Effects.HeroRectuitLimit;

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

            else if (config.AddType.Equals(ValueAddType.HeroMaxBlood))
            {
                int addValue = UtilTools.ParseInt(configLevel.AddValues[0]);
                _Effects.MaxBloodAdd += addValue;
            }
            else if (config.AddType.Equals(ValueAddType.StoreLimit))
            {
                _Effects.ResLimitAdd += UtilTools.ParseInt(configLevel.AddValues[0]);
            }
            else if (config.AddType.Equals(ValueAddType.RecruitVolume))
            {
                int career = UtilTools.ParseInt(configLevel.AddValues[0]);
                int volume = UtilTools.ParseInt(configLevel.AddValues[1]);
                this._Effects.RecruitVolume[career] = volume;
            }
            else if (config.AddType.Equals(ValueAddType.CityTroop))
            {
                _Effects.ArmyLimit = UtilTools.ParseInt(configLevel.AddValues[0]);
                _Effects.TroopNum = UtilTools.ParseInt(configLevel.AddValues[1]);
            }
            else if (config.AddType.Equals(ValueAddType.HeroRecruit))
            {
                _Effects.HeroRectuitLimit = UtilTools.ParseInt(configLevel.AddValues[0]);
            }
            else if (config.AddType.Equals(ValueAddType.HourTax))
            {
                //税收
                CostData add = new CostData();
                add.Init(configLevel.AddValues[0]);
                int limit = UtilTools.ParseInt(configLevel.AddValues[1]);
                _Effects.IncomeDic[add.id].Count += add.count;
                _Effects.IncomeDic[add.id].StoreLimit = limit;
            }
            else if (config.AddType.Equals(ValueAddType.BuildRange))
            {
                this._Effects.BuildRange = UtilTools.ParseInt(configLevel.AddValues[0]);
            }
            else if (config.AddType.Equals(ValueAddType.RecruitSecs))
            {
                this._Effects.RecruitReduceRate = UtilTools.ParseFloat(configLevel.AddValues[0]);
            }
        }//end for

        //处理监听
        RoleProxy._instance.ComputeBuildingEffect();
        HeroProxy._instance.ComputeBuildingEffect();

       

        //存储可以建筑的范围
        this.UpdateCanBuildSpot();
        if (oldRange != this._Effects.BuildRange && oldRange > 0)
        {
            BuildingData data = this.GetFirstBuilding(BuildingData.GateID);
            if (data._id == BuildingData.GateID)
            {
                BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(data._id, data._level);
                int range = UtilTools.ParseInt(configLv.AddValues[0]);
                int posx = range / 2 + 1;
                int posz = -1;
                data.SetCordinate(posx, posz);
                MediatorUtil.SendNotification(NotiDefine.BuildingRelocateResp, data._key);
                this.DoSaveWorldDatas();
            }
            this.SendNotification(NotiDefine.HomeRangeChanged);
        }
            
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


        BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(id, 1);
        bool isCostEnough = RoleProxy._instance.TryDeductCost(configLv.Cost);
        if (isCostEnough == false)
            return;
        

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

        BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(data._id, 1);
        bool isCostEnough = RoleProxy._instance.TryDeductCost(configLv.Cost);
        if (isCostEnough == false)
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
            data.SetStatus(BuildingData.BuildingStatus.NORMAL);
            ComputeEffects();//计算影响
        }
        
        this.SendNotification(NotiDefine.BuildingStatusChanged, key);
        this.DoSaveWorldDatas();
    }

    public List<StringKeyValue> GetAddOnDesc(int id, int level = 1,bool needExtra = false)
    {
        List<StringKeyValue> list = new List<StringKeyValue>();

        BuildingConfig config = BuildingConfig.Instance.GetData(id);
        string AddType = config.AddType;
        if (AddType.Equals(""))
            return list;
        BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(id, level);

      
        string[] AddValues = configLv.AddValues;
        int len = AddValues.Length;
        if (len == 0)
            return list;

        StringKeyValue kv;
        if (ValueAddType.CityTroop.Equals(AddType))
        {
            kv = new StringKeyValue();
            kv.key = config.AddDescs[0];
            kv.value = AddValues[0];
            list.Add(kv);

            kv = new StringKeyValue();
            kv.key = config.AddDescs[1];
            kv.value = AddValues[1];
            list.Add(kv);
        }
        else if (ValueAddType.HourTax.Equals(AddType))
        {
            CostData cost = new CostData();
            cost.Init(AddValues[0]);
            ItemInfoConfig configItem = ItemInfoConfig.Instance.GetData(cost.id);
            int currenValue = RoleProxy._instance.GetHourAwardValue(cost.id);


            kv = new StringKeyValue();
            kv.key = UtilTools.format(config.AddDescs[0], configItem.Name);
            kv.value = cost.count.ToString();
            list.Add(kv);

            kv = new StringKeyValue();
            kv.key = UtilTools.format(config.AddDescs[1], configItem.Name);
            int storeLimit = UtilTools.ParseInt(AddValues[1]);

            if (needExtra)
            {
                if (currenValue >= storeLimit)
                    kv.value = LanguageConfig.GetLanguage(LanMainDefine.ResCurrentFull, storeLimit, storeLimit);
                else
                    kv.value = LanguageConfig.GetLanguage(LanMainDefine.ResCurrent, storeLimit, currenValue);
            }
            else
            {
                kv.value = storeLimit.ToString();
            }

            list.Add(kv);
        }
        else if (ValueAddType.StoreLimit.Equals(AddType) ||
            ValueAddType.HeroMaxBlood.Equals(AddType) ||
            ValueAddType.Equipment.Equals(AddType) ||
            ValueAddType.Cure.Equals(AddType) ||
            ValueAddType.DeployCount.Equals(AddType) ||
            ValueAddType.HeroRecruit.Equals(AddType) ||
            ValueAddType.Worker.Equals(AddType))
        {
            kv = new StringKeyValue();
            kv.key = config.AddDescs[0];
            kv.value = AddValues[0];
            list.Add(kv);
        }
        else if (ValueAddType.ResearchSpeed.Equals(AddType) ||
            ValueAddType.RecruitSecs.Equals(AddType))
        {
            kv = new StringKeyValue();
            kv.key = config.AddDescs[0];
            kv.value = LanguageConfig.GetLanguage(LanMainDefine.Percent, AddValues[0]);
            list.Add(kv);
        }
        else if (ValueAddType.RecruitVolume.Equals(AddType))
        {
            kv = new StringKeyValue();
            kv.key = config.AddDescs[0];
            kv.value = AddValues[1];
            list.Add(kv);
        }
        else if (ValueAddType.ElementAdd.Equals(AddType))
        {
            string[] adds = AddValues[0].Split(':');
            string attrName = LanguageConfig.GetLanguage(adds[0]);
            kv = new StringKeyValue();
            kv.key = UtilTools.format(config.AddDescs[0], attrName);
            kv.value = adds[1];
            list.Add(kv);
        }
        else if (ValueAddType.BuildRange.Equals(AddType))
        {
            kv = new StringKeyValue();
            kv.key = config.AddDescs[0];
            kv.value = LanguageConfig.GetLanguage(LanMainDefine.BuildRange, AddValues[0], AddValues[0]);
            list.Add(kv);
        }

        return list;
    }
}//end class
