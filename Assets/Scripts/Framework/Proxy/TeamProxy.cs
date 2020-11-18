using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;


public class TeamProxy : BaseRemoteProxy
{
    private Dictionary<int, Team> _teams = new Dictionary<int, Team>();
 
    public static TeamProxy _instance;
    public TeamProxy() : base(ProxyNameDefine.TEAM)
    {
        _instance = this;
    }


    public void DoSaveTeams()
    {
        CloudDataTool.SaveFile(SaveFileDefine.Team, this._teams);
    }

    public int GetTeamCity(int teamid)
    {
        if (teamid <= 0)
            return -1;
        Team team = this.GetTeam(teamid);
        return team.CityID;
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

    public List<Team> GetCityTeams(int city)
    {
        List<Team> list = new List<Team>();
        foreach (Team t in this._teams.Values)
        {
            if (t.CityID == city)
                list.Add(t);
        }
        return list;
    }

 

    public void ComputeBuildingEffect(int city)
    {
        
    }

    public bool IsTeamOpen(int teamid, out int openLevel)
    {
        Team team = this.GetTeam(teamid);
        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects(team.CityID);
        bool isOpen = effect.TroopNum >= team.Index;

        openLevel = 0;
        if (isOpen == false)
        {
            BuildingConfig config = BuildingConfig.Instance.GetData(BuildingData.MainCityID);
            for (int i = 0; i < config.MaxLevel; ++i)
            {
                BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(BuildingData.MainCityID, i+1);
                int openCount = UtilTools.ParseInt( configLv.AddValues[1]);
                if (openCount >= team.Index)
                {
                    openLevel = configLv.Level;
                    break;
                }
            }
        }
        return isOpen;
    }

    public void SetTeamHero(Dictionary<string, object> vo)
    {
        int teamid = (int)vo["teamid"];
        int heroid = (int)vo["heroid"];
        int army = (int)vo["army"];
        int count = (int)vo["count"];

        Team team = this.GetTeam(teamid);
        if (team.Status != (int)TeamStatus.Idle)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.TeamNotIdle);
            return;
        }

        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects(team.CityID);
        bool isOpen = effect.TroopNum >= team.Index;
        if (isOpen == false)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.TeamNotOpen);
            return;
        }

        Hero heroTeam = HeroProxy._instance.GetHero(team.HeroID);
        Hero newHero = HeroProxy._instance.GetHero(heroid);

        if (heroTeam != null && team.HeroID != heroid)
        {
            //卸下兵种
            ArmyProxy._instance.ChangeArmyCount(team.CityID, heroTeam.ArmyTypeID, heroTeam.Blood);
            heroTeam.ArmyTypeID = 0;
            heroTeam.Blood = 0;
            heroTeam.TeamId = 0;
        }
        else if (heroTeam == null && newHero != null)
        {
            ArmyProxy._instance.ChangeArmyCount(team.CityID, army, -count);
            newHero.ArmyTypeID = army;
            newHero.Blood = count;
            newHero.TeamId = teamid;
        }
        else if (heroTeam != null && team.HeroID == heroid)
        {
            if (heroTeam.ArmyTypeID == army)
            {
                ArmyProxy._instance.ChangeArmyCount(team.CityID, heroTeam.ArmyTypeID, heroTeam.Blood - count);
            }
            else
            {
                ArmyProxy._instance.ChangeArmyCount(team.CityID, heroTeam.ArmyTypeID, heroTeam.Blood);//旧的加回去
                ArmyProxy._instance.ChangeArmyCount(team.CityID, army, -count);//新的减少掉
            }
            heroTeam.ArmyTypeID = army;
            heroTeam.Blood = count;
        }

        team.HeroID = heroid;
        HeroProxy._instance.DoSaveHeros();
        this.DoSaveTeams();
        this.SendNotification(NotiDefine.SetTeamHeroResp);
        PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.TeamSetSuccess));
    }

   

    public void InitCityTeam(int cityid)
    {
        BuildingConfig config = BuildingConfig.Instance.GetData(BuildingData.MainCityID);
        BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(BuildingData.MainCityID, config.MaxLevel);
        int max = UtilTools.ParseInt(configLv.AddValues[1]);
        for (int i = 0; i < max; ++i)
        {
            Team team = new Team();
            team.Index = i + 1;
            team.Id = cityid * 100 + team.Index;
            team.HeroID = 0;
            team.Status = (int)TeamStatus.Idle;
            team.CityID = cityid;
            team.FromID = cityid;
            team.TargetID = 0;
            team.StartTime = 0;
            team.ExpireTime = 0;
            this._teams[team.Id] = team;
        }
        this.DoSaveTeams();
    }

    public void LoadAllTeam()
    {
        this._teams.Clear();
        string fileName = UtilTools.combine(SaveFileDefine.Team);
        string json = CloudDataTool.LoadFile(fileName);
        if (json.Equals(string.Empty) == false)
        {
            this._teams = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, Team>>(json);
        }
        else
        {
            //构造主城的Team
            this.InitCityTeam(0);
        }
        this.SendNotification(NotiDefine.LoadAllTeamResp);
    }
    

}//end class
