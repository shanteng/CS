using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;


//时间戳回调管理
public class WorldProxy : BaseRemoteProxy
{
    public static WorldProxy _instance;
    public static WorldConfig _config;
    private int _World;

    private List<string> _canOperateSpots = new List<string>();//我可以操作的地块
    private Dictionary<string, VInt2> _VisibleSpots = new Dictionary<string, VInt2>();//我可以操作的地块
    private WorldBuildings _WorldData;
    private Dictionary<string, PatrolData> _OutPatrolDic = new Dictionary<string, PatrolData>();

    private Dictionary<int, BuildingEffectsData> _CityEffects = new Dictionary<int, BuildingEffectsData>();
    public WorldProxy() : base(ProxyNameDefine.WORLD)
    {
        _instance = this;
    }

    public List<string> GetCanOperateSpots()
    {
        return this._canOperateSpots;
    }

    public Dictionary<string, VInt2> GetVisibleSpots()
    {
        return this._VisibleSpots;
    }


    public int GetBuildingCount(int id, int city = 0)
    {
        List<BuildingData> list = this.GetCityBuildings(city);
        int count = 0;
        foreach (BuildingData data in list)
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
        this._OutPatrolDic.Clear();
        WorldProxy._config = WorldConfig.Instance.GetData(this._World);
        GameIndex.ROW = WorldProxy._config.MaxRowCount;
        GameIndex.COL = WorldProxy._config.MaxColCount;
        this.GenerateAllBuilding(world);
        this.SendNotification(NotiDefine.GenerateMySpotResp);
    }//end func

    public VInt2 GetCityCordinate(int cityid)
    {
        if (cityid == 0)
            return new VInt2();
        return new VInt2();//暂时写死，之后读取配置
    }

    private void ComputeCityVisibleSpot()
    {
        //城市
        List<VInt2> addList = new List<VInt2>();
        foreach (BuildingEffectsData Effect in this._CityEffects.Values)
        {
            if (Effect.VisibleRangeData == null)
                continue;
            VInt2 centerPos = this.GetCityCordinate(Effect.VisibleRangeData.CityID);
            int halfRange = Effect.VisibleRangeData.Range;
            int startX = centerPos.x - halfRange;
            int endX = centerPos.x + halfRange;

            int startZ = centerPos.y - halfRange;
            int endZ = centerPos.y + halfRange;

            for (int row = startX; row <= endX; ++row)
            {
                int corX = row;
                for (int col = startZ; col <= endZ; ++col)
                {
                    int corZ = col;
                    string key = UtilTools.combine(corX, "|", corZ);
                    if (this._VisibleSpots.ContainsKey(key) == false)
                    {
                        VInt2 kv = new VInt2(corX, corZ);
                        addList.Add(kv);
                    }
                }//end for col
            }//end for row
        }//end foreach

        if (addList.Count > 0)
        {
            this.AddVisibleSpots(addList);
        }
    }

    public bool IsSpotVisible(int x, int z)
    {
        string key = UtilTools.combine(x, "|", z);
        return _VisibleSpots.ContainsKey(key);
    }

    public void AddVisibleSpots(List<VInt2> addList)
    {
        Dictionary<string, VInt2> NewAddVisibleList = new Dictionary<string, VInt2>();
        foreach (VInt2 pos in addList)
        {
            string key = UtilTools.combine(pos.x, "|", pos.y);
            if (this._VisibleSpots.ContainsKey(key) == false)
            {
                VInt2 kv = new VInt2(pos.x, pos.y);
                NewAddVisibleList.Add(key, kv);
                this._VisibleSpots.Add(key, kv);
            }
        }

        if (NewAddVisibleList.Count > 0)
        {
            this.SendNotification(NotiDefine.LandVisibleChanged, NewAddVisibleList);
            this.DoSaveVisibleSpot();
        }
    }


