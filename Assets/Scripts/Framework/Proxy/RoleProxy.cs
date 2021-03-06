﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using DG.Tweening.Plugins;
using Newtonsoft.Json.Utilities;
using SMVC.Patterns;
using UnityEngine;
using UnityEngine.Events;
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
    public int Head;
    public int Frame;
}


//时间戳回调管理
public class RoleProxy : BaseRemoteProxy
     , IConfirmListener
{
    private Dictionary<string, int> IncomeDic = new Dictionary<string, int>();
    private Dictionary<string, int> HourAwardLimitDic = new Dictionary<string, int>();
    private Queue<LogData> _LogDatas = new Queue<LogData>();
    private RoleInfo _role;
    public static RoleProxy _instance;
    public RoleProxy() : base(ProxyNameDefine.ROLE)
    {
        _instance = this;
    }

    public RoleInfo Role => this._role;

    public void AddLog(LogType type,string Content, VInt2 Position = null,string bdKey = "")
    {
        LogData data = new LogData();
        data.New = true;
        data.ID = UtilTools.GenerateUId();
        data.Type = type;
        data.Content = Content;
        data.BdKey = bdKey;
        if (Position != null)
            data.Position = Position;
        data.Time = GameIndex.ServerTime;
        this._LogDatas.Enqueue(data);

        if (this._LogDatas.Count > 300)
        {
            this._LogDatas.Dequeue();
        }

        this.DoSaveLog();

        this.SendNotification(NotiDefine.NewLogNoti,data);
    }

    public void SetLogOld()
    {
        foreach (LogData log in this._LogDatas)
        {
            log.New = false;
        }
        this.DoSaveLog();
    }

    public Queue<LogData> GetLogs()
    {
        return this._LogDatas;
    }

    public void ComputeIncome()
    {
        this.IncomeDic.Clear();
        this.HourAwardLimitDic.Clear();
        //建筑加成
        Dictionary<int, BuildingEffectsData> Effects = WorldProxy._instance.AllEffects;
        foreach (BuildingEffectsData datas in Effects.Values)
        {
            foreach (string key in datas.IncomeDic.Keys)
            {
                int oldvalue = 0;
                if (this.IncomeDic.TryGetValue(key, out oldvalue) == false)
                    oldvalue = 0;
                this.IncomeDic[key] = datas.IncomeDic[key].Count + oldvalue;

                oldvalue = 0;
                if (this.HourAwardLimitDic.TryGetValue(key, out oldvalue) == false)
                    oldvalue = 0;
                this.HourAwardLimitDic[key] = datas.IncomeDic[key].LimitVolume + oldvalue;
            }
        }
    }

    public void ComputeBuildingEffect()
    {
        this.ComputeIncome();
        this.ComputePower(false);
        this.UpdateHourAward();
        this.DoSaveRole();
        this.SendNotification(NotiDefine.ResLimitHasUpdated);
    }

    public void ComputePower(bool save)
    {
        RoleLevelConfig config = RoleLevelConfig.Instance.GetData(this._role.Level);
        int armyPower = ArmyProxy._instance.GetPower();
        this._role.Power = config.Power  + armyPower;

        Dictionary<int, BuildingEffectsData> Effects = WorldProxy._instance.AllEffects;
        foreach (BuildingEffectsData datas in Effects.Values)
        {
            this._role.Power += datas.PowerAdd;
        }
           
        if (save)
        {
            this.DoSaveRole();
            this.SendNotification(NotiDefine.PowerChanged);
        }
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
                    int addVolume = this.GetHourAwardLimitVolume(key);//不能超过仓库上限
                    if (addValue > addVolume)
                        addValue = addVolume;

                    CostData data = new CostData();
                    data.id = AddUpAwards[i].id;
                    data.count = addValue;
                    awards.Add(data);

                    //计算剩下的时间
                    AddUpAwards[i].generate_time = GameIndex.ServerTime;
                    AddUpAwards[i].add_up_value = 0;

                    this.DoSaveRole();
                }
                break;
            }//end if
        }//end for

        if (awards.Count > 0)
        {
            string name = ItemInfoConfig.Instance.GetData(awards[0].id).Name;
            string Notice = LanguageConfig.GetLanguage(LanMainDefine.GetHourTax, awards[0].count, name);
            RoleProxy._instance.AddLog(LogType.HourTax, Notice);

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
            data.InitJustItem(costs[i]);
            int myValue = this.GetNumberValue(data.id);
            int valueCanDO = myValue / data.count;
            if (valueCanDO < canDoMin)
                canDoMin = valueCanDO;
        }//end for

        if (canDoMin == int.MaxValue)
            return 0;
        return canDoMin;
    }

    public bool IsDedutStisfy(string[] costs, float mutil = 1)
    {
        List<CostData> awards = new List<CostData>();
        int count = costs.Length;
        for (int i = 0; i < count; ++i)
        {
            CostData data = new CostData();
            data.InitJustItem(costs[i], mutil);
            int myValue = this.GetNumberValue(data.id);
            if (myValue < data.count)
            {
                return false;
            }
            data.count = -data.count;
            awards.Add(data);
        }
        return true;
    }
     
    public bool TryDeductCost(string[] costs,float mutil = 1f)
    {
        List<string> attrNames = new List<string>();
        List<CostData> awards = new List<CostData>();
        int count = costs.Length;
        for (int i = 0; i < count; ++i)
        {
            CostData data = new CostData();
            data.InitJustItem(costs[i], mutil);
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

    public void GMAddValue(string key)
    {
        List<CostData> awards = new List<CostData>();
        CostData data = new CostData();
        data.id = key;
        data.count = 5000;
        awards.Add(data);
        this.ChangeRoleNumberValue(awards);
    }

    public void TryAddNumValue(string[] costs, float mutil = 1f)
    {
        List<CostData> awards = new List<CostData>();
        int count = costs.Length;
        for (int i = 0; i < count; ++i)
        {
            CostData data = new CostData();
            data.InitJustItem(costs[i], mutil);
            awards.Add(data);
        }

        if (awards.Count > 0)
        {
            this.ChangeRoleNumberValue(awards);
        }
    }

    public void ChangeRoleNumberValueBy(CostData addData,bool NeedSave = true)
    {
        Dictionary<string, int> ShowAdds = new Dictionary<string, int>();
        string key = addData.id;
        CostData data = this.GetNumberValueData(key);
        if (data == null)
        {
            data = new CostData();
            data.id = key;
            data.count = 0;
            this._role.ItemList.Add(data);
            ShowAdds[key] = addData.count;
        }
        data.count += addData.count;

        if (NeedSave)
        {
            this.SendNotification(NotiDefine.NumberValueHasUpdated, ShowAdds);
            this.DoSaveRole();
        }
           
    }

    public void ChangeRoleNumberValue(List<CostData> addDatas)
    {
        Dictionary<string, int> ShowAdds = new Dictionary<string, int>();
        int count = addDatas.Count;
        for (int i = 0; i < count; ++i)
        {
            string key = addDatas[i].id;
            this.ChangeRoleNumberValueBy(addDatas[i],false);
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

    private object _callBackParam;
    private List<CostData> _costsSpeedUp = new List<CostData>();
    private ConfirmData _confirmSpeed;
    public bool TrySpeedUp(int secs,UnityAction<object> callBack,object callBackParam)
    {
        if (this._confirmSpeed == null)
        {
            _confirmSpeed = new ConfirmData();
            _confirmSpeed.contentText = LanguageConfig.GetLanguage(LanMainDefine.SpeedUpNotice);
            _confirmSpeed.resTitleText = LanguageConfig.GetLanguage(LanMainDefine.NeedCostValue);
            _confirmSpeed.Reses = new List<CostData>();
            _confirmSpeed.listener = this;
            _confirmSpeed.userKey = "SpeedUp";
        }

        _confirmSpeed.Reses.Clear();
        _confirmSpeed.param = callBack;

        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.SpeedUpOneSecsNeed);
        int count = cfgconst.StringValues.Length;
        _costsSpeedUp.Clear();
        for (int i = 0; i < count; ++i)
        {
            CostData data = new CostData();
            data.InitJustItem(cfgconst.StringValues[i], secs);
            int myValue = this.GetNumberValue(data.id);
            if (myValue < data.count)
            {
                ItemInfoConfig cfg = ItemInfoConfig.Instance.GetData(data.id);
                PopupFactory.Instance.ShowErrorNotice(ErrorCode.SpeedUpCostNotEnought, cfg.Name);
                return false;
            }

            _confirmSpeed.Reses.Add(data);

            CostData costData = new CostData();
            costData.count = -data.count;
            costData.id = data.id;
            _costsSpeedUp.Add(costData);
        }
        this._callBackParam = callBackParam;

       
        PopupFactory.Instance.ShowConfirmBy(_confirmSpeed);
        return true;
    }

    public void OnConfirm(ConfirmData data)
    {
        if (data.userKey.Equals("SpeedUp"))
        {
            //扣除费用
            this.ChangeRoleNumberValue(this._costsSpeedUp);
            UnityAction<object> callBack = (UnityAction<object>)data.param;
            callBack.Invoke(this._callBackParam);//回调
        }
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

    public int GetHourAwardLimitVolume(string key)
    {
        int value = 0;
        if (this.HourAwardLimitDic.TryGetValue(key, out value))
            return value;
        return 0;
    }

    public HourAwardData GetCanAcceptIncomeData(string key)
    {
       foreach(HourAwardData data in this.Role.AddUpAwards)
        {
            if (data.generate_time > 0 && data.id.Equals(key))
            {
                return data;
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

    public void DoCreateRole(Dictionary<string, object> vo)
    {
        this._role = new RoleInfo();
        this._role.UID = PlayerIdentityManager.Current.userId;
        this._role.Name = (string)vo["name"];
        this._role.Head = (int)vo["head"];
        this.Role.Frame = 0;
        this._role.Level = 1;
        this._role.Exp = 0;
        ConstConfig config = ConstConfig.Instance.GetData(ConstDefine.InitRes);
        int count = config.StringValues.Length;
        this._role.ItemList = new List<CostData>();

        this._role.AddUpAwards = new List<HourAwardData>();
        for (int i = 0; i < count; ++i)
        {
            CostData data = new CostData();
            data.InitJustItem(config.StringValues[i]);
            this._role.ItemList.Add(data);

            HourAwardData hourAward = new HourAwardData();
            hourAward.id = data.id;
            hourAward.add_up_value = 0;
            hourAward.base_secs_value = 0;
            hourAward.generate_time = 0;
            this._role.AddUpAwards.Add(hourAward);
        }
        this.DoSaveRole();
        this.SendNotification(NotiDefine.CreateRoleResp);
        this.Login();
    }

    private void Login()
    {
        this._role.UID = PlayerIdentityManager.Current.userId;

        //初始化数据
        WorldProxy._instance.GenerateAllBaseSpot(1);
        ArmyProxy._instance.LoadAllArmys();
        HeroProxy._instance.LoadAllHeros();
        TeamProxy._instance.LoadAllTeam();
        TeamProxy._instance.LoadGroup();

        WorldProxy._instance.LoadPatrol();
        WorldProxy._instance.LoadQuestCity();
        //加载场景
        this.SendNotification(NotiDefine.DoLoadScene, SceneDefine.Home);
    }

    public void DoEnterGame()
    {
        string json = CloudDataTool.LoadFile(SaveFileDefine.Role);
        if (json.Equals(string.Empty))
        {
            MediatorUtil.ShowMediator(MediatorDefine.CREATE);
        }
        else
        {
            this._role = Newtonsoft.Json.JsonConvert.DeserializeObject<RoleInfo>(json);
            this.Login();
        }

        json = CloudDataTool.LoadFile(SaveFileDefine.Log);
        if (json.Equals(string.Empty))
        {
            this._LogDatas.Clear();
        }
        else
        {
            this._LogDatas = Newtonsoft.Json.JsonConvert.DeserializeObject<Queue<LogData>>(json);
        }
    }//end func

    private void DoSaveRole()
    {
        //存储
        CloudDataTool.SaveFile(SaveFileDefine.Role, _role);
    }


    private void DoSaveLog()
    {
        //存储
        CloudDataTool.SaveFile(SaveFileDefine.Log, this._LogDatas);
    }


}//end class
