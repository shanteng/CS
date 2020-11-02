using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;


public class HeroProxy : BaseRemoteProxy
{
    private Dictionary<int, Hero> _datas = new Dictionary<int, Hero>();
    private HeroRecruitRefreshData _refreshData = new HeroRecruitRefreshData();

    public static HeroProxy _instance;
    public HeroProxy() : base(ProxyNameDefine.HERO)
    {
        _instance = this;
    }

    public void DoSaveHeros()
    {
        List<Hero> list = new List<Hero>();
        foreach (Hero data in this._datas.Values)
        {
            list.Add(data);
        }
        CloudDataTool.SaveFile(SaveFileDefine.HeroDatas, this._datas);
    }

    public Hero GetHero(int id)
    {
        Hero hero = null;
        if (this._datas.TryGetValue(id, out hero))
            return hero;
        return null;
    }

    public Dictionary<int, Hero> GeAllHeros()
    {
        return this._datas;
    }

    public void ComputeBuildingEffect()
    {
        bool isChange = false;
        var it = this._datas.Values.GetEnumerator();
        while (it.MoveNext())
        {
            Hero hero = it.Current;
            if (hero.Belong == (int)HeroBelong.My)
            {
                isChange = true;
                hero.ComputeAttributes();
            }
              
        }
        it.Dispose();
        if (isChange)
            this.SendNotification(NotiDefine.HerosUpdated);
    }

    public  FavorLevelConfig GetFaovrConfig(int favor)
    {
        Dictionary<int, FavorLevelConfig> dic = FavorLevelConfig.Instance.getDataArray();
        foreach (FavorLevelConfig config in dic.Values)
        {
            if (favor >= config.FavorRange[0] && favor <= config.FavorRange[1])
                return config;
        }
        return dic[0];
    }

    public void LoadRefreshData(bool isGenerate)
    {
        string fileName = UtilTools.combine(SaveFileDefine.HeroRecruitRefresh);
        string json = CloudDataTool.LoadFile(fileName);
        if (json.Equals(string.Empty) == false)
        {
            this._refreshData = Newtonsoft.Json.JsonConvert.DeserializeObject<HeroRecruitRefreshData>(json);
            this.AddTimeCallBack();
        }
        else if(isGenerate)
        {
            this.GenerateRefresh();
        }

        MediatorUtil.SendNotification(NotiDefine.GetHeroRefreshResp);
    }

    public void OnRefreshTimeReachedNoti()
    {
        this.GenerateRefresh();
        MediatorUtil.SendNotification(NotiDefine.HeroTavernRefresh);
    }


    private void GenerateRefresh()
    {
        this._refreshData = new HeroRecruitRefreshData();
        BuildingEffectsData data = WorldProxy._instance.GetBuildingEffects();
        int limitCount = data.HeroRectuitLimit;

        if (limitCount == 0)
            return;

        List<int> wildHero = new List<int>();
        foreach (Hero hero in this._datas.Values)
        {
            if (hero.Belong == (int)HeroBelong.Wild)
                wildHero.Add(hero.Id);
        }

        if (limitCount > wildHero.Count)
            limitCount = wildHero.Count;

        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.TavernRefreshSecs);
        int secs = cfgconst.IntValues[0];

        this._refreshData.IDs = UtilTools.GetRandomChilds<int>(wildHero, limitCount);
        this._refreshData.ExpireTime = GameIndex.ServerTime + secs;
        this.DoSaveRefresh();
        this.AddTimeCallBack();
    }

    public bool IsInMyTarvenHero(int id)
    {
        Hero he = this.GetHero(id);
        return this._refreshData.IDs.Contains(id) && he.Belong == (int)HeroBelong.Wild;
    }

    public long GetTervenExpire()
    {
        return this._refreshData.ExpireTime;
    }

    private void AddTimeCallBack()
    {
        TimeCallData dataTime = new TimeCallData();
        dataTime._key = UtilTools.GenerateUId();
        dataTime._notifaction = NotiDefine.HeroTavernRefreshReachedNoti;
        dataTime.TimeStep = this._refreshData.ExpireTime;
        dataTime._param = "";
        MediatorUtil.SendNotification(NotiDefine.AddTimestepCallback, dataTime);
    }

  

    public void DoSaveRefresh()
    {
        CloudDataTool.SaveFile(SaveFileDefine.HeroRecruitRefresh, this._refreshData);
    }

    public void LoadAllHeros()
    {
        this._datas.Clear();
        string fileName = UtilTools.combine(SaveFileDefine.HeroDatas);
        string json = CloudDataTool.LoadFile(fileName);
        if (json.Equals(string.Empty) == false)
        {
           this._datas = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, Hero>>(json);
        }

        bool hasNewHero = false;
        Dictionary<int, HeroConfig> heroDic = HeroConfig.Instance.getDataArray();
        var it = heroDic.Values.GetEnumerator();
        while (it.MoveNext())
        {
            HeroConfig config = it.Current;
            if (this._datas.ContainsKey(config.ID))
                continue;
            hasNewHero = true;
            Hero newHero = new Hero();
            newHero.Create(config);
            this._datas[newHero.Id] = newHero;
        }
        it.Dispose();

        if (hasNewHero)
            this.DoSaveHeros();

        LoadRefreshData(false);
        this.SendNotification(NotiDefine.LoadAllHeroResp);
    }

    public void TalkToHero(int id)
    {
        Hero hero = this.GetHero(id);
        if (hero.TalkExpire > 0 && hero.TalkExpire > GameIndex.ServerTime)
        {
            string cdStr = UtilTools.GetCdStringExpire(hero.TalkExpire);
            PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.AfterTimeTalk, cdStr));
            return;
        }

        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.HeroTalkGap);
        hero.TalkExpire = GameIndex.ServerTime + cfgconst.IntValues[0];
        FavorLevelConfig configLv = this.GetFaovrConfig(hero.Favor);
        this.ChangeHeroFavor(id, configLv.TalkAdd);
        this.SendNotification(NotiDefine.TalkToHeroResp);
    }

    public void ChangeHeroFavor(int id, int addOn)
    {
        Hero hero = this.GetHero(id);
        int oldFavor = hero.Favor;
        hero.Favor += addOn;

        HeroConfig config = HeroConfig.Instance.GetData(id);
        FavorLevelConfig configLvOld = this.GetFaovrConfig(oldFavor);
        FavorLevelConfig configLv = this.GetFaovrConfig(hero.Favor);

        if (configLvOld != configLv)
        {
            PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.HeroFavorLevelChanged, config.Name, configLv.Name));
        }
        else if (hero.Favor > oldFavor)
        {
            PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.HeroFavorUp, config.Name, configLv.Name), CommonUINameDefine.UP_arrow);
        }
        else if (hero.Favor < oldFavor)
        {
            PopupFactory.Instance.ShowNotice(LanMainDefine.HeroFavorDown);
        }

        this.DoSaveHeros();
    }

    public void ChangeHeroBelong(Dictionary<string, object> vo)
    {
        int id = (int)vo["id"];
        int belong = (int)vo["belong"];

        Hero hero = this.GetHero(id);
        if (hero == null) 
            return;
        int oldBelong = hero.Belong;
        if (oldBelong == belong)
            return;
        hero.Belong = belong;
        hero.Blood = 0;//变更所有权后兵力归零
        if (belong == (int)HeroBelong.My)
            this.SendNotification(NotiDefine.GetHeroNoti, id);
        else 
            this.SendNotification(NotiDefine.LosetHeroNoti, id);

        this.DoSaveHeros();
    }


}//end class