    private void UpdateCanBuildSpot()
    {
        BuildingEffectsData Effect = this.GetBuildingEffects(0);
        this._canOperateSpots.Clear();
        int halfRange = Effect.BuildRange / 2;//必须为基数的格子数
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

    public void LoadVisibleSpot()
    {
        string json = CloudDataTool.LoadFile(SaveFileDefine.VisibleSpot);
        if (json.Equals(string.Empty) == false)
        {
            this._VisibleSpots = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, VInt2>>(json);
        }
        else
        {
            this._VisibleSpots = new Dictionary<string, VInt2>();
            this.ComputeCityVisibleSpot();
        }
    }

    public void LoadPatrol()
    {
        string json = CloudDataTool.LoadFile(SaveFileDefine.Patrol);
        if (json.Equals(string.Empty) == false)
        {
            this._OutPatrolDic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, PatrolData>>(json);
        }

        List<string> finishs = new List<string>();
        var it = this._OutPatrolDic.Values.GetEnumerator();
        while (it.MoveNext())
        {
            PatrolData data = it.Current;
            if (GameIndex.ServerTime >= data.ExpireTime)
            {
                finishs.Add(data.ID);
            }
            else
            {
                this.PatrolAction(data);
            }
        }
        it.Dispose();

        if (finishs.Count > 0)
        {
            foreach (string id in finishs)
            {
                this.OnPatrolExpireFinsih(id, false);
            }
            this.DoSaveOutPatrol();
        }
           
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
            this._WorldData.Datas = new Dictionary<int, List<BuildingData>>();

            BuildingData mainCity = new BuildingData();
            BuildingConfig config = BuildingConfig.Instance.GetData(BuildingData.MainCityID);
            int posx = (config.RowCount - 1) / 2;
            int posz = (config.ColCount - 1) / 2;
            _WorldData.StartCordinates = new VInt2();
            _WorldData.StartCordinates.x = 0;
            _WorldData.StartCordinates.y = 0;
            mainCity.Create(BuildingData.MainCityID, -posx, -posz);
            mainCity.SetLevel(1);
            mainCity.SetStatus(BuildingData.BuildingStatus.NORMAL);

            List<BuildingData> datas = new List<BuildingData>();
            datas.Add(mainCity);

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
            datas.Add(wallData);

            this._WorldData.Datas.Add(0, datas);

            this.DoSaveWorldDatas();
        }
        else
        {
            _WorldData = Newtonsoft.Json.JsonConvert.DeserializeObject<WorldBuildings>(json);
            foreach (List<BuildingData> datas in _WorldData.Datas.Values)
            {
                foreach (BuildingData data in datas)
                {
                    if (data._status == BuildingData.BuildingStatus.BUILD ||
                  data._status == BuildingData.BuildingStatus.UPGRADE)
                    {
                        this.AddOneTimeListener(data);
                    }
                }
            }
        }
        this.LoadVisibleSpot();//读取可视区域
        this.LoadPatrol();
        ComputeEffects();
    }

    public void DoSaveWorldDatas()
    {
        //存储
        string fileName = UtilTools.combine(SaveFileDefine.WorldBuiding, this._World);
        CloudDataTool.SaveFile(fileName, _WorldData);
    }

    public void DoSaveVisibleSpot()
    {
        //存储
        CloudDataTool.SaveFile(SaveFileDefine.VisibleSpot, this._VisibleSpots);
    }

