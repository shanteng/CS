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

    public int GetCity(int teamid)
    {
        if (teamid == 0)
            return -1;
        Team team = this.GetTeam(teamid);
        return team.BelongID;
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

 

    public void ComputeBuildingEffect(int city)
    {
        
    }

    public void SetTeamHero(int teamid,int heroid)
    {
        Team team = this.GetTeam(teamid);
        if (team.Status != (int)TeamStatus.Idle)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.TeamNotIdle);
            return;
        }

        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects(team.BelongID);
        bool isOpen = effect.TroopNum >= team.Index;
        if (isOpen == false)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.TeamNotOpen);
            return;
        }
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
            team.BelongID = cityid;
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
