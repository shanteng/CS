using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;


public class ArmyProxy : BaseRemoteProxy
    , IConfirmListener
{
    private Dictionary<int, Dictionary<int, Army>> _datas = new Dictionary<int, Dictionary<int, Army>>();

    public static ArmyProxy _instance;
    public ArmyProxy() : base(ProxyNameDefine.ARMY)
    {
        _instance = this;
    }

    public void DoSave()
    {
        CloudDataTool.SaveFile(SaveFileDefine.Army, this._datas);
    }

    public Dictionary<int, Army> GetCityArmys(int city)
    {
        Dictionary<int, Army> armys;
        this._datas.TryGetValue(city, out armys);
        return armys;
    }

    public Army GetArmy(int id,int city)
    {
        Dictionary<int, Army> armys = this.GetCityArmys(city);
        if (armys == null)
            return null;
        Army army = null;
        if (armys.TryGetValue(id, out army))
            return army;
        return null;
    }

    public Army GetCareerDoingArmy(int career,int cityid)
    {
        Dictionary<int, Army> armys = this.GetCityArmys(cityid);
        if (armys == null)
            return null;
        foreach (Army army in armys.Values)
        {
            if (army.ReserveCount == 0)
                continue;
            ArmyConfig config = ArmyConfig.Instance.GetData(army.Id);
            if (config.Career == career)
            {
                return army;
            }
        }
        return null;
    }


    public int GetCareerRecruitCount(int career,int cityid)
    {
        Dictionary<int, Army> armys = this.GetCityArmys(cityid);
        if (armys == null)
            return 0;

        int count = 0;
        foreach (Army army in armys.Values)
        {
            if (army.ReserveCount == 0)
                continue;
            ArmyConfig config = ArmyConfig.Instance.GetData(army.Id);
            if (config.Career == career)
            {
                int recruiCount = army.ReserveCount;
                count += recruiCount;
            }
        }
        return count;
    }

 
    public bool isArmyOpen(int id)
    {
        return true;
    }

    public int GetArmyCountBy(int id,int cityid)
    {
        Dictionary<int, Army> armys = this.GetCityArmys(cityid);
        if (armys == null)
            return 0;

        foreach (Army army in armys.Values)
        {
            ArmyConfig config = ArmyConfig.Instance.GetData(army.Id);
            if (config.ID == id)
            {
                return  army.Count;
            }
        }
        return 0;
    }

    public int GetCareerRecruitVolume(int career,int city)
    {
        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects(city);
        int CareerLimit = 0;
        if (effect.RecruitVolume.TryGetValue(career, out CareerLimit) == false)
            return 0;
        return CareerLimit;
    }

    public VInt2 GetArmyCanRecruitCountBy(int id,int cityid)
    {
        VInt2 kv = new VInt2();
        ArmyConfig config = ArmyConfig.Instance.GetData(id);
        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects(cityid);

        int CareerLimit = this.GetCareerRecruitVolume(config.Career, cityid);
        if (CareerLimit == 0)
            return kv;
        
        int CareerDoingCount = this.GetCareerRecruitCount(config.Career, cityid);//正在招募
        if (CareerDoingCount > 0)
            return kv;

        kv.x = RoleProxy._instance.GetCanDoMinCountBy(config.Cost);//计算当前资源量可以招募的上限
        kv.y = CareerLimit;//最大数量
        if (kv.x > CareerLimit)
            kv.x = CareerLimit;
        return kv;
    }

    public int GetOneRecruitSecs(int cityid)
    {
        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects(cityid);
        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.RecruitSecs);
        int nSecs = cfgconst.IntValues[0];
        int RecruitOneSces = Mathf.FloorToInt(nSecs * (1f - effect.RecruitReduceRate));
        return RecruitOneSces;
    }

    public void RecruitArmy(Dictionary<string, object> vo)
    {
        int cityid = (int)vo["cityid"];
        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects(cityid);
        int id = (int)vo["id"];
        int count = (int)vo["count"];

        if (count <= 0)
            return;

        ArmyConfig config = ArmyConfig.Instance.GetData(id);
        int careerRecruitNow = this.GetCareerRecruitCount(config.Career, cityid);
        if (careerRecruitNow > 0)
        {
            string str = CareerDefine.GetName(config.Career);
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.CareerRecruitYet, str);
            return;
        }

        int volume = this.GetCareerRecruitVolume(config.Career, cityid);
        if (count > volume)
        {
            string str = CareerDefine.GetName(config.Career);
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.CareerRecruitLimit, volume);
            return;
        }


        //扣除消耗
        bool isCostEnough = RoleProxy._instance.TryDeductCost(config.Cost,count);
        if (isCostEnough == false)
        {
            return;
        }

        Army army = this.GetArmy(id,cityid);
        if (army == null)
        {
            army = new Army();
            army.Init(id);
            army.TimeKey = UtilTools.GenerateUId();
            Dictionary<int, Army> armys = this.GetCityArmys(cityid);
            if (armys == null)
            {
                armys = new Dictionary<int, Army>();
                this._datas.Add(cityid, armys);
            }
            armys[id] = army;
        }

        army.RecruitOneSces = this.GetOneRecruitSecs(cityid);
        army.RecruitStartTime = GameIndex.ServerTime;
        army.RecruitExpireTime = army.RecruitStartTime + army.RecruitOneSces * count;
        army.ReserveCount = count;
        army.CanAccept = false;
        this.AddOneTimeListener(army);
        this.DoSave();
        MediatorUtil.SendNotification(NotiDefine.ArmyStateChange, army);
    }

    public void SpeedUpRecruitArmy(VInt2 kv, bool isIgnorlNotice = false)
    {
        int cityid = kv.x;
        int id = kv.y;
        Army army = this.GetArmy(id, cityid);
        ArmyConfig config = ArmyConfig.Instance.GetData(army.Id);
        if (army == null || army.ReserveCount == 0)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.NoArmyRecruit, config.Name);
            return;
        }

        long leftSecs = army.RecruitExpireTime - GameIndex.ServerTime;
        if (leftSecs <= 0)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.FinishArmyRecruit);
            return;
        }
        bool IsOk = RoleProxy._instance.TrySpeedUp((int)leftSecs,this.OnSpeedSure, army);
    }

    public void OnSpeedSure(object param)
    {
        Army army = (Army)param;
        //时间中心去掉
        MediatorUtil.SendNotification(NotiDefine.RemoveTimestepCallback, army.TimeKey);
        this.OnRecruitExpireFinish(army);
        this.DoSave();
    }

    public void CancelRecruitArmy(VInt2 kv, bool isIgnorlNotice = false)
    {
        Army army = this.GetArmy(kv.x,kv.y);
        ArmyConfig config = ArmyConfig.Instance.GetData(army.Id);
        if (army == null || army.ReserveCount == 0)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.NoArmyRecruit, config.Name);
            return;
        }

        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.CancelArmyReturnRate);
        int rate = cfgconst.IntValues[0];
        if (isIgnorlNotice == false)
        {
            PopupFactory.Instance.ShowConfirm(LanguageConfig.GetLanguage(LanMainDefine.CancelArmyNotice, rate), this, "CancelRecruitArmy", kv);
            return;
        }

        //计算返还
        int count = army.ReserveCount * rate / 100;
        RoleProxy._instance.TryAddNumValue(config.Cost, count);

        army.RecruitExpireTime = 0;
        army.RecruitStartTime = 0;
        army.ReserveCount = 0;
        army.CanAccept = false;
        this.DoSave();
        MediatorUtil.SendNotification(NotiDefine.ArmyStateChange, army);
    }

    public void ChangeArmyCount(int city,int id, int changeCount)
    {
        if (changeCount == 0)
            return;
        Army army = this.GetArmy(id, city);
        if (army == null)
            return;
        army.Count += changeCount;
        if (army.Count < 0)
            army.Count = 0;
        this.DoSave();
        RoleProxy._instance.ComputePower(true);
    }

    public void HarvestArmy(VInt2 kv)
    {
        int id = kv.x;
        int city = kv.y;
        Army army = this.GetArmy(id, city);
        if (army == null)
            return;
        ArmyConfig config = ArmyConfig.Instance.GetData(army.Id);
        if (army == null || army.CanAccept == false)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.NoArmyCanHarvest, config.Name);
            return;
        }

        if (army.ReserveCount > 0)
        {
            PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.ArmyHarvest, army.ReserveCount, config.Name));
            RoleProxy._instance.AddLog(LogType.HarvestArmy, LanguageConfig.GetLanguage(LanMainDefine.ArmyHarvest, army.ReserveCount, config.Name));
        }
            

        army.Count += army.ReserveCount;
        army.RecruitExpireTime = 0;
        army.RecruitStartTime = 0;
        army.ReserveCount = 0;
        army.CanAccept = false;

        this.DoSave();
        MediatorUtil.SendNotification(NotiDefine.ArmyStateChange, army);
        RoleProxy._instance.ComputePower(true);
    }

    public int GetPower()
    {
        int power = 0;
        foreach (Dictionary<int,Army> armys in this._datas.Values)
        {
            foreach (Army am in armys.Values)
            {
                ArmyConfig config = ArmyConfig.Instance.GetData(am.Id);
                int cur = config.Power * am.Count;
                power += cur;
            }
        }
        return power;
    }

    public void OnConfirm(ConfirmData data)
    {
     
        if (data.userKey.Equals("CancelRecruitArmy"))
        {
            VInt2 kv = (VInt2)data.param;
            this.CancelRecruitArmy(kv, true);
        }
    }

    public void LoadAllArmys()
    {
        this._datas.Clear();
        string json = CloudDataTool.LoadFile(SaveFileDefine.Army);
        if (json.Equals(string.Empty) == false)
        {
           this._datas = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, Army>>>(json);
        }

        foreach (Dictionary<int, Army> armys in this._datas.Values)
        {
            foreach (Army army in armys.Values)
            {
                if (army.ReserveCount == 0)
                    continue;
                long leftSecs = army.RecruitExpireTime - GameIndex.ServerTime;
                if (leftSecs <= 0)
                {
                    this.OnRecruitExpireFinish(army);
                }
                else
                {
                    this.AddOneTimeListener(army);
                }
            }
        }
        //添加招募的時間監聽
        this.SendNotification(NotiDefine.LoadAllArmyResp);
    }

    public void OnRecruitExpireFinish(Army army)
    {
        Army curArmy = this.GetArmy(army.Id, army.CityID);
        curArmy.CanAccept = true;
        curArmy.RecruitExpireTime = 0;
        curArmy.RecruitStartTime = 0;
       

        string cityName = WorldProxy._instance.GetCityName(army.CityID);
        VInt2 pos = WorldProxy._instance.GetCityCordinate(army.CityID);
        ArmyConfig config = ArmyConfig.Instance.GetData(army.Id);

        string bdKey = WorldProxy._instance.GetArmyBuildingBy(config.Career, army.CityID);

        string notice = LanguageConfig.GetLanguage(LanMainDefine.ArmyRecruitFinish, cityName,config.Name, curArmy.ReserveCount);
        PopupFactory.Instance.ShowNotice(notice);
        RoleProxy._instance.AddLog(LogType.FinshArmy, notice, pos, bdKey);

        this.DoSave();
        MediatorUtil.SendNotification(NotiDefine.ArmyStateChange, army);
    }

    
    private void AddOneTimeListener(Army data)
    {
        TimeCallData dataTime = new TimeCallData();
        dataTime._key = data.TimeKey;
        dataTime._notifaction = NotiDefine.ArmyRecruitExpireReachedNoti;
        dataTime.TimeStep = data.RecruitExpireTime;
        dataTime._param = data;
        MediatorUtil.SendNotification(NotiDefine.AddTimestepCallback, dataTime);
    }


}//end class
