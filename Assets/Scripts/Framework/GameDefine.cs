using Boo.Lang;
using System.Collections.Generic;
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

    public const string BuildingRelocateDo = "BuildingRelocateDo";
    public const string BuildingRelocateResp = "BuildingRelocateResp";

    public const string TryBuildBuilding = "TryBuildBuilding";

    public const string BuildingCancelDo = "BuildingCancelDo";
    public const string BuildingSpeedUpDo = "BuildingSpeedUpDo";


    public const string BuildingRemoveNoti = "BuildingRemoveNoti";

    public const string ConfirmBuild = "ConfirmBuild";

    public const string GenerateMySpotDo = "GenerateMySpotDo";
    public const string GenerateMySpotResp = "GenerateMySpotResp";

    public const string GenerateMyBuildingDo = "GenerateMyBuildingDo";
    public const string GenerateMyBuildingResp = "GenerateMyBuildingResp";

   
    public const string LoadRoleDo = "LoadRoleDo";
    public const string LoadRoleResp = "LoadRoleResp";

   
    public const string AcceptHourAwardDo = "AcceptHourAwardDo";
    public const string AcceptHourAwardResp = "AcceptHourAwardResp";

    public const string IncomeHasUpdated = "IncomeHasUpdated";
    public const string NumberValueHasUpdated = "NumberValueHasUpdated";
    public const string ResLimitHasUpdated = "ResLimitHasUpdated";
    public const string RoleLvExpHasUpdated = "RoleLvExpHasUpdated";

    public const string LoadAllHeroDo = "LoadAllHeroDo";
    public const string LoadAllHeroResp = "LoadAllHeroResp";

    public const string ChangeHeroBelongDo = "ChangeHeroBelongDo";
 

    public const string GetHeroNoti = "GetHeroNoti";
    public const string LosetHeroNoti = "LosetHeroNoti";


    public const string HerosUpdated = "HerosUpdated";

    public const string ErrorCode = "ErrorCode";
    
}

public class ErrorCode
{
    public const string ValueOutOfRange = "ValueOutOfRange";
}

public enum HeroBelong
{
    Wild= 0,
    My = -1,
}

public enum ItemTypeDefine
{
    RES = 1,
    DROP_BAG = 2,
    GIVE_FAVOR = 3,
}

public enum FancyDefine
{
    WINE = 1,
    EQUIPMENT = 2,
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
    BUILD_CENTER
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
}

public class SceneDefine
{
    public const string World = "World";
    public const string Home = "Home";
    public const string GameIndex = "GameIndex";
}

public class ValueAddType
{
    public const string DayTax = "DayTax";
    public const string StoreLimit = "StoreLimit";
    public const string HourTax = "HourTax";
    public const string World = "World";
    public const string HeroMaxBlood = "HeroMaxBlood";
    public const string ReserverLimit = "ReserverLimit";
    public const string TroopCount = "TroopCount";
    public const string RecruitSecs = "RecruitSecs";
    public const string ElementAdd = "ElementAdd";
    public const string AttributeAdd = "AttributeAdd";
    public const string MarchSpeed = "MarchSpeed";
    public const string Defense = "Defense";
    public const string DeployCount = "DeployCount";
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
    public const string AtkSpeed = "AtkSpeed";
    public const string Burst = "Burst";
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

public class BuildingEffectsData
{
    public System.Collections.Generic.List<string> _changeKeys = new System.Collections.Generic.List<string>();
    public Dictionary<int, Dictionary<string, float>> CareerAttrAdds = new Dictionary<int, Dictionary<string, float>>();
    public Dictionary<string, float> ElementAdds = new Dictionary<string, float>();
    public float MarchSpeedAdd = 0;
    public int MaxBloodAdd = 0;
    public int ResLimitAdd = 0;
    public int PowerAdd = 0;
    public Dictionary<string, int> IncomeDic = new Dictionary<string, int>();
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

public class Hero
{
    public int Id;//唯一标识
    public int Level;
    public int Exp;
    public System.Collections.Generic.List<AttributeData> Attributes;
    public float MarchSpeed;
    public float ElementValue;
    public int Blood;
    public int MaxBlood;
    public int Belong;//0-在野 1-我方  >0 为对应Npc君主的ID
    public int TeamId;//上阵队伍ID 0-未上阵
    public int Favor;//好感度
    public Dictionary<string, int> GetItems;//获得过的馈赠

    public void Create(HeroConfig config)
    {
        this.Id = config.ID;
        this.Level = 1;//可以根据主角的声望来进行初始设置
        this.Exp = 0;
        this.Blood = 0;
        this.Belong = (int)HeroBelong.Wild;
        this.TeamId = 0;
        this.Favor = 0;
        this.GetItems = new Dictionary<string, int>();
        this.ComputeAttributes();
    }

    public void ComputeAttributes()
    {
        HeroConfig config = HeroConfig.Instance.GetData(this.Id);
        HeroLevelConfig configLv = HeroLevelConfig.Instance.GetData(this.Level);
        if (this.Attributes == null)
            this.Attributes = new System.Collections.Generic.List<AttributeData>();
        else
            this.Attributes.Clear();
        Dictionary<string, float> initValueDic = AttributeData.InitAttributes(config.InitAttribute);
        Dictionary<string, float> levelAddValueDic = AttributeData.InitAttributes(config.AttributeGrow);
        BuildingEffectsData bdAddData = WorldProxy._instance.GetBuildingEffects();

        foreach (string key in initValueDic.Keys)
        {
            float curAdd = 0;
            if (levelAddValueDic.TryGetValue(key, out curAdd))
            {
                float levelAdd = curAdd * (this.Level - 1);
                //等级增加的属性
                initValueDic[key] += levelAdd;
            }

            if (bdAddData.CareerAttrAdds[config.Career].TryGetValue(key, out curAdd) && this.Belong == (int)HeroBelong.My)
            {
                //建筑属性增加只有自己的英雄计算
                initValueDic[key] += curAdd;
            }

            AttributeData data = new AttributeData();
            data.Id = key;
            data.Value = initValueDic[key];
            this.Attributes.Add(data);
        }//end for

        if (this.Belong == (int)HeroBelong.My)
        {
            this.MarchSpeed = config.MarchSpeed + bdAddData.MarchSpeedAdd;
            this.ElementValue = config.ElementValue + bdAddData.ElementAdds[config.Element];
            this.MaxBlood = configLv.BloodMax + bdAddData.MaxBloodAdd;
        }
        else
        {
            this.MarchSpeed = config.MarchSpeed ;
            this.ElementValue = config.ElementValue;
            this.MaxBlood = configLv.BloodMax;
        }

    }//end function
}

