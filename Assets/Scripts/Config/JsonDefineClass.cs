﻿
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using UnityEngine;

public enum JsonKeyType
{
    INT,
    STRING,
}

public class JsonNameDefine
{
    public static string Language = "Language";
    public static string LanError = "LanError";
    public static string ItemInfo = "ItemInfo";
    public static string Building = "Building";
    public static string BuildingUpgrade = "BuildingUpgrade";
    public static string World = "World";
    public static string Const = "Const";
    public static string RoleLevel = "RoleLevel";
    public static string Hero = "Hero";
    public static string HeroLevel = "HeroLevel";
    public static string HeroStar = "HeroStar";
    public static string HeroPool = "HeroPool";
    public static string Army = "Army";
    public static string FavorLevel = "FavorLevel";
    public static string CareerEvaluate = "CareerEvaluate";
    public static string City = "City";
    public static string NpcTeam = "NpcTeam";
    public static string BattleScene = "BattleScene";

    public static string Skill = "Skill";
    public static string SkillEffect = "SkillEffect";
    public static string SkillLevel = "SkillLevel";

    public static string RangeFunction = "RangeFunction";
    public static string Question = "Question";

    public static string Name = "Name";
    public static string Round = "Round";

    public static JArray JsonRead(string name)
    {
        string json = "";
        string path = "Config/" + name ;
        TextAsset text = Resources.Load<TextAsset>(path);
        if (text == null)
            return null;
        json = text.text;
        JArray jArray = JArray.Parse(json);
        return jArray;
    }
}

#region ConfigClassDefine
public class LanErrorConfig : Config<LanErrorConfig>
{
    public string Value;
    public LanErrorConfig() : base(JsonNameDefine.LanError, JsonKeyType.STRING) { }
    public static string GetLanguage(string key, params object[] paramName)
    {
        LanErrorConfig config = LanErrorConfig.Instance.GetData(key);
        if (config == null)
            return "";
        return UtilTools.format(config.Value, paramName);
    }
}

public class LanguageConfig : Config<LanguageConfig>
{
    public string Value;
    public LanguageConfig() : base(JsonNameDefine.Language, JsonKeyType.STRING) { }
    public static string GetLanguage(string key, params object[] paramName)
    {
        LanguageConfig config = LanguageConfig.Instance.GetData(key);
        if (config == null)
            return "";
        return UtilTools.format(config.Value, paramName);
    }
}

public class ConstConfig : Config<ConstConfig>
{
    public string[] StringValues;
    public int[] IntValues;

    public ConstConfig() : base(JsonNameDefine.Const, JsonKeyType.STRING) { }
    public int ValueInt
    {
        get
        {
            if (this.IntValues.Length > 0)
                return this.IntValues[0];
            return 0;
        }
    }

    public string ValueStr
    {
        get
        {
            if (this.StringValues.Length > 0)
                return this.StringValues[0];
            return "";
        }
    }

}

public class ArmyConfig : Config<ArmyConfig>
{
    public string Name;
    public string Desc;
    public string Model;
    public int Career;
    public int Star;
    public float SpeedRate;
    public float Attack;
    public float Defense;

    public float Blood;
    public int Load;
    public int Power;
 
    public string[] Cost;
    public int UnlockTech;//招募所需科技ID

    public float FightOneExpAdd;
    public ArmyConfig() : base(JsonNameDefine.Army) { }
}

public class SkillConfig : Config<SkillConfig>
{
    public int Type;
    public string Name;
    public string EffectRes;
    public string ReleaseTerm;

    public string[] Descs;
    public int[] EffectIDs;
    public int MpCost;
    public SkillConfig() : base(JsonNameDefine.Skill) { }
}

public class SkillEffectConfig : Config<SkillEffectConfig>
{
    public string Type;
    public string Name;
    public string Desc;

    public string Target;
    public int JudgeInDemageRange;
    public string Value;
    public string[] ComputeParams;
    public string Rate;
    public string Active_Rate;
    public string Active_Mode;
    public int TriggerInSelf;

    public int Duration;
    public SkillEffectConfig() : base(JsonNameDefine.SkillEffect) { }
}