    public void DoSaveOutPatrol()
    {
        //存储
        CloudDataTool.SaveFile(SaveFileDefine.Patrol, this._OutPatrolDic);
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

    public BuildingData GetBuildingInRange(int x, int z)
    {

        foreach (List<BuildingData> datas in this._WorldData.Datas.Values)
        {
            foreach (BuildingData bd in datas)
            {
                bool isIn = this.IsInBulildOccupy(bd, x, z);
                if (isIn)
                    return bd;
            }
        }
        return null;
    }

    public bool IsInBulildOccupy(BuildingData bd, int x, int z)
    {
        foreach (VInt2 occupy in bd._occupyCordinates)
        {
            if (occupy.x == x && occupy.y == z)
                return true;
        }
        return false;
    }

    public BuildingData GetBuilding(string key, int cityid = 0)
    {
        List<BuildingData> list = this.GetCityBuildings(cityid);
        int count = list.Count;
        for (int i = 0; i < count; ++i)
        {
            if (list[i]._key.Equals(key))
                return list[i];
        }
        return null;
    }

    public bool IsUpgradeConditonStaisfy(string key)
    {
        BuildingData bd = WorldProxy._instance.GetBuilding(key);
        BuildingUpgradeConfig configNext = BuildingUpgradeConfig.GetConfig(bd._id, bd._level + 1);
        if (configNext == null)
            return false;
        if (configNext.Condition.Length == 0)
            return true;
        BuildingData bdNeed = WorldProxy._instance.GetTopLevelBuilding(configNext.Condition[0]);
        return bdNeed != null && bdNeed._level >= configNext.Condition[1];
    }

    public List<BuildingData> GetCityBuildings(int city = 0)
    {
        List<BuildingData> list;
        if (this._WorldData.Datas.TryGetValue(city, out list))
            return list;
        return new List<BuildingData>();
    }

    public BuildingData GetFirstBuilding(int id, int cityid = 0)
    {
        List<BuildingData> list = this.GetCityBuildings(cityid);
        int count = list.Count;
        for (int i = 0; i < count; ++i)
        {
            if (list[i]._id.Equals(id))
                return list[i];
        }
        return null;
    }

    public BuildingData GetTopLevelBuilding(int id, int cityid = 0)
    {
        List<BuildingData> list = this.GetCityBuildings(cityid);

        BuildingData data = null;
        int curLevel = 0;
        int count = list.Count;
        for (int i = 0; i < count; ++i)
        {
            if (list[i]._id.Equals(id) && list[i]._level > curLevel)
            {
                data = list[i];
                curLevel = data._level;
            }
        }
        return data;
    }

    private void ComputeCityEffects(int city)
    {
        BuildingEffectsData Effect;
        if (this._CityEffects.TryGetValue(city, out Effect) == false)
        {
            Effect = new BuildingEffectsData();
            this._CityEffects[city] = Effect;
        }

        int oldRange = Effect.BuildRange;

        Effect.Reset();


        List<BuildingData> buildings = this.GetCityBuildings(city);
        foreach (BuildingData data in buildings)
        {
            if (data._status == BuildingData.BuildingStatus.BUILD || data._status == BuildingData.BuildingStatus.INIT)
                continue;
            BuildingConfig config = BuildingConfig.Instance.GetData(data._id);
            BuildingUpgradeConfig configLevel = BuildingUpgradeConfig.GetConfig(data._id, data._level);
            Effect.PowerAdd += configLevel.Power;

            if (config.AddType.Equals(ValueAddType.AttributeAdd))
            {
                int curCareer = UtilTools.ParseInt(configLevel.AddValues[0]);
                Dictionary<string, float> attrDic = Effect.CareerAttrAdds[curCareer];
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
                Effect.ElementAdds[curElement] += addValue;
            }

            else if (config.AddType.Equals(ValueAddType.HeroMaxBlood))
            {
                int addValue = UtilTools.ParseInt(configLevel.AddValues[0]);
                Effect.MaxBloodAdd += addValue;
            }
            else if (config.AddType.Equals(ValueAddType.Patrol))
            {
                Effect.PatrolMax += UtilTools.ParseInt(configLevel.AddValues[0]);
            }
            else if (config.AddType.Equals(ValueAddType.RecruitVolume))
            {
                int career = UtilTools.ParseInt(configLevel.AddValues[0]);
                int volume = UtilTools.ParseInt(configLevel.AddValues[1]);
                Effect.RecruitVolume[career] = volume;
            }
            else if (config.AddType.Equals(ValueAddType.CityTroop))
            {
                Effect.DayBoxLimit = UtilTools.ParseInt(configLevel.AddValues[0]);
                Effect.TroopNum = UtilTools.ParseInt(configLevel.AddValues[1]);
            }
            else if (config.AddType.Equals(ValueAddType.HeroRecruit))
            {
                Effect.HeroRectuitLimit = UtilTools.ParseInt(configLevel.AddValues[0]);
            }
            else if (config.AddType.Equals(ValueAddType.HourTax))
            {
                //税收
                CostData add = new CostData();
                add.Init(configLevel.AddValues[0]);
                int limit = UtilTools.ParseInt(configLevel.AddValues[1]);
                Effect.IncomeDic[add.id].Count += add.count;
                Effect.IncomeDic[add.id].LimitVolume += limit;
            }
            else if (config.AddType.Equals(ValueAddType.BuildRange))
            {
                Effect.BuildRange = UtilTools.ParseInt(configLevel.AddValues[0]);
                Effect.VisibleRangeData.CityID = city;
                Effect.VisibleRangeData.Range = UtilTools.ParseInt(configLevel.AddValues[1]);
            }
            else if (config.AddType.Equals(ValueAddType.RecruitSecs))
            {
                Effect.RecruitReduceRate = UtilTools.ParseFloat(configLevel.AddValues[0]);
            }
        }//end for

        if (oldRange != Effect.BuildRange && oldRange > 0)
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
            this.SendNotification(NotiDefine.HomeRangeChanged, city);
        }

        //存储可以建筑的范围
        if (city == 0)
            this.UpdateCanBuildSpot();

        HeroProxy._instance.ComputeBuildingEffect(city);
        TeamProxy._instance.ComputeBuildingEffect(city);

    }//end class

