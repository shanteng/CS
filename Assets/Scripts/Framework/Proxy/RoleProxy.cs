﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using DG.Tweening.Plugins;
using Newtonsoft.Json.Utilities;
using SMVC.Patterns;
using UnityEngine;
using UnityEngine.PlayerIdentity;

public class RoleInfo
{
    public string UID;
    public string Name;
    public int Level;//
    public int Exp;//
    public int Power;//声望
    public List<CostData> ItemList;//属性道具
    public List<HourAwardData> AddUpAwards;//当前可以领取的数值
    public int ResValueLimit;//上限
}


//时间戳回调管理
public class RoleProxy : BaseRemoteProxy
{
    private Dictionary<string, int> IncomeDic = new Dictionary<string, int>();
    private List<string> _LimitValueKeys = new List<string>();
    private RoleInfo _role;
    public static RoleProxy _instance;
    public RoleProxy() : base(ProxyNameDefine.ROLE)
    {
        _instance = this;
    }

    public RoleInfo Role => this._role;
    public int ResValueLimit => this._role.ResValueLimit;

    public void ComputeIncome()
    {
        this.IncomeDic.Clear();


        //建筑加成
        BuildingEffectsData datas = WorldProxy._instance.GetBuildingEffects();
        foreach (string key in datas.IncomeDic.Keys)
        {
            int oldvalue = 0;
            if (this.IncomeDic.TryGetValue(key, out oldvalue) == false)
                oldvalue = 0;
            this.IncomeDic[key] = datas.IncomeDic[key].Count+ oldvalue;
        }

        //占领的野外加成

    }

    public void ComputeBuildingEffect()
    {
        //计算收益和可领取的收益
        BuildingEffectsData datas = WorldProxy._instance.GetBuildingEffects();
        this.ComputeIncome();
        
        ConstConfig configCst = ConstConfig.Instance.GetData(ConstDefine.InitLimit);
        this._role.ResValueLimit = configCst.ValueInt;
        this._role.ResValueLimit += datas.ResLimitAdd;

        this.ComputePower(false);
        this.UpdateHourAward();
        this.DoSaveRole();
        this.SendNotification(NotiDefine.ResLimitHasUpdated);
    }

    public void ComputePower(bool save)
    {
        BuildingEffectsData datas = WorldProxy._instance.GetBuildingEffects();
        RoleLevelConfig config = RoleLevelConfig.Instance.GetData(this._role.Level);
        this._role.Power = config.Power + datas.PowerAdd;
        if (save)
            this.DoSaveRole();
    }

   

    private void UpdateHourAward()
    {
        List<HourAwardData> AddUpAwards = this._role.AddUpAwards;
        int count = AddUpAwards.Count;
        for (int i = 0; i < count; ++i)
        {
            if (AddUpAwards[i].generate_time > 0)
            {
                long passSecs = GameIndex.ServerTime - AddUpAwards[i].generate_time;
                int addValue = Mathf.CeilToInt(passSecs * AddUpAwards[i].base_secs_value);
                if (addValue < 0)
                    addValue = 0;
                AddUpAwards[i].add_up_value += addValue;
            }
            AddUpAwards[i].generate_time = GameIndex.ServerTime;
            AddUpAwards[i].base_secs_value = (float)this.GetHourInCome(AddUpAwards[i].id) / 3600f;
        }
    }

    private int IsAddOverLimit(string key, int addValue)
    {
        int limit = RoleProxy._instance.ResValueLimit;
        CostData currentValue = this.GetNumberValueData(key);
        int afterValue = currentValue.count + addValue;
        bool isOverLimit =  this._LimitValueKeys.Contains(key) && afterValue > limit;
        if (isOverLimit)
        {
            if (currentValue.count >= limit)
            {
                string attrName = ItemKey.GetName(key);
                PopupFactory.Instance.ShowErrorNotice(ErrorCode.ValueOutOfRange, attrName);
                return -1;
            }
            int overNum = afterValue - limit;
            return overNum;
        }
        return 0;
    }


