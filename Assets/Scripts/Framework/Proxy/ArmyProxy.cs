using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;


public class ArmyProxy : BaseRemoteProxy
    ,IConfirmListener
{
    private Dictionary<int, Army> _datas = new Dictionary<int, Army>();
  
    public static ArmyProxy _instance;
    public ArmyProxy() : base(ProxyNameDefine.ARMY)
    {
        _instance = this;
    }

    public void DoSave()
    {
        CloudDataTool.SaveFile(SaveFileDefine.Army, this._datas);
    }

    public Army GetArmy(int id)
    {
        Army army = null;
        if (this._datas.TryGetValue(id, out army))
            return army;
        return null;
    }

    public Army GetCareerDoingArmy(int career)
    {
        foreach (Army army in this._datas.Values)
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

    public Dictionary<int, Army> GeAllArmys()
    {
        return this._datas;
    }

 

    public int GetCareerRecruitCount(int career)
    {
        int count = 0;
        foreach (Army army in this._datas.Values)
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

    public int GetArmyTotleCount(int career = 0)
    {
        int count = 0;
        foreach (Army army in this._datas.Values)
        {
            if (army.ReserveCount == 0)
                continue;
            ArmyConfig config = ArmyConfig.Instance.GetData(army.Id);
            if (config.Career == career || career == 0)
            {
                count += army.Count;
            }
        }
        return count;
    }

    public bool isArmyOpen(int id)
    {
        return true;
    }

    public int GetArmyCountBy(int id)
    {
        foreach (Army army in this._datas.Values)
        {
            ArmyConfig config = ArmyConfig.Instance.GetData(army.Id);
            if (config.ID == id)
            {
                return  army.Count;
            }
        }
        return 0;
    }

    public int GetCareerRecruitVolume(int career)
    {
        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects();
        int CareerLimit = 0;
        if (effect.RecruitVolume.TryGetValue(career, out CareerLimit) == false)
            return 0;
        return CareerLimit;
    }

    public VInt2 GetArmyCanRecruitCountBy(int id)
    {
        VInt2 kv = new VInt2();
        ArmyConfig config = ArmyConfig.Instance.GetData(id);
        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects();

        int CareerLimit = this.GetCareerRecruitVolume(config.Career);
        if (CareerLimit == 0)
            return kv;
        
        int CareerDoingCount = this.GetCareerRecruitCount(config.Career);//正在招募
        if (CareerDoingCount > 0)
            return kv;

        kv.x = RoleProxy._instance.GetCanDoMinCountBy(config.Cost);//计算当前资源量可以招募的上限
        kv.y = CareerLimit;//最大数量
        if (kv.x > CareerLimit)
            kv.x = CareerLimit;
        return kv;
    }

    public int GetOneRecruitSecs()
    {
        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects();
        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.RecruitSecs);
        int nSecs = cfgconst.IntValues[0];
        int RecruitOneSces = Mathf.FloorToInt(nSecs * (1f - effect.RecruitReduceRate));
        return RecruitOneSces;
    }

    public void RecruitArmy(Dictionary<string, object> vo)
    {
        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects();
        int id = (int)vo["id"];
        int count = (int)vo["count"];

        if (count <= 0)
            return;

        ArmyConfig config = ArmyConfig.Instance.GetData(id);
        int careerRecruitNow = this.GetCareerRecruitCount(config.Career);
        if (careerRecruitNow > 0)
        {
            string str = CareerDefine.GetName(config.Career);
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.CareerRecruitYet, str);
            return;
        }

        int volume = this.GetCareerRecruitVolume(config.Career);
        if (count > volume)
        {
            string str = CareerDefine.GetName(config.Career);
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.CareerRecruitLimit, volume);
            return;
        }

        int totleCount = this.GetArmyTotleCount();
        if (totleCount >= effect.ArmyLimit)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.CityArmyFull,effect.ArmyLimit);
            return;
        }

        //扣除消耗
        bool isCostEnough = RoleProxy._instance.TryDeductCost(config.Cost,count);
        if (isCostEnough == false)
        {
            return;
        }

        Army army = this.GetArmy(id);
        if (army == null)
        {
            army = new Army();
            army.Init(id);
            army.TimeKey = UtilTools.GenerateUId();
            this._datas[id] = army;
        }

        army.RecruitOneSces = this.GetOneRecruitSecs();
        army.RecruitStartTime = GameIndex.ServerTime;
        army.RecruitExpireTime = army.RecruitStartTime + army.RecruitOneSces * count;
        army.ReserveCount = count;
        army.CanAccept = false;
        this.AddOneTimeListener(army);
        this.DoSave();
        MediatorUtil.SendNotification(NotiDefine.RecruitArmyResp, army.Id);
        MediatorUtil.SendNotification(NotiDefine.ArmyStateChange, id);
    }

    public void SpeedUpRecruitArmy(int id, bool isIgnorlNotice = false)
    {
        Army army = this.GetArmy(id);
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
        bool IsOk = RoleProxy._instance.TrySpeedUp((int)leftSecs,this.OnSpeedSure,id);
    }

    public void OnSpeedSure(object param)
    {
        int id = (int)param;
        Army army = this.GetArmy(id);
        //时间中心去掉
        MediatorUtil.SendNotification(NotiDefine.RemoveTimestepCallback, army.TimeKey);
        this.OnRecruitExpireFinish(id);
        this.DoSave();
        MediatorUtil.SendNotification(NotiDefine.SpeedUpArmyResp, id);
        MediatorUtil.SendNotification(NotiDefine.ArmyStateChange, id);

    }

    public void CancelRecruitArmy(int id, bool isIgnorlNotice = false)
    {
        Army army = this.GetArmy(id);
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
            PopupFactory.Instance.ShowConfirm(LanguageConfig.GetLanguage(LanMainDefine.CancelArmyNotice, rate), this, "CancelRecruitArmy", id);
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
        MediatorUtil.SendNotification(NotiDefine.CancelArmyResp,id);
        MediatorUtil.SendNotification(NotiDefine.ArmyStateChange, id);
    }


    public void HarvestArmy(int id,bool ignorlOverflow = false)
    {
        Army army =  this.GetArmy(id);
        ArmyConfig config = ArmyConfig.Instance.GetData(army.Id);
        if (army == null || army.CanAccept == false)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.NoArmyCanHarvest, config.Name);
            return;
        }

        int totleCount = this.GetArmyTotleCount();
        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects();
        int afterCount = totleCount + army.ReserveCount;
        int overFlow = afterCount - effect.ArmyLimit;
        if (overFlow > 0 && ignorlOverflow == false)
        {
            PopupFactory.Instance.ShowConfirm(LanguageConfig.GetLanguage(LanMainDefine.ArmyOverFlowHarvest, army.ReserveCount,overFlow), this, "HarvestArmyOverFlow", id);
            return;
        }

        if (overFlow < 0)
            overFlow = 0;

        int addCount = army.ReserveCount - overFlow;
        army.Count += addCount;
        army.RecruitExpireTime = 0;
        army.RecruitStartTime = 0;
        army.ReserveCount = 0;
        army.CanAccept = false;

        if (addCount > 0)
            PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.ArmyHarvest, addCount, config.Name));

        this.DoSave();
        MediatorUtil.SendNotification(NotiDefine.HarvestArmyResp, addCount);
        MediatorUtil.SendNotification(NotiDefine.ArmyStateChange,id);
    }

    public void OnConfirm(ConfirmData data)
    {
        if (data.userKey.Equals("HarvestArmyOverFlow"))
        {
            int id = (int)data.param;
            this.HarvestArmy(id, true);
        }
        else if (data.userKey.Equals("CancelRecruitArmy"))
        {
            int id = (int)data.param;
            this.CancelRecruitArmy(id, true);
        }
    }

    public void LoadAllArmys()
    {
        this._datas.Clear();
        string json = CloudDataTool.LoadFile(SaveFileDefine.Army);
        if (json.Equals(string.Empty) == false)
        {
           this._datas = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, Army>>(json);
        }

        foreach (Army army in this._datas.Values)
        {
            if (army.ReserveCount == 0)
                continue;
            long leftSecs = army.RecruitExpireTime - GameIndex.ServerTime;
            if (leftSecs <= 0)
            {
                this.OnRecruitExpireFinish(army.Id);
            }
            else
            {
                this.AddOneTimeListener(army);
            }
        }
        //添加招募的時間監聽
        this.SendNotification(NotiDefine.LoadAllArmyResp);
    }

    public void OnRecruitExpireFinish(int id)
    {
        Army army = this.GetArmy(id);
        if (army == null)
            return;
        army.CanAccept = true;
        army.RecruitExpireTime = 0;
        army.RecruitStartTime = 0;
        this.DoSave();
        this.SendNotification(NotiDefine.ArmyRecruitFinishedNoti, id);
        MediatorUtil.SendNotification(NotiDefine.ArmyStateChange, id);
    }

    
    private void AddOneTimeListener(Army data)
    {
        TimeCallData dataTime = new TimeCallData();
        dataTime._key = data.TimeKey;
        dataTime._notifaction = NotiDefine.ArmyRecruitExpireReachedNoti;
        dataTime.TimeStep = data.RecruitExpireTime;
        dataTime._param = data.Id;
        MediatorUtil.SendNotification(NotiDefine.AddTimestepCallback, dataTime);
    }


}//end class