    private void ComputeEffects(int city = -1)
    {
        if (city < 0)
        {
            foreach (int cityid in this._WorldData.Datas.Keys)
            {
                this.ComputeCityEffects(cityid);
            }//end for
        }
        else
        {
            this.ComputeCityEffects(city);
        }

        this.ComputeCityVisibleSpot();
        //处理监听
        RoleProxy._instance.ComputeBuildingEffect();

    }

    public Dictionary<int, BuildingEffectsData> AllEffects => this._CityEffects;

    public BuildingEffectsData GetBuildingEffects(int cityid)
    {
        BuildingEffectsData effect;
        this._CityEffects.TryGetValue(cityid, out effect);
        //根据
        return effect;
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


    public VInt2 GetCityPatrolInfo(int cityid)
    {
        VInt2 kv = new VInt2();
        BuildingEffectsData Effect = this.GetBuildingEffects(cityid);
        if (Effect == null)
            return kv;
        foreach (PatrolData data in this._OutPatrolDic.Values)
        {
            if (data.FromCIty == cityid)
                kv.x++;
        }
        kv.y = Effect.PatrolMax;
        return kv;
    }

    public long GetMoveExpireTime(int fromX, int fromZ, int targetX, int targetZ, float deltaSecs)
    {
        int descX = fromX - targetX;
        int descY = fromZ - targetZ;
        float distance = Mathf.Sqrt(descX * descX + descY * descY);
        long expireSces = GameIndex.ServerTime + Mathf.CeilToInt(deltaSecs * distance);
        return expireSces;
    }

    public bool CanMoveTo(int x, int z)
    {
        int halfRange = 1;
        int startX = x - halfRange;
        int endX = x + halfRange;

        int startZ = z - halfRange;
        int endZ = z + halfRange;

        for (int row = startX; row <= endX; ++row)
        {
            int corX = row;
            for (int col = startZ; col <= endZ; ++col)
            {
                int corZ = col;
                string key = UtilTools.combine(corX, "|", corZ);
                if (this._VisibleSpots.ContainsKey(key))
                {
                    return true;
                }
            }//end for col
        }//end for row

        return false;
    }

    public void DoPatrol(Dictionary<string, object> vo)
    {
        int city = (int)vo["city"];
        int x = (int)vo["x"];
        int z = (int)vo["z"];

        bool canMove = this.CanMoveTo(x, z);
        VInt2 gamePos = UtilTools.WorldToGameCordinate(x, z);
        if (canMove == false)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.NoVisibleNoPatrol, gamePos.x, gamePos.y);
            return;
        }

