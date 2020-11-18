
using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NotiDefine
{
    public const string APP_START_UP = "APP_START_UP";
    public const string MVC_STARTED = "MVC_STARTED";

    public const string WINDOW_DO_SHOW = "WINDOW_DO_SHOW";
    public const string WINDOW_DO_HIDE = "WINDOW_DO_HIDE";
    public const string WINDOW_DO_DESTROY = "WINDOW_DO_DESTROY";
    public const string WINDOW_HAS_SHOW = "WINDOW_HAS_SHOW";
    public const string FULLSCREEN_WINDOW_SHOW = "FULLSCREEN_WINDOW_SHOW";
    public const string GAME_RESET = "GAME_RESET";

    public const string OPEN_ = "OPEN_";
    public const string CLOSE_ = "CLOSE_";

    public const string DoLoadScene = "DoLoadScene";
    public const string LoadSceneFinish = "LoadSceneFinish";

   
    public const string AddTimestepCallback = "AddTimestepCallback";
    public const string RemoveTimestepCallback = "RemoveTimestepCallback";//key，动态生成的唯一标识

    public const string TEST_CALLBACK_NOTI = "TEST_CALLBACK_NOTI";

    public const string CreateOneBuildingDo = "CreateOneBuildingDo";
    public const string CreateOneBuildingResp = "CreateOneBuildingResp";

    public const string UpgradeOneBuildingDo = "UpgradeOneBuildingDo";
    public const string UpgradeOneBuildingResp = "UpgradeOneBuildingResp";


    public const string BuildingExpireReachedNoti = "BuildingExpireReachedNoti";
    public const string BuildingStatusChanged = "BuildingStatusChanged";
    public const string PatrolExpireReachedNoti = "PatrolExpireReachedNoti";
    public const string QuestCityExpireReachedNoti = "QuestCityExpireReachedNoti";

    public const string PatrolDo = "PatrolDo";
    public const string PatrolResp = "PatrolResp";
    public const string PatrolFinishNoti = "PatrolFinishNoti";

    public const string QuestCityDo = "QuestCityDo";
    public const string QuestCityResp = "QuestCityResp";

    public const string DoOwnCityDo = "DoOwnCityDo";
    public const string DoOwnCityResp = "DoOwnCityResp";

    public const string BuildingRelocateDo = "BuildingRelocateDo";
    public const string BuildingRelocateResp = "BuildingRelocateResp";

    public const string TryBuildBuilding = "TryBuildBuilding";

    public const string BuildingCancelDo = "BuildingCancelDo";
    public const string BuildingSpeedUpDo = "BuildingSpeedUpDo";

    public const string HomeRangeChanged = "HomeRangeChanged";
    public const string LandVisibleChanged = "LandVisibleChanged";

    public const string BuildingRemoveNoti = "BuildingRemoveNoti";

    public const string ConfirmBuild = "ConfirmBuild";

    public const string GenerateMySpotDo = "GenerateMySpotDo";
    public const string GenerateMySpotResp = "GenerateMySpotResp";

    public const string EnterGameDo = "EnterGameDo";
    public const string CreateRoleDo = "CreateRoleDo";
    public const string CreateRoleResp = "CreateRoleResp";


    public const string PowerChanged = "PowerChanged";
    public const string NewLogNoti = "NewLogNoti";
    public const string JudegeNewLog = "JudegeNewLog";


    public const string AcceptHourAwardDo = "AcceptHourAwardDo";
    public const string AcceptHourAwardResp = "AcceptHourAwardResp";

    public const string JudgeIncome = "JudgeIncome";

    public const string IncomeHasUpdated = "IncomeHasUpdated";
    public const string NumberValueHasUpdated = "NumberValueHasUpdated";
    public const string ResLimitHasUpdated = "ResLimitHasUpdated";
    public const string RoleLvExpHasUpdated = "RoleLvExpHasUpdated";

    public const string LoadAllHeroDo = "LoadAllHeroDo";
    public const string LoadAllHeroResp = "LoadAllHeroResp";

    public const string GetHeroRefreshDo = "GetHeroRefreshDo";
    public const string GetHeroRefreshResp = "GetHeroRefreshResp";

    public const string TalkToHeroDo = "TalkToHeroDo";
    public const string TalkToHeroResp = "TalkToHeroResp";
    public const string FavorLevelUpNoti = "FavorLevelUpNoti";

    public const string RecruitHeroDo = "RecruitHeroDo";
    public const string RecruitHeroResp = "RecruitHeroResp";


   

    public const string HeroTavernRefreshReachedNoti = "HeroTavernRefreshReachedNoti";

    public const string HeroTavernRefresh = "HeroTavernRefresh";


    public const string GetHeroNoti = "GetHeroNoti";
    public const string LosetHeroNoti = "LosetHeroNoti";


    public const string HerosUpdated = "HerosUpdated";
    public const string CordinateChange = "CordinateChange";
    public const string WorldGoToStart = "WorldGoToStart";

    public const string LoadAllArmyDo = "LoadAllArmyDo";
    public const string LoadAllArmyResp = "LoadAllArmyResp";
    public const string ArmyRecruitExpireReachedNoti = "ArmyRecruitExpireReachedNoti";

    public const string RecruitArmyDo = "RecruitArmyDo";
    public const string RecruitArmyResp = "RecruitArmyResp";

    public const string HarvestArmyDo = "HarvestArmyDo";
    public const string HarvestArmyResp = "HarvestArmyResp";

    public const string CancelArmyDo = "CancelArmyDo";
    public const string CancelArmyResp = "CancelArmyResp";

    public const string SpeedUpArmyDo = "SpeedUpArmyDo";
    public const string SpeedUpArmyResp = "SpeedUpArmyResp";

    public const string ArmyStateChange = "ArmyStateChange";


    public const string ArmyRecruitFinishedNoti = "ArmyRecruitFinishedNoti";


    public const string ErrorCode = "ErrorCode";

    public const string GO_TO_SELEC_BUILDING_BY_ID = "GO_TO_SELEC_BUILDING_BY_ID";


    public const string LoadAllTeamDo = "LoadAllTeamDo";
    public const string LoadAllTeamResp = "LoadAllTeamResp";

    public const string InitCityTeamDo = "InitCityTeamDo";
    public const string InitCityTeamResp = "InitCityTeamResp";

    public const string SetTeamHeroDo = "SetTeamHeroDo";
    public const string SetTeamHeroResp = "SetTeamHeroResp";

    public const string PathAddNoti = "PathAddNoti";
    public const string PathRemoveNoti = "PathRemoveNoti";

    public const string NewCitysVisbleNoti = "NewCitysVisbleNoti";

}