    public int GetHourAwardValue(string key)
    {
        List<HourAwardData> AddUpAwards = this._role.AddUpAwards;
        int count = AddUpAwards.Count;
        for (int i = 0; i < count; ++i)
        {
            if (AddUpAwards[i].id.Equals(key))
            {
                long passSecs = GameIndex.ServerTime - AddUpAwards[i].generate_time;
                int addValue = Mathf.CeilToInt(passSecs * AddUpAwards[i].base_secs_value) + AddUpAwards[i].add_up_value;
                return addValue;
            }
        }
        return 0;
    }

    public void AcceptHourAward(string key)
    {
        //重新计算收益和可领取的收益
        BuildingEffectsData effects = WorldProxy._instance.GetBuildingEffects();
        List<HourAwardData> AddUpAwards = this._role.AddUpAwards;
        List<CostData> awards = new List<CostData>();
        int count = AddUpAwards.Count;
        for (int i = 0; i < count; ++i)
        {
            if (AddUpAwards[i].id.Equals(key))
            {
                long passSecs = GameIndex.ServerTime - AddUpAwards[i].generate_time;
                int addValue = Mathf.CeilToInt(passSecs * AddUpAwards[i].base_secs_value) + AddUpAwards[i].add_up_value;
                if (addValue > 0)
                {
                    IncomeData bdIncome;
                    if (effects.IncomeDic.TryGetValue(key, out bdIncome) && addValue > bdIncome.StoreLimit)
                    {
                        //只领取存储上限
                        addValue = bdIncome.StoreLimit;
                    }
                    
                    //判断是否超过资源上限
                    int overNum = this.IsAddOverLimit(key,addValue);
                    if (overNum < 0)//资源本身已经到达上限
                        return;

                    int currentAdd = addValue;
                    if (overNum > 0)//只领取未超出部分
                        currentAdd -= overNum;

                    CostData data = new CostData();
                    data.id = AddUpAwards[i].id;
                    data.count = currentAdd;
                    awards.Add(data);

                    //计算剩下的时间
                    int leftValue = addValue - currentAdd;
                    int LeftGeneratePassSecs = Mathf.CeilToInt(leftValue / AddUpAwards[i].base_secs_value);
                    AddUpAwards[i].generate_time = GameIndex.ServerTime - LeftGeneratePassSecs;
                    AddUpAwards[i].add_up_value = 0;

                    this.DoSaveRole();
                }
                break;
            }//end if
        }//end for

        if (awards.Count > 0)
        {
            this.ChangeRoleNumberValue(awards);
            this.SendNotification(NotiDefine.AcceptHourAwardResp);
        }
           
    }

    public int GetCanDoMinCountBy(string[] costs)
    {
        int canDoMin = int.MaxValue;//当前拥有资源最少可以做多少个
        List<CostData> awards = new List<CostData>();
        int count = costs.Length;
        for (int i = 0; i < count; ++i)
        {
            CostData data = new CostData();
            data.Init(costs[i]);
            int myValue = this.GetNumberValue(data.id);
            int valueCanDO = myValue / data.count;
            if (valueCanDO < canDoMin)
                canDoMin = valueCanDO;
        }//end for

        if (canDoMin == int.MaxValue)
            return 0;
        return canDoMin;
    }
     
    public bool TryDeductCost(string[] costs)
    {
        List<string> attrNames = new List<string>();
        List<CostData> awards = new List<CostData>();
        int count = costs.Length;
        for (int i = 0; i < count; ++i)
        {
            CostData data = new CostData();
            data.Init(costs[i]);
            int myValue = this.GetNumberValue(data.id);
            if (myValue < data.count)
            {
                string attrName = ItemKey.GetName(data.id);
                attrNames.Add(attrName);
                continue;
            }
            data.count = -data.count;
            awards.Add(data);
        }

        if (attrNames.Count > 0)
        {
            string names = string.Join(",", attrNames);
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.CostNotEnought, names);
            return false;
        }
     