        bool isVisible = this.IsSpotVisible(x, z);
        if (isVisible)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.IsVisibleNoPatrol, gamePos.x, gamePos.y);
            return;
        }

        VInt2 kv = this.GetCityPatrolInfo(city);
        if (kv.x >= kv.y)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.NoPatrol);
            return;
        }

        bool hasSendYet = false;
        foreach (PatrolData data in this._OutPatrolDic.Values)
        {
            if (data.Target.x == x  && data.Target.y == z)
            {
                hasSendYet = true;
                break;
            }
        }

        if (hasSendYet)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.HasSendPatrol, gamePos.x, gamePos.y);
            return;
        }


        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.PatrolDeltaSces);
        int SecsDelta = cfgconst.IntValues[0];

        VInt2 cityPos = this.GetCityCordinate(city);
        PatrolData patrol = new PatrolData();
        patrol.ID = UtilTools.GenerateUId();
        patrol.FromCIty = city;
        patrol.Start = this.GetCityCordinate(city);
        patrol.Target.x = x;
        patrol.Target.y = z;
        patrol.StartTime = GameIndex.ServerTime;
        patrol.ExpireTime = this.GetMoveExpireTime(cityPos.x, cityPos.y, x, z, 1);
        patrol.Range = 2;
        this._OutPatrolDic.Add(patrol.ID, patrol);

        this.PatrolAction(patrol);
        PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.DoPatrol, gamePos.x, gamePos.y));
        this.SendNotification(NotiDefine.PatrolResp, patrol);
        this.DoSaveOutPatrol();
    }

    private void PatrolAction(PatrolData patrol)
    {
        //测试
     //   patrol.ExpireTime = GameIndex.ServerTime + 20;
        //路线添加
        PathData path = new PathData();
        path.ID = patrol.ID;
        path.Type = PathData.TYPE_PATROL;
        path.Start = patrol.Start;
        path.Target = patrol.Target;
        path.StartTime = patrol.StartTime;
        path.ExpireTime = patrol.ExpireTime;
        path.Model = "PathModel";
        path.Param = patrol;
        PathProxy._instance.AddPath(path);

        //时间监听
        TimeCallData dataTime = new TimeCallData();
        dataTime._key = patrol.ID;
        dataTime._notifaction = NotiDefine.PatrolExpireReachedNoti;
        dataTime.TimeStep = patrol.ExpireTime;
        dataTime._param = patrol.ID;
        MediatorUtil.SendNotification(NotiDefine.AddTimestepCallback, dataTime);
        this.DoSaveOutPatrol();
    }

    public void OnPatrolExpireFinsih(string key,bool needSave = true)
    {
        PatrolData data;
        if (this._OutPatrolDic.TryGetValue(key,out data) == false)
            return;
        //将新探索的点设置为可见
        List<VInt2> addList = new List<VInt2>();
        //根据科技计算范围，先写死当前坐标

        int halfRange = data.Range;
        for (int row = -halfRange; row <= halfRange; ++row)
        {
            int corX = data.Target.x + row;
            for (int col = -halfRange; col <= halfRange; ++col)
            {
                int corZ = data.Target.y + col;
                bool isVisible = WorldProxy._instance.IsSpotVisible(corX, corZ);
                if (isVisible == false)
                {
                    VInt2 kv = new VInt2(corX, corZ);
                    addList.Add(kv);
                }
            }
        }//end for

        this.AddVisibleSpots(addList);

        VInt2 gamePos = UtilTools.WorldToGameCordinate(data.Target.x, data.Target.y);
       
        this.SendNotification(NotiDefine.PatrolFinishNoti, data.Target);
        this._OutPatrolDic.Remove(key);
        PathProxy._instance.RemovePath(key);
        if (needSave)
        {
            PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.FinishPatrol, gamePos.x, gamePos.y));
            this.DoSaveOutPatrol();
        }
           
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

        List<BuildingData> maincitylist = this.GetCityBuildings(0);
        maincitylist.Add(data);
     
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

    public void OnSpeedSure(object param)
    {
        string key = (string)param;
        BuildingData data = this.GetBuilding(key);
        if (data == null || data._status == BuildingData.BuildingStatus.NORMAL || data._status == BuildingData.BuildingStatus.BUILD)
            return;
        //时间中心去掉
        MediatorUtil.SendNotification(NotiDefine.RemoveTimestepCallback, key);

        data.SetStatus(BuildingData.BuildingStatus.NORMAL);
        data.SetLevel(data._level + 1);
        ComputeEffects(data._city);//计算影响
        this.DoSaveWorldDatas();
        MediatorUtil.SendNotification(NotiDefine.BuildingStatusChanged, key);
    }

    public void SpeedUpUpgrade(string key)
    {
        BuildingData data = this.GetBuilding(key);
        if (data == null || data._status == BuildingData.BuildingStatus.NORMAL || data._status == BuildingData.BuildingStatus.BUILD)
            return;

        long leftSecs = data._expireTime - GameIndex.ServerTime;
        bool IsOk = RoleProxy._instance.TrySpeedUp((int)leftSecs, this.OnSpeedSure, key);
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
            List<BuildingData> maincitylist = this.GetCityBuildings(0);
            maincitylist.Remove(data);
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
            ComputeEffects(data._city);//计算影响
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

        if (level <= 0)
            level = 1;
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
        else if (ValueAddType.Patrol.Equals(AddType) ||
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

            int halfRange = UtilTools.ParseInt(AddValues[1]);
            int range = halfRange * 2 + 1;
            kv = new StringKeyValue();
            kv.key = config.AddDescs[1];
            kv.value = LanguageConfig.GetLanguage(LanMainDefine.BuildRange, range, range);
            list.Add(kv);
        }

        return list;
    }
}//end class