public class SkillLevelConfig : Config<SkillLevelConfig>
{
    public int Skill;
    public int Level;
    public string AttackRangeID;
    public string DemageRangeID;
    public SkillLevelConfig() : base(JsonNameDefine.SkillLevel) { }
}

public class RangeFunctionConfig : Config<RangeFunctionConfig>
{
    public string Name;
    public string Function;
    public int[] ComputeParams;
    public RangeFunctionConfig() : base(JsonNameDefine.RangeFunction, JsonKeyType.STRING) { }
}

public class BattleSceneConfig : Config<BattleSceneConfig>
{
    public string Scene;
    public int[] RowCol;
    public int[] AttackBorn;
    public int[] DefenseBorn;
    public BattleSceneConfig() : base(JsonNameDefine.BattleScene) { }
}

public class RoleLevelConfig : Config<RoleLevelConfig>
{
    public int Exp;
    public int Power;

    public RoleLevelConfig() : base(JsonNameDefine.RoleLevel) { }
}

public class NpcTeamConfig : Config<NpcTeamConfig>
{
    public int Type;
    public int Hero;
    public int Level;
    public int[] SkillLvs;
    public int Army;
    public int Count;
   
    public NpcTeamConfig() : base(JsonNameDefine.NpcTeam) { }
}

public class CityConfig : Config<CityConfig>
{
    public int Type;
    public string Name;
    public int SubType;
    public int[] Range;
    public string Model;
    public int[] Position;
    public int[] NpcTeams;
    public int[] NpcBorns;
    public string[] Buildings;
    public int Power;//
    public string[] QuestDrops;
    public string[] QuestTalents;//TalentDefine
    public string[] AttackDrops;//
    public int BattleSceneID;
    public CityConfig() : base(JsonNameDefine.City) { }

    public static BuildingUpgradeConfig GetConfig(int id, int level)
    {
        int levelid = id * 100 + level;
        return BuildingUpgradeConfig.Instance.GetData(levelid);
    }
}

public class LotteryNameConfig : Config<LotteryNameConfig>
{
    public string Name;
    public string Group;

    public LotteryNameConfig() : base(JsonNameDefine.Name) { }
}

public class LotteryRoundConfig : Config<LotteryRoundConfig>
{
    public string Title;
    public string Name;
    public int Count;
    public int PageCount;
    public int Image;
    public LotteryRoundConfig() : base(JsonNameDefine.Round) { }
}

public class QuestionConfig : Config<QuestionConfig>
{
    public string Question;
    public string Selection;
    public string Answer;

    public QuestionConfig() : base(JsonNameDefine.Question) { }
}



public class HeroLevelConfig : Config<HeroLevelConfig>
{
    public int Exp;
    public int BloodMax;
    public float RangeRate;

    public HeroLevelConfig() : base(JsonNameDefine.HeroLevel) { }
}

public class FavorLevelConfig : Config<FavorLevelConfig>
{
    public string Name;
    public int[] FavorRange;
    public int TalkAdd;
    public FavorLevelConfig() : base(JsonNameDefine.FavorLevel) { }
}

public class CareerEvaluateConfig : Config<CareerEvaluateConfig>
{
    public int Percent;
    public CareerEvaluateConfig() : base(JsonNameDefine.CareerEvaluate) { }
}

public class HeroConfig : Config<HeroConfig>
{
    public string Name;
    public string Model;
    public int[] CareerRates;
    public int Star;

    public int InitBelong;
    public string Element;
    public float Speed;//真实攻速为和Army的SpeedType相加得到
    public int[] Skills;
    public string[] Cost;
    
    public int NeedPower;//招募到达声望
    public int FavorLevel;//招募或者好高度高于
    public int Fancy;//FancyDefine

    public string[] Talents;//TalentDefine
    public int Lucky;//
    public int DarkSide;//是否为心魔

    public string AttackRangeID;
    public string DemageRangeID;

    public HeroConfig() : base(JsonNameDefine.Hero) { }
}

public class HeroStarConfig : Config<HeroStarConfig>
{
    public string[] BaseDemage;
    public string[] GrowDemage;
    public int ElementValue;
    public float RangeBase;
    public HeroStarConfig() : base(JsonNameDefine.HeroStar) { }
  
}

public class HeroPoolConfig : Config<HeroPoolConfig>
{
    public int[] QualtyRate;
    public HeroPoolConfig() : base(JsonNameDefine.HeroPool) { }
}