        if (awards.Count > 0)
        {
            this.ChangeRoleNumberValue(awards);
        }
        return true;
    }

    public void ChangeRoleNumberValue(List<CostData> addDatas)
    {
        Dictionary<string, int> ShowAdds = new Dictionary<string, int>();
        int count = addDatas.Count;
        for (int i = 0; i < count; ++i)
        {
            string key = addDatas[i].id;
            CostData data = this.GetNumberValueData(key);
            if (data == null)
            {
                data = new CostData();
                data.id = key;
                data.count = 0;
                this._role.ItemList.Add(data);
            }

            data.count += addDatas[i].count;
            int limit = RoleProxy._instance.ResValueLimit;
            if (this._LimitValueKeys.Contains(key) && data.count > limit)
                data.count = limit;//超过上限了
            else if (data.count < 0)
                data.count = 0;

            if (addDatas[i].count > 0)
            {
                int oldValue = 0;
                if (ShowAdds.TryGetValue(key, out oldValue) == false)
                    ShowAdds[key] = 0;
                ShowAdds[key] += addDatas[i].count;
            }

        }//end for
        this.SendNotification(NotiDefine.NumberValueHasUpdated, ShowAdds);
        this.DoSaveRole();
    }

    public CostData GetNumberValueData(string key)
    {
        foreach (CostData data in this._role.ItemList)
        {
            if (data.id.Equals(key))
                return data;
        }
        return null;
    }



    public int GetNumberValue(string key)
    {
        foreach (CostData data in this._role.ItemList)
        {
            if (data.id.Equals(key))
                return data.count;
        }
        return 0;
    }

    public int GetHourInCome(string key)
    {
        int value = 0;
        if (this.IncomeDic.TryGetValue(key, out value))
            return value;
        return 0;
    }

    public HourAwardData GetCanAcceptIncomeData(string key)
    {
        List<HourAwardData> AddUpAwards = this._role.AddUpAwards;
        int count = AddUpAwards.Count;
        for (int i = 0; i < count; ++i)
        {
            if (AddUpAwards[i].generate_time > 0 && AddUpAwards[i].id.Equals(key))
            {
                return AddUpAwards[i];
            }
        }
        return null;
    }

    public int GetCanAcceptIncomeValue(string key)
    {
        HourAwardData curData = RoleProxy._instance.GetCanAcceptIncomeData(key);
        if (curData != null && curData.generate_time > 0)
        {
            long passSecs = GameIndex.ServerTime - curData.generate_time;
            int addValue = Mathf.CeilToInt(passSecs * curData.base_secs_value) + curData.add_up_value;
            return addValue;
        }
        return 0;
    }

    public void LoadOrGenerateRole()
    {
        string json = CloudDataTool.LoadFile(SaveFileDefine.Role);
        if (json.Equals(string.Empty) == false)
        {
            this._role = Newtonsoft.Json.JsonConvert.DeserializeObject<RoleInfo>(json);
        }
        else
        {
            this._role = new RoleInfo();
           ;
            this._role.UID = PlayerIdentityManager.Current.userId;
            this._role.Name = PlayerIdentityManager.Current.displayName;
            this._role.Level = 1;
            this._role.Exp = 0;
            ConstConfig config = ConstConfig.Instance.GetData(ConstDefine.InitRes);
            int count = config.StringValues.Length;
            this._role.ItemList = new List<CostData>();

            this._role.AddUpAwards = new List<HourAwardData>();
            for (int i = 0; i < count; ++i)
            {
                CostData data = new CostData();
                data.Init(config.StringValues[i]);
                this._role.ItemList.Add(data);

                HourAwardData hourAward = new HourAwardData();
                hourAward.id = data.id;
                hourAward.add_up_value = 0;
                hourAward.base_secs_value = 0;
                hourAward.generate_time = 0;
                this._role.AddUpAwards.Add(hourAward);
            }
        }

        Dictionary<string, ItemInfoConfig> dic = ItemInfoConfig.Instance.getStrDataArray();
        foreach (ItemInfoConfig config in dic.Values)
        {
            if (config.Type == (int)ItemTypeDefine.RES && config.isMaxLimit > 0)
                this._LimitValueKeys.Add(config.IDs);
        }


        this._role.UID = PlayerIdentityManager.Current.userId;
        this._role.Name = PlayerIdentityManager.Current.displayName;

        //初始化数据
        MediatorUtil.SendNotification(NotiDefine.GenerateMySpotDo, 1);
        MediatorUtil.SendNotification(NotiDefine.LoadAllHeroDo);
        //加载场景
        this.SendNotification(NotiDefine.DoLoadScene, SceneDefine.Home);
    }//end func

    private void DoSaveRole()
    {
        //存储
        CloudDataTool.SaveFile(SaveFileDefine.Role, _role);
    }

 


}//end class