public class ErrorCode
{
    public const string ValueOutOfRange = "ValueOutOfRange";
    public const string CostNotEnought = "CostNotEnought";
    public const string CareerRecruitYet = "CareerRecruitYet";
    public const string CityArmyFull = "CityArmyFull";
    public const string CareerRecruitLimit = "CareerRecruitLimit";
    public const string NoArmyCanHarvest = "NoArmyCanHarvest";
    public const string NoArmyRecruit = "NoArmyRecruit";
    public const string SpeedUpCostNotEnought = "SpeedUpCostNotEnought";
    public const string FinishArmyRecruit = "FinishArmyRecruit";
    public const string TeamNotIdle = "TeamNotIdle";
    public const string TeamNotOpen = "TeamNotOpen";
    public const string NoPatrol = "NoPatrol";
    public const string NoVisibleNoPatrol = "NoVisibleNoPatrol";
    public const string IsVisibleNoPatrol = "IsVisibleNoPatrol";
    public const string HasSendPatrol = "HasSendPatrol";
    public const string NoPatroller = "NoPatroller";
    public const string CityNoQuestDrop = "CityNoQuestDrop";
    public const string NoVisibleNoQuest = "NoVisibleNoQuest";
    public const string HeroInTeamNoQuest = "HeroInTeamNoQuest";
    public const string HeroHasQuest = "HeroHasQuest";
    public const string HeroNoEnegry = "HeroNoEnegry";
    public const string NoOwnNoQuest = "NoOwnNoQuest";
    public const string NoVisibleNoAttack = "NoVisibleNoAttack";
    public const string NoHeroFreeToQuest = "NoHeroFreeToQuest";
}

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
    public int _city;//0-主城

    public int _UpgradeSecs = 0;//下一个等级所需时间
    public VInt2 _cordinate = new VInt2();//左下角的坐标
    public List<VInt2> _occupyCordinates;//占领的全部地块坐标

    public int _level = 0;
    public BuildingStatus _status = BuildingStatus.INIT;//建筑的状态
    public long _expireTime;//建造或者升级的到期时间
    public int _durability;//当前耐久度

    public void Create(int id, int x, int z, int city = 0)
    {
        this._key = UtilTools.GenerateUId();
        this._id = id;
        this._city = city;
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

public class CommonUINameDefine
{
    public const string DOWN_arrow = "DOWN_arrow";
    public const string UP_arrow = "UP_arrow";
}


public class CityData
{
    public int ID;
    public bool Visible;
    public bool IsOwn;
    public List<int> QuestIndex;//探索的奖励下标
}

public class LogData
{
    public string ID;
    public LogType Type;
    public long Time;
    public string Content;
    public VInt2 Position;
    public string BdKey;
    public bool New;
}

public enum LogType
{
    HeroFavorLevel = 1,
    HeroFavorChange,
    HarvestArmy,
    DoPatrol,
    FinishPatrol,
    QuestCity,
    QuestCityResult,
    PatroFindCity,
    BuildUp,
    RecruitHeroSuccess,
    OwnCityResp,
    HourTax,
    FinshArmy,
}

public enum HeroBelong
{
    MainCity = 0,
    Wild = -1,
}

public enum CareerRateDefine
{ 
    C = 1,
    B = 2,
    A = 3,
    S = 4,
}

public enum ItemTypeDefine
{
    RES = 1,
    DROP_BAG = 2,
    GIVE_FAVOR = 3,
}

public enum Layer
{
    DEFAULT = 0,
    UI = 5,
    HeroScene = 17,
}

public enum BuildingType
{
    Economy= 1,
    Military = 2,
    Decorate = 3,
    Wild = 4,
}

public enum TalentDefine
{
    KOU_CAI = 1,
    QIE_CUO = 2,
    QI_YI = 3,
    WEN_CAI = 4,
}

public enum  FancyDefine
{
    WINE = 1,
    DRAWING = 2,
    BOOK = 3,
    TREASURE = 4,
}

public enum MediatorDefine
{
    NONE,
    DATA_CENTER,
    LOGIN,

    HOME_LAND,
    SCENE_LOADER,
    MAIN,
    BUILD_CENTER,
    RECRUIT,
    ARMY,
    CREATE,
    HERO,
    BAG,
    GAME_LOG,
    TEAM,
    SET_TEAM_HERO,
}

public class StringKeyValue
{
    public string key = "";
    public string value = "";
}

public class VInt2
{

    public int x;
    public int y;
    public VInt2()
    {
        this.x = 0;
        this.y = 0;
    }
    public VInt2(int xx, int yy)
    {
        this.x = xx;
        this.y = yy;
    }
}

class ProxyNameDefine
{
    public const string TIME_CENTER = "TIME_CENTER";
    public const string WORLD = "WORLD";
    public const string ROLE = "ROLE";
    public const string HERO = "HERO";
    public const string ARMY = "ARMY";
    public const string TEAM = "TEAM";
    public const string PATH = "PATH";
}

public class SceneDefine
{
    public const string World = "World";
    public const string Home = "Home";
    public const string GameIndex = "GameIndex";
    public const string Hero = "Hero";
}

public class ValueAddType
{
    public const string CityTroop = "CityTroop";
    public const string BuildRange = "BuildRange";
    public const string Patrol = "Patrol";
    public const string HourTax = "HourTax";
    public const string World = "World";
    public const string HeroMaxBlood = "HeroMaxBlood";
    public const string Equipment = "Equipment";
    public const string ResearchSpeed = "ResearchSpeed";
    public const string RecruitSecs = "RecruitSecs";
    public const string ElementAdd = "ElementAdd";
    public const string RecruitVolume = "RecruitVolume";
    public const string Cure = "Cure";
    public const string DeployCount = "DeployCount";
    public const string HeroRecruit = "HeroRecruit";
    public const string AttributeAdd = "AttributeAdd";
    public const string Worker = "Worker";
}

public class ElementDefine
{
    public const string Fire = "Fire";
    public const string Wind = "Wind";
    public const string Water = "Water";
}

public class AttributeDefine
{
    public const string Attack = "Attack";
    public const string Defense = "Defense";
}


public class CareerDefine
{
    public const int Rider = 1;
    public const int Archer = 2;
    public const int Infantry = 3;
    public const int Count = 3;
    public static string GetName(int type)
    {
        if (type.Equals(Rider))
            return LanguageConfig.GetLanguage(LanMainDefine.Rider);

        if (type.Equals(Archer))
            return LanguageConfig.GetLanguage(LanMainDefine.Archer);

        if (type.Equals(Infantry))
            return LanguageConfig.GetLanguage(LanMainDefine.Infantry);
        return "";
    }
}

public class AttributeData
{
    public string Id;
    public float Value;

    public void Init(string keyValueStr)
    {
        string[] list = keyValueStr.Split(':');
        this.Id = list[0];
        this.Value = UtilTools.ParseFloat(list[1]);
    }

    public static Dictionary<string,float> InitAttributes(string[] keyValues)
    {
        Dictionary<string, float> dic = new Dictionary<string, float>();
        int len = keyValues.Length;
        for (int i = 0; i < len; ++i)
        {
            AttributeData data = new AttributeData();
            data.Init(keyValues[i]);
            float oldValue = 0;
            if (dic.TryGetValue(data.Id, out oldValue) == false)
                oldValue = 0;
            dic[data.Id] = oldValue + data.Value;
        }
        return dic;
    }//end func

    public static Dictionary<string, float> InitAttributesBy(string keyValuesStr,char split='|')
    {
        string[] keyvalues = keyValuesStr.Split(split);
        return InitAttributes(keyvalues);
    }//end func
}

public class IncomeData
{
    public string Key;
    public int Count;
    public int LimitVolume;//容量
    public IncomeData(string key)
    {
        this.Key = key;
        this.Count = 0;
    }
}

public class PathData
{
    public static int TYPE_PATROL = 1;
    public static int TYPE_TEAM = 2;
    public static int TYPE_QUEST_CITY = 3;

    public string ID;
    public int Type;
    public VInt2 Start = new VInt2();
    public VInt2 Target = new VInt2();
    public long StartTime;
    public long ExpireTime;
    public string Model;
    public object Param;
    public string Picture;
}


public class PatrolData
{
    public string ID;
    public int FromCIty;
    public VInt2 Start = new VInt2();
    public VInt2 Target = new VInt2();
    public long StartTime;
    public long ExpireTime;
    public int Range;
}

public class QuestCityData
{
    public string ID;
    public int HeroID;
    public int TargetCity;
    public VInt2 Start = new VInt2();
    public VInt2 Target = new VInt2();
    public long StartTime;
    public long ExpireTime;
}

public class VisibleData
{
    public int CityID;//0-主城
    public int Range;
}

public class BuildingEffectsData
{
    public System.Collections.Generic.List<string> _changeKeys = new System.Collections.Generic.List<string>();
    public Dictionary<int, Dictionary<string, float>> CareerAttrAdds = new Dictionary<int, Dictionary<string, float>>();
    public Dictionary<string, float> ElementAdds = new Dictionary<string, float>();

    public int MaxBloodAdd = 0;

    public int PatrolMax = 0;
    public int PowerAdd = 0;
    public int BuildRange = 0;
    public VisibleData VisibleRangeData;
    public float RecruitReduceRate = 0f;
    public int TroopNum = 0;//可以配置队伍数量
    public int DayBoxLimit = 0;//每日宝箱次数上限
    public int HeroRectuitLimit = 0;//酒馆刷新上限

    public Dictionary<int, int> RecruitVolume = new Dictionary<int, int>();//兵种每次招募的上限
    public Dictionary<string, IncomeData> IncomeDic = new Dictionary<string, IncomeData>();

    public void Reset()
    {
        for (int i = (int)CareerDefine.Rider; i <= (int)CareerDefine.Count; ++i)
        {
            Dictionary<string, float> attrDic = new Dictionary<string, float>();
            attrDic[AttributeDefine.Attack] = 0;
            attrDic[AttributeDefine.Defense] = 0;
        
            this.CareerAttrAdds[i] = attrDic;
        }

        this.VisibleRangeData = new VisibleData();
        this.ElementAdds[ElementDefine.Fire] = 0;
        this.ElementAdds[ElementDefine.Wind] = 0;
        this.ElementAdds[ElementDefine.Water] = 0;

        this.IncomeDic[ItemKey.gold] = new IncomeData(ItemKey.gold);
        this.IncomeDic[ItemKey.food] = new IncomeData(ItemKey.food);
        this.IncomeDic[ItemKey.wood] = new IncomeData(ItemKey.wood);
        this.IncomeDic[ItemKey.metal] = new IncomeData(ItemKey.metal);
        this.IncomeDic[ItemKey.stone] = new IncomeData(ItemKey.stone);

        this.RecruitVolume[CareerDefine.Archer] = 0;
        this.RecruitVolume[CareerDefine.Rider] = 0;
        this.RecruitVolume[CareerDefine.Infantry] = 0;

        this.PatrolMax = 0;

        this.MaxBloodAdd = 0;
        this.PatrolMax = 0;
        this.PowerAdd = 0;
        this.BuildRange = 0;
    }
}

public class ItemKey
{
    public const string gold = "gold";
    public const string food = "food";
    public const string wood = "wood";
    public const string metal = "metal";
    public const string stone = "stone";

    public static string GetName(string key)
    {
        ItemInfoConfig config = ItemInfoConfig.Instance.GetData(key);
        if (config != null)
            return config.Name;
        return "";
    }
}

public class Army
{
    public int Id;//兵种ID
    public int Count;//可以使用的数量
    public int Injured;//伤兵
    public int RecruitOneSces;//招募开始时每个招募所需的时间
    public long RecruitStartTime;//招募开始时间
    public long RecruitExpireTime;//招募到期时间
    public int ReserveCount;//
   
    public bool CanAccept;//是否可以领取了
    public string TimeKey = "";
    public int CityID;//所属城市

    public void Init(int id)
    {
        this.Id = id;
        this.Count = 0;
        this.RecruitExpireTime = 0;
        this.RecruitOneSces = 0;
        this.RecruitStartTime = 0;
        this.ReserveCount = 0;
        this.Injured = 0;
        this.CanAccept = false;
        this.TimeKey = "";
    }
}

public enum EquipType
{
    Weapon =1,
    Armor = 2,
    Horse = 3,
};

public enum HeroTeamState
{
    //>0再队伍中
    NoTeam = 0,
    QuestCity = -1,
   
};

public enum TeamStatus
{
    Idle = 1,
    March = 2,
    Back = 3,
    Fight = 3,
};

public class HeroRecruitRefreshData
{
    public int City;
    public List<int> IDs;
    public long ExpireTime;
}

public class Team
{
    public int Id;//CityID*100+Index
    public int Index;//序列
    public int HeroID;//上阵英雄
    public int Status;//0-空闲 1-行军 2-返回 3-战斗
    public int CityID;//0-主城，>0 Npc城市ID
    public int FromID;//0-主城，>0 要塞ID
    public int TargetID;//移动目标 0-主城 >0 Npc城市ID 
    public long StartTime;//行军或者返回开始时间
    public long ExpireTime;//行军或者返回的结束时间
}

public class Hero
{
    public int Id;//唯一标识
    public int Level;
    public int StarRank;//升星
    public int Exp;
    public float ElementValue;//和稀有的挂钩
    public int ArmyTypeID;//当前兵种
    public int Blood;//当前兵力
    public int MaxBlood;//带兵上限 等级和建筑计算
    public int Belong;//-1-在野  >=0 为对应城市ID
    public bool IsMy;//是否为我方
    public int TeamId;//上阵队伍ID 0-未上阵 -1-探索
    public int Favor;//好感度
    public long TalkExpire;//上次的聊天时间 HeroTalkGap 时间后俩天可以增加好感度
    public long EnegryFullExpire;//体力恢复满的时间
    public int MaxEnegry;
    public Dictionary<string, float> Attributes;

    public Dictionary<string, int> GetItems;//获得过的馈赠
    public Dictionary<int, string> Equips;// EqupType 装备的部位和道具ID

    public void Create(HeroConfig config)
    {
        this.Id = config.ID;
        this.Level = config.InitLevel;
        this.Exp = 0;
        this.ArmyTypeID = 0;
        this.Blood = 0;
        this.Belong = config.InitBelong;
        this.IsMy = this.Belong == 0;
        this.TeamId = 0;
        this.StarRank = 0;
        this.Favor = 0;
        this.TalkExpire = 0;
        this.EnegryFullExpire = 0;
        this.MaxEnegry = ConstConfig.Instance.GetData(ConstDefine.HeroEnegry).IntValues[0];
        this.GetItems = new Dictionary<string, int>();
        this.Equips = new Dictionary<int, string>();
        this.ComputeAttributes();
    }

    public int GetEnegry()
    {
        long leftSecs = this.EnegryFullExpire - GameIndex.ServerTime;
        if (leftSecs <= 0)
            return this.MaxEnegry;

        int EnegryDelta = ConstConfig.Instance.GetData(ConstDefine.HeroEnegryRecoverDelta).IntValues[0];
        int curEnergy = this.MaxEnegry - Mathf.CeilToInt((float)leftSecs / (float)EnegryDelta);
        return curEnergy;
    }

    public void ChangeEnegry(int addEnegry)
    {
        int EnegryDelta = ConstConfig.Instance.GetData(ConstDefine.HeroEnegryRecoverDelta).IntValues[0];
        int TotleSecs = EnegryDelta * addEnegry;
        long leftSecs = this.EnegryFullExpire - GameIndex.ServerTime;
        if (leftSecs <= 0)
            this.EnegryFullExpire = GameIndex.ServerTime + TotleSecs;
        else
            this.EnegryFullExpire +=  TotleSecs;
    }

    public void ComputeAttributes()
    {
        HeroConfig config = HeroConfig.Instance.GetData(this.Id);
        HeroLevelConfig configLv = HeroLevelConfig.Instance.GetData(this.Level);
        HeroStarConfig configStar = HeroStarConfig.Instance.GetData(config.Star);
        int cityid = TeamProxy._instance.GetTeamCity(this.TeamId);
        BuildingEffectsData bdAddData = WorldProxy._instance.GetBuildingEffects(cityid);

        if (bdAddData != null)
        {
            this.ElementValue = ComputeElementValue(this.Id) + bdAddData.ElementAdds[config.Element];
            this.MaxBlood = configLv.BloodMax + bdAddData.MaxBloodAdd;
        }
        else
        {
            this.ElementValue = ComputeElementValue(this.Id);
            if (configLv == null)
            {
                Debug.LogError("configLv");
            }
            this.MaxBlood = configLv.BloodMax;
        }

        if (this.Attributes == null)
            this.Attributes = new Dictionary<string, float>();
        else
            this.Attributes.Clear();
        foreach (string str in configStar.BaseDemage)
        {
            string[] list = str.Split(':');
            float baseValue = UtilTools.ParseFloat(list[1]);
            float oldValue = 0;
            if (this.Attributes.TryGetValue(list[0], out oldValue) == false)
                oldValue = 0;
            Attributes[list[0]] = oldValue + baseValue;
        }//end foreach

        foreach (string str in configStar.GrowDemage)
        {
            string[] list = str.Split(':');
            float levelValue = UtilTools.ParseFloat(list[1]) * (this.Level-1);
            float oldValue = 0;
            if (this.Attributes.TryGetValue(list[0], out oldValue) == false)
                oldValue = 0;
            Attributes[list[0]] = oldValue + levelValue;
        }//end foreach

    }//end function

    public static int ComputeElementValue(int id)
    {
        HeroConfig config = HeroConfig.Instance.GetData(id);
        HeroStarConfig configStar = HeroStarConfig.Instance.GetData(config.Star);
        if (configStar == null)
        {
            return 0;
        }
        return configStar.ElementValue;
    }

    public static string GetCareerEvaluateName(int rate)
    {
        string key = UtilTools.combine(LanMainDefine.CareerRate, rate);
        return LanguageConfig.GetLanguage(key);
    }
}


public class CostData
{
    public string id;
    public int count;

    public void Init(string keyValueStr,float mutil =1f)
    {
        string[] list = keyValueStr.Split(':');
        this.id = list[0];
        this.count = Mathf.RoundToInt(UtilTools.ParseFloat(list[1]) * mutil);
    }

    public void Init(CostData d)
    {
        this.id = d.id;
        this.count = d.count;
    }
}

public class HourAwardData
{
    public string id;
    public int add_up_value;//当前已经累计量
    public float base_secs_value;//每秒可产出的量
    public long generate_time;//开始计算的时间
}