public class BuildingConfig : Config<BuildingConfig>
{
    public string Prefab;
    public string AddType;
    public string Name;
    public string Desc;
    public string[] Condition;
    public int MaxLevel;
    public int RowCount;
    public int ColCount;
    public string[] AddDescs;
    public int Type;
    public int RemoveType;
    public BuildingConfig() : base(JsonNameDefine.Building) { }

}

public class BuildingUpgradeConfig : Config<BuildingUpgradeConfig>
{
    public int BuildingID;//Id
    public int Level;//
    public int NeedTime;//升级所需时间
    public int Durability;//耐久度
    public string[] Cost;
    public int Power;//
    public string[] AddValues;
    public int[] Condition;

    public int[] Parts;
    public BuildingUpgradeConfig() : base(JsonNameDefine.BuildingUpgrade) { }

    public static BuildingUpgradeConfig GetConfig(int id, int level)
    {
        int levelid = id * 100 + level;
        return BuildingUpgradeConfig.Instance.GetData(levelid);
    }
}

public class WorldConfig : Config<WorldConfig>
{
    public string Scene;
    public string Name;
    public string Desc;
 
    public int MaxRowCount;
    public int MaxColCount;


    public WorldConfig() : base(JsonNameDefine.World) { }
}

public class ItemInfoConfig : Config<ItemInfoConfig>
{
    public string Name;//Id
    public string Desc;//
    public string Icon;//
    public int Type;//升级所需时间 ItemTypeDefine
    public int SubType;//
    public int Quality;//耐久度
    public int isMaxLimit;//
    public string[] Values;
    public string[] BelongToHeros;

    public ItemInfoConfig() : base(JsonNameDefine.ItemInfo,JsonKeyType.STRING) { }

    public static string GetEquipTypeName(EquipType type)
    {
        if (type == EquipType.Weapon)
            return LanguageConfig.GetLanguage(LanMainDefine.Weapon);
        if (type == EquipType.Armor)
            return LanguageConfig.GetLanguage(LanMainDefine.Armor);
        if (type == EquipType.Horse)
            return LanguageConfig.GetLanguage(LanMainDefine.Horse);
        return type.ToString();
    }
}
#endregion

public class Config<T> where T : new()
{
    public string jName;
    public int ID;
    public string IDs;
    public Dictionary<int, T> _configDic;
    public Dictionary<string, T> _configStringDic;

    private JsonKeyType keyType;

    private static readonly object sycObj = new object();
    private static T t;
    private bool _isInit = false;
    public static T Instance
    {
        get
        {
            if (t == null)
            {
                lock (sycObj)
                {
                    if (t == null)
                    {
                        t = new T();
                    }
                }
            }
            return t;
        }
    }

    public Config(string jName, JsonKeyType tt = JsonKeyType.INT)
    {
        this.jName = jName;
        this.keyType = tt;
        if (tt == JsonKeyType.INT)
            this._configDic = new Dictionary<int, T>();
        else if (tt == JsonKeyType.STRING)
            this._configStringDic = new Dictionary<string, T>();
    }
    private void Init()
    {
        if (this._isInit)
            return;
        this._isInit = true;
        //读取Json
        JArray list = JsonNameDefine.JsonRead(this.jName);
        foreach (var item in list)
        {
            T oneJson = item.ToObject<T>();
            if (this.keyType == JsonKeyType.INT)
            {
                int ID = item.Value<int>("ID");
                this._configDic[ID] = oneJson;
            }
            else if (this.keyType == JsonKeyType.STRING)
            {
                string IDs = item.Value<string>("IDs");
                this._configStringDic[IDs] = oneJson;
            }
        }//end for
    }


    public Dictionary<int,T> getDataArray()
    {
        this.Init();
        return this._configDic;
    }

    public Dictionary<string, T> getStrDataArray()
    {
        this.Init();
        return this._configStringDic;
    }

    public T GetData(int id)
    {
        this.Init();
        T config;
        if (this._configDic.TryGetValue(id, out config))
            return config;
        return default(T);
    }

    public T GetData(string ids)
    {
        this.Init();
        T config;
        if (this._configStringDic.TryGetValue(ids, out config))
            return config;
        return default(T);
    }
}

