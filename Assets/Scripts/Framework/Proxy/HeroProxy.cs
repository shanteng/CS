using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;


public class HeroProxy : BaseRemoteProxy
{
    private Dictionary<int, Team> _teams = new Dictionary<int, Team>();
    private Dictionary<int, Hero> _datas = new Dictionary<int, Hero>();
    private Dictionary<int, HeroRecruitRefreshData> _refreshData = new Dictionary<int, HeroRecruitRefreshData>();

    public static HeroProxy _instance;
    public HeroProxy() : base(ProxyNameDefine.HERO)
    {
        _instance = this;
    }


    public void DoSaveTeams()
    {
        CloudDataTool.SaveFile(SaveFileDefine.Team, this._teams);
    }

    public Team GetTeam(int id)
    {
        Team team = null;
        if (this._teams.TryGetValue(id, out team))
            return team;
        return null;
    }

    public Dictionary<int, Team> GetTeams()
    {
        return this._teams;
    }

    public void DoSaveHeros()
    {
        CloudDataTool.SaveFile(SaveFileDefine.HeroDatas, this._datas);
    }


    public int JudegeTalentResult(int type, int value, int heroid)
    {
        HeroConfig heroCoinfig = HeroConfig.Instance.GetData(heroid);
        int heroValue = 0;
        for (int i = 0; i < heroCoinfig.Talents.Length; ++i)
        {
            string[] testTalent = heroCoinfig.Talents[i].Split(':');
            int talentType = UtilTools.ParseInt(testTalent[0]);
            int talentValue = UtilTools.ParseInt(testTalent[1]);
            if (talentType == type)
            {
                heroValue = talentValue;
                break;
            }
        }//end for

        if (heroValue > value)
            return 1;

        int curLucky = UtilTools.RangeInt(0, 100);
        if (heroValue == value)
        {
            //根据幸运值判断
            if (heroCoinfig.Lucky >= curLucky)
                return 2;
            return 0;
        }

        int luckLow = Mathf.CeilToInt((float)heroCoinfig.Lucky * (float)heroValue / 100f);
        if (luckLow >= curLucky)
            return 2;
        return 0;
    }

    public int GetHeroCareerRate(int heroid, int curCareer)
    {
        HeroConfig config = HeroConfig.Instance.GetData(heroid);
        if (config == null)
            return 0;
        int rateID = 0;
        for (int i = 0; i < config.CareerRates.Length; ++i)
        {
            int career = i + 1;
            if (career == curCareer)
            {
                rateID = config.CareerRates[i];
                break;
            }
        }//end for
        return rateID;
    }

    public Hero GetHero(int id)
    {
        Hero hero = null;
        if (this._datas.TryGetValue(id, out hero))
            return hero;
        return null;
    }

    public void AddExpToHero(int id, int addExp,bool needSave = true)
    {
        Hero hero = this.GetHero(id);
        if (hero == null) return;

        int realAddExp = 0;
        int totleExp = addExp + hero.Exp;
        hero.Exp = 0;
        int oldLevel = hero.Level;
        int nextLevel = hero.Level + 1;
        HeroLevelConfig configNext = HeroLevelConfig.Instance.GetData(nextLevel);
        while (configNext != null && totleExp > 0)
        {
            bool isLevelUp = totleExp >= configNext.Exp;
            int curLevelAddExp = isLevelUp ? configNext.Exp : totleExp;
            if (isLevelUp)
            {
                nextLevel += 1;
                hero.Level += 1;
                hero.Exp = 0;
                configNext = HeroLevelConfig.Instance.GetData(nextLevel);
                totleExp -= curLevelAddExp;
            }
            else
            {
                hero.Exp = curLevelAddExp;
                totleExp = 0;
            }

            realAddExp += curLevelAddExp;
        }//end while


        if (realAddExp > 0)
        {
            //升级了
            HeroConfig config = HeroConfig.Instance.GetData(id);
            string notice = "";
            if (oldLevel < hero.Level)
            {
                notice = LanguageConfig.GetLanguage(LanMainDefine.HeroAddExpLevelUp, config.Name, realAddExp, hero.Level);
                hero.ComputeAttributes();
            }
            else
            {
                notice = LanguageConfig.GetLanguage(LanMainDefine.HeroAddExp, config.Name, realAddExp);
            }
            RoleProxy._instance.AddLog(LogType.Hero, notice);
        }

        if (needSave)
            this.DoSaveHeros();
    }

    public Dictionary<int, Hero> GetAllHeros()
    {
        return this._datas;
    }

