using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;

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
    public Dictionary<string, int> IncomeDic;
}


//时间戳回调管理
public class RoleProxy : BaseRemoteProxy
{
    private List<string> _LimitValueKeys = new List<string>();
    private RoleInfo _role;
    public static RoleProxy _instance;
    public RoleProxy() : base(ProxyNameDefine.ROLE)
    {
        _instance = this;
    }

    public RoleInfo Role => this._role;
    public int ResValueLimit => this._role.ResValueLimit;

    public void ComputeBuildingEffect()
    {
        //重新计算收益和可领取的收益
        BuildingEffectsData datas = WorldProxy._instance.GetBuildingEffects();
        this._role.IncomeDic = datas.IncomeDic;
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

    private bool IsAddOverLimit(string key, int addValue)
    {
        int limit = RoleProxy._instance.ResValueLimit;
        CostData data = this.GetNumberValueData(key);
        int afterValue = data.count + addValue;
        bool isOverLimit =  this._LimitValueKeys.Contains(key) && afterValue > limit;
        if (isOverLimit)
        {
            string attrName = ItemKey.GetName(key);
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.ValueOutOfRange, attrName, addValue);
            this.SendNotification(NotiDefine.ErrorCode, ErrorCode.ValueOutOfRange);
        }
        return isOverLimit;
    }

    public void AcceptHourAward(string key)
    {
        //重新计算收益和可领取的收益
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
                    bool isOverLimit = this.IsAddOverLimit(key,addValue);
                    if (isOverLimit)
                        return;
                    
                    CostData data = new CostData();
                    data.id = AddUpAwards[i].id;
                    data.count = addValue;
                    awards.Add(data);

                    AddUpAwards[i].add_up_value = 0;
                    AddUpAwards[i].generate_time = GameIndex.ServerTime;
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
        if (this._role.IncomeDic != null &&  this._role.IncomeDic.TryGetValue(key, out value))
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
            this._role.UID = UIRoot.Intance._sdkCore.userId;
            this._role.Name = UIRoot.Intance._sdkCore.displayName;
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

 
        this._role.UID = UIRoot.Intance._sdkCore.userId;
        this._role.Name = UIRoot.Intance._sdkCore.displayName;
        this.SendNotification(NotiDefine.DoLoadScene, SceneDefine.Home);
    }//end func

    private void DoSaveRole()
    {
        //存储
        CloudDataTool.SaveFile(SaveFileDefine.Role, _role);
    }

 


}//end class
