﻿using System;
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
    private Dictionary<string, QuestCityData> _QuestCityDic = new Dictionary<string, QuestCityData>();
    private Dictionary<int, CityData> _AllCityDatas = new Dictionary<int, CityData>();

    public Dictionary<int, CityData> AllCitys => this._AllCityDatas;
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
        {
            return new VInt2();
        }

        CityConfig config = CityConfig.Instance.GetData(cityid);
        VInt2 pos = new VInt2(config.Position[0], config.Position[1]);
        return pos;
    }

    private void ComputeCityShowState(bool judgeSave)
    {
        //根据当前探索的区域来确定当前是否有新的City可见
        bool isChange = false;
        List<int> news = new List<int>();
        foreach (CityData city in this._AllCityDatas.Values)
        {
            if (city.IsOwn || city.Visible)
                continue;
            bool isInCityRange = this.IsInCityVisibleRange(city.ID);
            if (isInCityRange)
            {
                if (judgeSave == false)
                {
                    CityConfig config = CityConfig.Instance.GetData(city.ID);
                    PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.FindCity, config.Name));
                    RoleProxy._instance.AddLog(LogType.PatroFindCity, LanguageConfig.GetLanguage(LanMainDefine.FindCity, config.Name));
                }
                
                isChange = true;
                city.Visible = true;
                news.Add(city.ID);
            }
        }//end foreach

        if (judgeSave || isChange)
            this.DoSaveCitys();
        if (isChange)
            MediatorUtil.SendNotification(NotiDefine.NewCitysVisbleNoti, news);//更新地图
    }

    public void SetCityRangeSpotVisible(int cityid)
    {
        CityConfig config = CityConfig.Instance.GetData(cityid);
        VInt2 centerPos = this.GetCityCordinate(cityid);
        int halfRange = config.Range[1];
        int startX = centerPos.x - halfRange;
        int endX = centerPos.x + halfRange;
        int startZ = centerPos.y - halfRange;
        int endZ = centerPos.y + halfRange;
        List<VInt2> newAdds = new List<VInt2>();
        for (int row = startX; row <= endX; ++row)
        {
            int corX = row;
            for (int col = startZ; col <= endZ; ++col)
            {
                int corZ = col;
                bool isVisible = this.IsSpotVisible(corX, corZ);
                if (isVisible == false)
                {
                    VInt2 kv = new VInt2(corX, corZ);
                    newAdds.Add(kv);
                }
            }//end for col
        }//end for row

        this.AddVisibleSpots(newAdds);
    }

    public bool IsInCityRange(int cityid,int x,int y)
    {
        CityConfig config = CityConfig.Instance.GetData(cityid);
        VInt2 centerPos = this.GetCityCordinate(cityid);
        int halfRange = config.Range[0]/2;

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
                if (corX == x && corZ == y)
                    return true;
            }
        }

        return false;
    }

    public bool IsInCityVisibleRange(int cityid)
    {
        CityConfig config = CityConfig.Instance.GetData(cityid);
        VInt2 centerPos = this.GetCityCordinate(cityid);
        int halfRange = config.Range[1];
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
                bool isVisible = this.IsSpotVisible(corX, corZ);
                if (isVisible)
                    return true;
            }//end for col
        }//end for row
        return false;
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
            //计算是否有新的城市可见
            this.ComputeCityShowState(false);
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

    public void LoadAllCitys()
    {
        string json = CloudDataTool.LoadFile(SaveFileDefine.Citys);
        if (json.Equals(string.Empty) == false)
        {
            this._AllCityDatas = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, CityData>>(json);
        }
        else
        {
            this._AllCityDatas = new Dictionary<int, CityData>();
            //添加主城
            CityData data = new CityData();
            data.ID = 0;
            data.Visible = true;
            data.IsOwn = true;
            this._AllCityDatas.Add(data.ID, data);
        }

        int oldCount = this._AllCityDatas.Count;

        //全部的城市
        Dictionary<int, CityConfig> dic = CityConfig.Instance.getDataArray();
        foreach (CityConfig config in dic.Values)
        {
            if (this._AllCityDatas.ContainsKey(config.ID))
                continue;
            CityData data = new CityData();
            data.ID = config.ID;
            data.Visible = false;
            data.IsOwn = false;
            this._AllCityDatas.Add(data.ID, data);
        }

        if (oldCount != this._AllCityDatas.Count)
        {
            this.ComputeCityShowState(true);
        }
    }

    public void LoadQuestCity()
    {
        string json = CloudDataTool.LoadFile(SaveFileDefine.QuestCity);
        if (json.Equals(string.Empty) == false)
        {
            this._QuestCityDic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, QuestCityData>>(json);
        }

        List<string> finishs = new List<string>();
        var it = this._QuestCityDic.Values.GetEnumerator();
        while (it.MoveNext())
        {
            QuestCityData data = it.Current;
            if (GameIndex.ServerTime >= data.ExpireTime)
            {
                finishs.Add(data.ID);
            }
            else
            {
                this.QuestCityAction(data);
            }
        }
        it.Dispose();

        if (finishs.Count > 0)
        {
            foreach (string id in finishs)
            {
                this.OnFinishQuestCity(id, false);
            }
            this.DoSaveQuestCity();
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
        this.LoadAllCitys();
        //this.LoadPatrol();
        //this.LoadQuestCity();

        ComputeEffects();
    }

    public void DoSaveWorldDatas()
    {
        //存储
        string fileName = UtilTools.combine(SaveFileDefine.WorldBuiding, this._World);
        CloudDataTool.SaveFile(fileName, _WorldData);
    }

    public void DoSaveCitys()
    {
        //存储
        CloudDataTool.SaveFile(SaveFileDefine.Citys, this._AllCityDatas);
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

    public void DoSaveQuestCity()
    {
        //存储
        CloudDataTool.SaveFile(SaveFileDefine.QuestCity, this._QuestCityDic);
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

    public string GetArmyBuildingBy(int armyCareer, int cityid = 0)
    {
        List<BuildingData> list = this.GetCityBuildings(cityid);
        foreach (BuildingData data in list)
        {
            BuildingConfig config = BuildingConfig.Instance.GetData(data._id);
            if (config.AddType.Equals(ValueAddType.RecruitVolume))
            {
                BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(config.ID, data._level);
                int career = UtilTools.ParseInt(configLv.AddValues[0]);
                if (career == armyCareer)
                    return data._key;
            }
        }
        return "";
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
                add.InitJustItem(configLevel.AddValues[0]);
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
            else if (config.AddType.Equals(ValueAddType.TroopSpeed))
            {
                Effect.TeamMoveReduceRate = UtilTools.ParseInt(configLevel.AddValues[0]);
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

    public bool IsOwnCity(int city)
    {
        CityData data;
        if (this.AllCitys.TryGetValue(city, out data))
            return data.IsOwn;
        return false;
    }

    public CityData GetCityDataInPostionRange(int x,int y)
    {
        //获取坐标是否再Npc城市的范围内
        foreach (CityData data in this.AllCitys.Values)
        {
            if (data.ID == 0)
                continue;//主城不计算
            VInt2 pos = this.GetCityCordinate(data.ID);
            bool isInCityRange = this.IsInCityRange(data.ID,x,y);
            if (isInCityRange)
                return data;
            
        }
        return null;
    }


    public CityData GetCity(int city)
    {
        CityData data;
        if (this.AllCitys.TryGetValue(city, out data))
            return data;
        return null;
    }
    public List<int> GetAllOwnCity()
    {
        List<int> citys = new List<int>();
        foreach (CityData city in this._AllCityDatas.Values)
        {
            if (city.IsOwn)
                citys.Add(city.ID);
        }
        return citys;
    }

    public string GetCityName(int cityid)
    {
        if (cityid == 0)
            return LanguageConfig.GetLanguage(LanMainDefine.MainCity);
        CityConfig config = CityConfig.Instance.GetData(cityid);
        return config.Name;//读取配置
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

    public List<PatrolData> GetCItyPatrolDatas(int city)
    {
        List<PatrolData> datas = new List<PatrolData>();
        foreach (PatrolData data in this._OutPatrolDic.Values)
        {
            if (data.FromCIty == city)
                datas.Add(data);
        }
        return datas;
    }

    public int GetMoveDistance(int fromX, int fromZ, int targetX, int targetZ)
    {
        int descX = fromX - targetX;
        int descY = fromZ - targetZ;
        float distance = Mathf.Sqrt(descX * descX + descY * descY);
        return Mathf.CeilToInt(distance);
    }

    public long GetMoveExpireTime(int fromX, int fromZ, int targetX, int targetZ, float deltaSecs)
    {
        int descX = fromX - targetX;
        int descY = fromZ - targetZ;
        int distance = GetMoveDistance(fromX, fromZ, targetX, targetZ);
        long expireSces = GameIndex.ServerTime + Mathf.CeilToInt(deltaSecs * distance);
        return expireSces;
    }

    public bool CanMoveTo(int x, int z,int halfRange = 1)
    {
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
        patrol.ExpireTime = this.GetMoveExpireTime(cityPos.x, cityPos.y, x, z, SecsDelta);
        patrol.Range = 2;
        this._OutPatrolDic.Add(patrol.ID, patrol);

        this.PatrolAction(patrol);
        PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.DoPatrol, gamePos.x, gamePos.y));

        VInt2 worldPos = new VInt2(x, z);
        RoleProxy._instance.AddLog(LogType.DoPatrol, LanguageConfig.GetLanguage(LanMainDefine.DoPatrol, gamePos.x, gamePos.y), worldPos);

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
        path.Picture = "default";
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
        PathProxy._instance.RemovePath(key);
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
        this._OutPatrolDic.Remove(key);
     
        if (needSave)
        {
            this.SendNotification(NotiDefine.PatrolFinishNoti);
            PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.FinishPatrol, gamePos.x, gamePos.y));

            VInt2 worldPos = new VInt2(data.Target.x, data.Target.y);
            RoleProxy._instance.AddLog(LogType.FinishPatrol, LanguageConfig.GetLanguage(LanMainDefine.FinishPatrol, gamePos.x, gamePos.y), worldPos);

            this.DoSaveOutPatrol();
        }
           
    }

    public void DoOwnCity(int cityid,bool isOwn = true)
    {
        CityData city = this.GetCity(cityid);
        if (city == null || city.Visible == false)
            return;

        VInt2 cityPos = this.GetCityCordinate(cityid);
        CityConfig config = CityConfig.Instance.GetData(cityid);
      /*  int borderRange = config.Range[0] / 2 + 1;
        bool canMove = this.CanMoveTo(cityPos.x, cityPos.y, borderRange);
        if (canMove == false)
        {
           // PopupFactory.Instance.ShowErrorNotice(ErrorCode.NoVisibleNoAttack, config.Name);
          //  return;
        }
      */

        city.IsOwn = isOwn;

        //发奖励
        if (isOwn)
        {
            List<CostData> list = new List<CostData>();
            foreach (string award in config.AttackDrops)
            {
                CostData cost = new CostData();
                cost.InitFull(award);
                if (cost.type.Equals(CostData.TYPE_ITEM))
                    list.Add(cost);
                else if (cost.type.Equals(CostData.TYPE_HERO))
                    HeroProxy._instance.DoOwnHero(cost);
            }
            if (list.Count > 0)
                RoleProxy._instance.ChangeRoleNumberValue(list);
            //设置新的迷雾解锁
            SetCityRangeSpotVisible(cityid);
        }
      
        this.DoSaveCitys();
        this.SendNotification(NotiDefine.DoOwnCityResp, cityid);
    }

    public bool DoQuestCity(Dictionary<string, object> vo)
    {
        int HeroID = (int)vo["HeroID"];
        int TargetCity = (int)vo["TargetCity"];

        CityConfig config = CityConfig.Instance.GetData(TargetCity);

        int hasGoneHeroID = 0;
        foreach (QuestCityData data in this._QuestCityDic.Values)
        {
            if (data.TargetCity == TargetCity)
            {
                hasGoneHeroID = data.HeroID;
                break;
            }
        }

        if (hasGoneHeroID > 0)
        {
            HeroConfig configGone = HeroConfig.Instance.GetData(hasGoneHeroID);
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.HeroHasQuest, configGone.Name,config.Name);
            return false;
        }


        Hero hero = HeroProxy._instance.GetHero(HeroID);
        HeroConfig confighe = HeroConfig.Instance.GetData(HeroID);
        int inTeamID = TeamProxy._instance.GetHeroTeamID(hero.Id);
        if (inTeamID > 0)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.HeroInTeamNoQuest, confighe.Name);
            return false;
        }

        VInt2 targetPos = this.GetCityCordinate(TargetCity);

       
        int borderRange = config.Range[0] / 2 + 1;
        bool canMove = this.CanMoveTo(targetPos.x, targetPos.y, borderRange);
        VInt2 gamePos = UtilTools.WorldToGameCordinate(targetPos.x, targetPos.y);
        if (canMove == false)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.NoVisibleNoQuest, config.Name);
            return false;
        }

        CityData city = this.GetCity(TargetCity);
        if (city.IsOwn == false)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.NoOwnNoQuest, config.Name);
            return false;
        }
        if (city.QuestIndex != null && city.QuestIndex.Count >= config.QuestDrops.Length)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.CityNoQuestDrop, config.Name);
            return false;
        }

        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.HeroCostEnegry);
        int needEnegry = cfgconst.IntValues[0];
        if (hero.GetEnegry() < needEnegry)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.HeroNoEnegry, needEnegry);
            return false;
        }

        hero.DoingState = (int)HeroDoingState.QuestCity;
        hero.ChangeEnegry(needEnegry);
        HeroProxy._instance.DoSaveHeros();
       
        cfgconst = ConstConfig.Instance.GetData(ConstDefine.QuestDeltaSces);
        int SecsDelta = cfgconst.IntValues[0];
        float HeroSecs = (float)SecsDelta / (float)confighe.Speed;

        QuestCityData quest = new QuestCityData();
        quest.ID = UtilTools.GenerateUId();
        quest.HeroID = HeroID;
        quest.TargetCity = TargetCity;
        quest.Start = this.GetCityCordinate(hero.City);
        quest.Target.x = targetPos.x;
        quest.Target.y = targetPos.y;
        quest.StartTime = GameIndex.ServerTime;
        quest.ExpireTime = this.GetMoveExpireTime(quest.Start.x, quest.Start.y, quest.Target.x, quest.Target.y, HeroSecs);
        this._QuestCityDic.Add(quest.ID, quest);

        this.QuestCityAction(quest);
        PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.DoQuestCity, confighe.Name,config.Name));
        RoleProxy._instance.AddLog(LogType.QuestCity, LanguageConfig.GetLanguage(LanMainDefine.DoQuestCity, confighe.Name, config.Name), targetPos);

        this.SendNotification(NotiDefine.QuestCityResp, quest);
        this.DoSaveQuestCity();
        return true;
    }

    private void QuestCityAction(QuestCityData quest)
    {
        //路线添加
        PathData path = new PathData();
        path.ID = quest.ID;
        path.Type = PathData.TYPE_QUEST_CITY;
        path.Start = quest.Start;
        path.Target = quest.Target;
        path.StartTime = quest.StartTime;
        path.ExpireTime = quest.ExpireTime;
        path.Model = "PathModel";//后面读取英灵模型，暂时写死
        path.Picture = HeroConfig.Instance.GetData(quest.HeroID).Model;
        path.Param = quest;
        PathProxy._instance.AddPath(path);

        //时间监听
        TimeCallData dataTime = new TimeCallData();
        dataTime._key = quest.ID;
        dataTime._notifaction = NotiDefine.QuestCityExpireReachedNoti;
        dataTime.TimeStep = quest.ExpireTime;
        dataTime._param = quest.ID;
        MediatorUtil.SendNotification(NotiDefine.AddTimestepCallback, dataTime);
        //this.DoSaveQuestCity();
    }

    public void OnFinishQuestCity(string questid,bool needSave = true)
    {
        PathProxy._instance.RemovePath(questid);
        QuestCityData data;
        if (this._QuestCityDic.TryGetValue(questid, out data) == false)
            return;
        int city = data.TargetCity;
        CityData cityInfo = this.GetCity(city);
        if (cityInfo == null)
            return;

        if (cityInfo.QuestIndex == null)
            cityInfo.QuestIndex = new List<int>();

        HeroProxy._instance.ChangeHeroDoing(data.HeroID, (int)HeroDoingState.Idle);
        this._QuestCityDic.Remove(questid);

        if (needSave)
            this.DoSaveQuestCity();

        //抽奖
        HeroConfig heroCoinfig = HeroConfig.Instance.GetData(data.HeroID);
        CityConfig config = CityConfig.Instance.GetData(city);

        //判断是否成功
        List<string> talents = new List<string>(config.QuestTalents);
        string[] testTalent = UtilTools.GetRandomChilds<string>(talents, 1)[0].Split(':');
        int talentType = UtilTools.ParseInt(testTalent[0]);
        int talentValue = UtilTools.ParseInt(testTalent[1]);
        int isSuccess = HeroProxy._instance.JudegeTalentResult(talentType, talentValue, data.HeroID);
        string Notice;
        if (isSuccess == 0)
        {
            //失败
            string lanKey = UtilTools.combine(LanMainDefine.TalentFail, talentType);
            Notice = LanguageConfig.GetLanguage(lanKey, heroCoinfig.Name, config.Name, talentValue);
        }
        else
        {
            //发奖励
            List<string> questDrops = new List<string>(config.QuestDrops);
            List<string> LastAwards = new List<string>();
            int count = questDrops.Count;
            for (int i = 0; i < count; ++i)
            {
                if (cityInfo.QuestIndex.Contains(i))
                    continue;//已经获取过了
                LastAwards.Add(questDrops[i]);
            }

            if (LastAwards.Count == 0)
            {
                PopupFactory.Instance.ShowErrorNotice(ErrorCode.CityNoQuestDrop, config.Name);
                return;
            }

            string awards = UtilTools.GetRandomChilds<string>(LastAwards, 1)[0];
            int indexof = questDrops.IndexOf(awards);
            if (cityInfo.QuestIndex.Contains(indexof) == false)
                cityInfo.QuestIndex.Add(indexof);
            //发放奖励
            string GetName = "";

            CostData cost = new CostData();
            cost.InitFull(awards);

           
            if (cost.type.Equals(CostData.TYPE_ITEM))
            {
                RoleProxy._instance.ChangeRoleNumberValueBy(cost);
                string name = ItemInfoConfig.Instance.GetData(cost.id).Name;
                GetName = LanguageConfig.GetLanguage(LanMainDefine.ItemCount, name, cost.count);
            }
            else if(cost.type.Equals(CostData.TYPE_HERO))
            {
                int heroid = UtilTools.ParseInt(cost.id);
                HeroProxy._instance.ChangeHeroBelong(heroid, (int)HeroBelong.MainCity);
                GetName = HeroConfig.Instance.GetData(heroid).Name;
            }

            string lanKey;
            if (isSuccess == 2)
            {
                //幸运
                lanKey = UtilTools.combine(LanMainDefine.LuckyTalentSuccess, talentType);
            }
            else
            {
                lanKey = UtilTools.combine(LanMainDefine.TalentSuccess, talentType);
            }

            Notice = LanguageConfig.GetLanguage(lanKey, heroCoinfig.Name, config.Name, GetName);
        }

        PopupFactory.Instance.ShowNotice(Notice);

        VInt2 cityInfoPos = this.GetCityCordinate(city);
        RoleProxy._instance.AddLog(LogType.QuestCityResult, Notice, cityInfoPos);
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
        this.OnBuildExpireFinsih(key);
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

            VInt2 pos = new VInt2(data._cordinate.x, data._cordinate.y);
            VInt2 gamePos = UtilTools.WorldToGameCordinate(pos.x, pos.y);
            string notice;
            if (data._status == BuildingData.BuildingStatus.BUILD)
                notice = LanguageConfig.GetLanguage(LanMainDefine.BuildFinish, config.Name);
            else
                notice = LanguageConfig.GetLanguage(LanMainDefine.UpgradeBuildFinish, config.Name, data._level);

            PopupFactory.Instance.ShowNotice(notice);
            RoleProxy._instance.AddLog(LogType.BuildUp, notice, null,data._key);

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
            cost.InitJustItem(AddValues[0]);
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