    public void ComputeBuildingEffect(int city)
    {
        bool isChange = false;
        var it = this._datas.Values.GetEnumerator();
        while (it.MoveNext())
        {
            Hero hero = it.Current;
            if (hero.IsMy)
            {
                int inTeamID = TeamProxy._instance.GetHeroTeamID(hero.Id);
                int cityid = TeamProxy._instance.GetTeamCity(inTeamID);
                if (cityid == city)
                {
                    isChange = true;
                    hero.ComputeAttributes();
                }
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

    public void GetHeroRefreshData(int city)
    {
        HeroRecruitRefreshData data = this.GetCityRefreshData(city);
        if (data == null)
        {
            this.GenerateRefresh(city);
        }
        MediatorUtil.SendNotification(NotiDefine.GetHeroRefreshResp, city);
    }

    private void LoadRefreshData()
    {
        string fileName = UtilTools.combine(SaveFileDefine.HeroRecruitRefresh);
        string json = CloudDataTool.LoadFile(fileName);
        if (json.Equals(string.Empty) == false)
        {
            this._refreshData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int,HeroRecruitRefreshData>>(json);
            foreach (HeroRecruitRefreshData data in this._refreshData.Values)
            {
                if (data.ExpireTime < GameIndex.ServerTime)
                    this.GenerateRefresh(data.City);
                else
                    this.AddTimeCallBack(data.City);
            }
        }
    }

    public void OnRefreshTimeReachedNoti(int city)
    {
        this.GenerateRefresh(city);
        MediatorUtil.SendNotification(NotiDefine.HeroTavernRefresh, city);
    }

    public bool IsHeroInCityTarven(int id)
    {
        foreach (HeroRecruitRefreshData refresh in this._refreshData.Values)
        {
            if (refresh.IDs.Contains(id))
                return true;
        }
        return false;
    }

    public HeroRecruitRefreshData GetCityRefreshData(int city)
    {
        HeroRecruitRefreshData cityRefresh;
        if (this._refreshData.TryGetValue(city, out cityRefresh))
            return cityRefresh;
        return null;
    }

    public void JustRefreshHero(int city,bool save = false)
    {
        HeroRecruitRefreshData cityRefresh = this.GetCityRefreshData(city);
        if (cityRefresh == null)
            return;
        BuildingEffectsData data = WorldProxy._instance.GetBuildingEffects(city);
        int limitCount = data.HeroRectuitLimit;
        if (limitCount == 0)
            return;

        List<int> wildHero = new List<int>();
        foreach (Hero hero in this._datas.Values)
        {
            HeroConfig config = HeroConfig.Instance.GetData(hero.Id);
            if (hero.City == (int)HeroBelong.Wild && config.DarkSide == 0)
            {
                //心魔不会刷出来
                bool isInTarven = this.IsHeroInCityTarven(hero.Id);
                if (isInTarven == false)
                    wildHero.Add(hero.Id);
            }
        }

        if (limitCount > wildHero.Count)
            limitCount = wildHero.Count;

        cityRefresh.IDs.Clear();
        cityRefresh.IDs = UtilTools.GetRandomChilds<int>(wildHero, limitCount);
        if (save)
        {
            this.DoSaveHeros();
        }
    }

    public void GenerateRefresh(int city)
    {
        BuildingEffectsData data = WorldProxy._instance.GetBuildingEffects(city);
        int limitCount = data.HeroRectuitLimit;
        if (limitCount == 0)
            return;
        HeroRecruitRefreshData cityRefresh = this.GetCityRefreshData(city);
        if (cityRefresh == null)
        {
            cityRefresh = new HeroRecruitRefreshData();
            cityRefresh.City = city;
            cityRefresh.IDs = new List<int>();
            this._refreshData.Add(city, cityRefresh);
        }

        this.JustRefreshHero(city,false);

        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.TavernRefreshSecs);
        int secs = cfgconst.IntValues[0];
        cityRefresh.ExpireTime = GameIndex.ServerTime + secs;
       
        this.DoSaveRefresh();
        this.AddTimeCallBack(city);
    }

    public bool IsInTarvenHero(int id,int cityid)
    {
        HeroRecruitRefreshData cityRefresh = this.GetCityRefreshData(cityid);
        if (cityRefresh == null)
            return false;
        Hero he = this.GetHero(id);
        return cityRefresh.IDs.Contains(id) && he.City == (int)HeroBelong.Wild;
    }

    public long GetTervenExpire(int city)
    {
        HeroRecruitRefreshData cityRefresh = this.GetCityRefreshData(city);
        if (cityRefresh == null)
            return 0;
        return cityRefresh.ExpireTime;
    }

    private void AddTimeCallBack(int city)
    {
        HeroRecruitRefreshData data = this.GetCityRefreshData(city);
        if (data == null)
            return;
        TimeCallData dataTime = new TimeCallData();
        dataTime._key = UtilTools.GenerateUId();
        dataTime._notifaction = NotiDefine.HeroTavernRefreshReachedNoti;
        dataTime.TimeStep = data.ExpireTime;
        dataTime._param = data.City;
        MediatorUtil.SendNotification(NotiDefine.AddTimestepCallback, dataTime);
    }

    public void DoSaveRefresh()
    {
        CloudDataTool.SaveFile(SaveFileDefine.HeroRecruitRefresh, this._refreshData);
    }

    public bool HasFreeHero()
    {
        foreach (Hero hero in this._datas.Values)
        {
            if (hero.IsMy && hero.DoingState == (int)HeroDoingState.Idle)
            {
                int teamid = TeamProxy._instance.GetHeroTeamID(hero.Id);
                if (teamid == 0)
                    return true;
            }
        }
        return false;
    }

    public void ReloadAllHerosSkills()
    {
        string fileName = UtilTools.combine(SaveFileDefine.HeroDatas);
        string json = CloudDataTool.LoadFile(fileName);
        if (json.Equals(string.Empty) == false)
        {
            this._datas = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, Hero>>(json);
            var it = this._datas.Values.GetEnumerator();
            while (it.MoveNext())
            {
                Hero hero = it.Current;
                hero.ReloadSkill();
            }
            it.Dispose();
            this.DoSaveHeros();
        } 
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
        else
        {
            Dictionary<int, HeroConfig> heroDic = HeroConfig.Instance.getDataArray();
            var it = heroDic.Values.GetEnumerator();
            while (it.MoveNext())
            {
                HeroConfig config = it.Current;
                if (this._datas.ContainsKey(config.ID))
                    continue;
                Hero newHero = new Hero();
                newHero.Create(config);
                this._datas[newHero.Id] = newHero;
            }
            it.Dispose();
        }

        LoadRefreshData();
        this.SendNotification(NotiDefine.LoadAllHeroResp);
    }

    public void RecruitHero(int id)
    {
        Hero hero = this.GetHero(id);
        HeroConfig config = HeroConfig.Instance.GetData(id);
        FavorLevelConfig configLv = HeroProxy._instance.GetFaovrConfig(hero.Favor);
        FavorLevelConfig configNeed = FavorLevelConfig.Instance.GetData(config.FavorLevel);

        int reputationNeed = config.NeedPower;
        int myPower = RoleProxy._instance.Role.Power;

        bool isConditonOk = myPower >= reputationNeed || configLv.ID >= configNeed.ID;
        if (isConditonOk == false)
        {
            PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.RecruitHeroUnSatisfy));
            return;
        }

        bool isCostEnough = RoleProxy._instance.TryDeductCost(config.Cost);
        if (isCostEnough == false)
        {
            return;
        }

        this.ChangeHeroBelong(id,(int)HeroBelong.MainCity);
        this.SendNotification(NotiDefine.RecruitHeroResp,id);
    }

