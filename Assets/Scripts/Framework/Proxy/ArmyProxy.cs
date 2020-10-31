using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;


public class ArmyProxy : BaseRemoteProxy
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

    public Army GetCareerDoingArmy(int id)
    {
        //还未实现
        Army army = null;
        if (this._datas.TryGetValue(id, out army))
            return army;
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
            if (army.RecruitExpireTime == 0)
                continue;
            ArmyConfig config = ArmyConfig.Instance.GetData(army.Id);
            if (config.Career == career)
            {
                int recruiCount = this.ComputeRecruitCount(army);
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
            if (army.RecruitExpireTime == 0)
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
            if (army.RecruitExpireTime == 0)
                continue;
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

    public void RecruitArmy(Dictionary<string, object> vo)
    {
        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects();
        int id = (int)vo["id"];
        int count = 100;// (int)vo["count"];

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
            this._datas[id] = army;
        }


        army.RecruitOneSces = this.GetOneRecruitSecs();
        army.RecruitStartTime = GameIndex.ServerTime;
        army.RecruitExpireTime = army.RecruitStartTime + army.RecruitOneSces * count;
        this.AddOneTimeListener(army);
        this.DoSave();
    }

    public int GetOneRecruitSecs()
    {
        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects();
        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.RecruitSecs);
        int nSecs = cfgconst.IntValues[0];
        int RecruitOneSces = Mathf.FloorToInt(nSecs * (1f - effect.RecruitReduceRate));
        return RecruitOneSces;
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
            if (army.RecruitExpireTime == 0)
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
        int recruiCount = this.ComputeRecruitCount(army);
        army.ReserveCount = recruiCount;
        //army.RecruitExpireTime = 0;等领取后在清理掉时间
       // army.RecruitStartTime = 0;
        this.DoSave();
        this.SendNotification(NotiDefine.ArmyRecruitFinishedNoti, id);
    }

    public int ComputeRecruitCount(Army army)
    {
        long passSecs = army.RecruitExpireTime - army.RecruitStartTime;
        int recruiCount = Mathf.FloorToInt((float)passSecs / (float)army.RecruitOneSces);
        return recruiCount;
    }

    private void AddOneTimeListener(Army data)
    {
        TimeCallData dataTime = new TimeCallData();
        dataTime._key = UtilTools.GenerateUId();
        dataTime._notifaction = NotiDefine.ArmyRecruitExpireReachedNoti;
        dataTime.TimeStep = data.RecruitExpireTime;
        dataTime._param = data.Id;
        MediatorUtil.SendNotification(NotiDefine.AddTimestepCallback, dataTime);
    }


}//end class