    public void ChangeHeroDoing(int id,int state)
    {
        Hero hero = this.GetHero(id);
        hero.DoingState = state;
        this.DoSaveHeros();
    }

    public void ChangeHeroEnegry(int id, int addValue)
    {
        Hero hero = this.GetHero(id);
        hero.ChangeEnegry(addValue);
    }

    public void DoOwnHero(CostData data)
    {
        int id = UtilTools.ParseInt(data.id);
        Hero hero = this.GetHero(id);
        if (hero.IsMy == false)
            this.ChangeHeroBelong(id, 0);
    }

    public void ChangeHeroBelong(int id ,int belongCity)
    {
        Hero hero = this.GetHero(id);
        if (belongCity == hero.City)
            return;

        bool oldMy = hero.IsMy;
        hero.City = belongCity;
        hero.ComputeAttributes();
        this.DoSaveHeros();

        if (oldMy != hero.IsMy && hero.IsMy)
        {
            HeroConfig config = HeroConfig.Instance.GetData(id);
            PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.RecruitHeroSuccess,config.Name));
            RoleProxy._instance.AddLog(LogType.RecruitHeroSuccess, LanguageConfig.GetLanguage(LanMainDefine.RecruitHeroSuccess, config.Name));
        }
    }

    public void TalkToHero(int id,bool isCorrect)
    {
        Hero hero = this.GetHero(id);
        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.HeroTalkGap);
        hero.TalkExpire = GameIndex.ServerTime + cfgconst.IntValues[0];
        if (isCorrect)
        {
            FavorLevelConfig configLv = this.GetFaovrConfig(hero.Favor);
            this.ChangeHeroFavor(id, configLv.TalkAdd);
        }
        else
        {
            this.DoSaveHeros();
        }
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
            if (configLv.ID > configLvOld.ID)
                this.SendNotification(NotiDefine.FavorLevelUpNoti, id);
            PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.HeroFavorLevelChanged, config.Name, configLv.Name));

            RoleProxy._instance.AddLog(LogType.HeroFavorLevel, LanguageConfig.GetLanguage(LanMainDefine.HeroFavorLevelChanged, config.Name, configLv.Name));
        }
        else if (hero.Favor > oldFavor)
        {
            PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.HeroFavorUp, config.Name), CommonUINameDefine.UP_arrow);

            RoleProxy._instance.AddLog(LogType.HeroFavorChange, LanguageConfig.GetLanguage(LanMainDefine.HeroFavorUp, config.Name));
        }
        else if (hero.Favor < oldFavor)
        {
            PopupFactory.Instance.ShowNotice(LanMainDefine.HeroFavorDown,config.Name);
            RoleProxy._instance.AddLog(LogType.HeroFavorChange, LanguageConfig.GetLanguage(LanMainDefine.HeroFavorDown, config.Name));
        }

        this.DoSaveHeros();
    }

}//end class
