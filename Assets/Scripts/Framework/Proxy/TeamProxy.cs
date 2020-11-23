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
    private Dictionary<string, Group> _GroupDic = new Dictionary<string, Group>();

    public static TeamProxy _instance;
    public TeamProxy() : base(ProxyNameDefine.TEAM)
    {
        _instance = this;
    }


    public void DoSaveTeams()
    {
        this.SendNotification(NotiDefine.TeamStateChangeNoti);
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

    public void GetFightAwardTotleExp(int npcCity, out int count, out int addExp)
    {
        count = 0;
        addExp = 0;
        float totleBlood = 0;
        CityConfig config = CityConfig.Instance.GetData(npcCity);
        int[] npcs = config.NpcTeams;
        for (int i = 0; i < npcs.Length; ++i)
        {
            NpcTeamConfig configNpc = NpcTeamConfig.Instance.GetData(npcs[i]);
            count += configNpc.Count;
            ArmyConfig configArmy = ArmyConfig.Instance.GetData(configNpc.Army);
            totleBlood += count * configArmy.Blood;
        }

        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.CancelArmyReturnRate);
        float rate = (float)cfgconst.IntValues[0] / 100f;
        addExp = Mathf.RoundToInt(rate * totleBlood);
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
                BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(BuildingData.MainCityID, i + 1);
                int openCount = UtilTools.ParseInt(configLv.AddValues[1]);
                if (openCount >= team.Index)
                {
                    openLevel = configLv.Level;
                    break;
                }
            }
        }
        return isOpen;
    }

    public Group GetGroup(string groupid)
    {
        Group data;
        if (this._GroupDic.TryGetValue(groupid, out data))
            return data;
        return null;
    }

    public List<string> GetAttackCityGroups(int city)
    {
        List<string> list = new List<string>();
        foreach (Group gp in this._GroupDic.Values)
        {
            if (gp.TargetCityID == city && gp.Status != (int)GroupStatus.Back)
                list.Add(gp.Id);
        }
        return list;
    }

    public void AttackCityDo(string groupid)
    {
        //通知battleproxy
        Group data;
        if (this._GroupDic.TryGetValue(groupid, out data) == false)
            return;
        CityConfig configCIty = CityConfig.Instance.GetData(data.TargetCityID);

        data.Status = (int)GroupStatus.Fight;//临时状态不存储
        BattleData battleData = new BattleData();
        battleData.Id = configCIty.BattleSceneID;
        battleData.Type = BattleType.AttackCity;
        battleData.Round = 0;
        battleData.Status = BattleStatus.PreStart;
        battleData.Players = new Dictionary<int, BattlePlayer>();
        battleData.Param = groupid;
        foreach (int teamid in data.Teams)
        {
            BattlePlayer player = new BattlePlayer();
            Team team = this.GetTeam(teamid);
            player.InitMy(team,data.GetMorale(),BattlePlace.Attack);
            battleData.Players.Add(teamid, player);
        }

        //构造敌方
        int index = 1;
        foreach (int npcTeam in configCIty.NpcTeams)
        {
            BattlePlayer player = new BattlePlayer();
            player.InitNpc(npcTeam, index, configCIty.BattleSceneID, BattlePlace.Defense);
            battleData.Players.Add(-index, player);
            ++index;
        }
        BattleProxy._instance.EnterBattle(battleData);
    }

    private void GroupBackCity(string groupid)
    {
        Group gp;
        if (this._GroupDic.TryGetValue(groupid, out gp) == false)
            return;
        gp.Status = (int)GroupStatus.Back;
        gp.StartTime = GameIndex.ServerTime;
        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.MoveBackDeltaSces);
        int SecsDelta = cfgconst.IntValues[0];
        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects(gp.CityID);
        float deltaFinal = (1f - effect.TeamMoveReduceRate) * (float)SecsDelta;

        VInt2 StartPos = WorldProxy._instance.GetCityCordinate(gp.TargetCityID);
        VInt2 Goto = WorldProxy._instance.GetCityCordinate(gp.CityID);
        gp.ExpireTime = WorldProxy._instance.GetMoveExpireTime(StartPos.x, StartPos.y, Goto.x, Goto.y, deltaFinal);//栈道来提升速度百分比
        gp.RealTargetPostion = new VInt2();
        gp.RealTargetPostion.x = Goto.x;
        gp.RealTargetPostion.y = Goto.y;

        PathProxy._instance.RemovePath(groupid);
        this.AttackOverBackCityAction(gp);

        this.DoSaveGroup();
    }

    public void AttackCityEnd(string groupid, bool isSuccess)
    {
        

        Group gp;
        if (this._GroupDic.TryGetValue(groupid, out gp) == false)
            return;

        //所有当前等待的Group都返回城市
        List<string> waitAndFightList = this.GetAttackCityGroups(gp.TargetCityID);
        foreach (string gpid in waitAndFightList)
        {
            Group curGp = this.GetGroup(gpid);
            if (curGp.Status == (int)GroupStatus.WaitFight ||
                curGp.Status == (int)GroupStatus.Fight)
            {
                this.GroupBackCity(gpid);
            }
        }

        VInt2 StartPos = WorldProxy._instance.GetCityCordinate(gp.TargetCityID);
        CityConfig config = CityConfig.Instance.GetData(gp.TargetCityID);
        string fName = WorldProxy._instance.GetCityName(gp.CityID);
        string tName = WorldProxy._instance.GetCityName(gp.TargetCityID);

        VInt2 gamePos = UtilTools.WorldToGameCordinate(StartPos.x, StartPos.y);
        string notice = "";
        if (isSuccess)
        {
            List<string> list = new List<string>();
            string GetName = "";
            foreach (string award in config.AttackDrops)
            {
                CostData cost = new CostData();
                cost.InitFull(award);
                if (cost.type.Equals(CostData.TYPE_ITEM))
                {
                    RoleProxy._instance.ChangeRoleNumberValueBy(cost);
                    string name = ItemInfoConfig.Instance.GetData(cost.id).Name;
                    GetName = LanguageConfig.GetLanguage(LanMainDefine.ItemCount, name, cost.count);
                }
                else if (cost.type.Equals(CostData.TYPE_HERO))
                {
                    int heroid = UtilTools.ParseInt(cost.id);
                    HeroProxy._instance.ChangeHeroBelong(heroid, (int)HeroBelong.MainCity);
                    GetName = HeroConfig.Instance.GetData(heroid).Name;
                }
                list.Add(GetName);
            }
            string awardStr = string.Join(",", list);
            notice = LanguageConfig.GetLanguage(LanMainDefine.AttackCitySuccess, fName, tName, awardStr);
        }
        else
        {
            notice = LanguageConfig.GetLanguage(LanMainDefine.AttackCityFail, fName, tName);
        }
            
        PopupFactory.Instance.ShowNotice(notice);
        RoleProxy._instance.AddLog(LogType.OwnCityResp, notice, StartPos);
        this.SendNotification(NotiDefine.MoveToAttackCityResp);
    }

    public void MoveToAttackCityDo(Dictionary<string, object> vo)
    {
        int cityid = (int)vo["cityid"];
        int from = (int)vo["from"];
        List<int> teams = (List<int>)vo["teams"];
        Group gp = new Group();
        gp.Id = UtilTools.GenerateUId();
        gp.CityID = from;
        gp.TargetCityID = cityid;
        gp.StartTime = GameIndex.ServerTime;

        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.AttackDeltaSces);
        int SecsDelta = cfgconst.IntValues[0];
        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects(from);
        float deltaFinal = (1f - effect.TeamMoveReduceRate) * (float)SecsDelta;
        VInt2 StartPos = WorldProxy._instance.GetCityCordinate(from);
        VInt2 Goto = WorldProxy._instance.GetCityCordinate(cityid);
        gp.ExpireTime = WorldProxy._instance.GetMoveExpireTime(StartPos.x, StartPos.y, Goto.x, Goto.y, deltaFinal);//栈道来提升速度百分比

        CityConfig config = CityConfig.Instance.GetData(cityid);
        int halfRange = config.Range[0] / 2;
        if (Goto.x < StartPos.x)
        {
            Goto.x = Goto.x + halfRange;
        }
        else if (Goto.x > StartPos.x)
        {
            Goto.x = Goto.x - halfRange;
        }

        if (Goto.y < StartPos.y)
        {
            Goto.y = Goto.y + halfRange;
        }
        else if (Goto.y > StartPos.y)
        {
            Goto.y = Goto.y - halfRange;
        }

        gp.RealTargetPostion = new VInt2();
        gp.RealTargetPostion.x = Goto.x;
        gp.RealTargetPostion.y = Goto.y;


        long NeedSecs = gp.ExpireTime - GameIndex.ServerTime;
        gp.MoraleExpire = gp.ExpireTime + NeedSecs;
        gp.Teams = new List<int>();
        gp.Teams.AddRange(teams);

        foreach (int teamid in teams)
        {
            Team t = this.GetTeam(teamid);
            t.Status = (int)TeamStatus.Fight;
        }

        this._GroupDic.Add(gp.Id, gp);
        this.AttackCityAction(gp);

        this.DoSaveTeams();
        this.DoSaveGroup();
        VInt2 cityFromPos = WorldProxy._instance.GetCityCordinate(from);
        VInt2 cityTargetPos = WorldProxy._instance.GetCityCordinate(cityid);
        string fName = WorldProxy._instance.GetCityName(from);
        string tName = WorldProxy._instance.GetCityName(cityid);

        VInt2 gamePos = UtilTools.WorldToGameCordinate(cityTargetPos.x, cityTargetPos.y);
        string notice = LanguageConfig.GetLanguage(LanMainDefine.AttackCityMoveOut, fName, teams.Count, tName);
        PopupFactory.Instance.ShowNotice(notice);
        RoleProxy._instance.AddLog(LogType.AttackCity, notice, cityTargetPos);
        this.SendNotification(NotiDefine.MoveToAttackCityResp);
    }

    private void AttackOverBackCityAction(Group gp)
    {
        //路线添加
        PathData path = new PathData();
        path.ID = gp.Id;
        path.Type = PathData.TYPE_GROUP_BACK_ATTACK;
        path.Start = WorldProxy._instance.GetCityCordinate(gp.TargetCityID);
        path.Target = WorldProxy._instance.GetCityCordinate(gp.CityID);
        path.StartTime = gp.StartTime;
        path.ExpireTime = gp.ExpireTime;
        path.Model = "PathModel";//后面读取英灵模型，暂时写死
        path.Picture = "default";
        path.Param = gp;
        PathProxy._instance.AddPath(path);

        //时间监听
        TimeCallData dataTime = new TimeCallData();
        dataTime._key = gp.Id;
        dataTime._notifaction = NotiDefine.AttackCityBackHomeExpireReachedNoti;
        dataTime.TimeStep = gp.ExpireTime;
        dataTime._param = gp.Id;
        MediatorUtil.SendNotification(NotiDefine.AddTimestepCallback, dataTime);
    }

    private void AttackCityAction(Group gp)
    {
        //路线添加
        PathData path = new PathData();
        path.ID = gp.Id;
        path.Type = PathData.TYPE_GROUP_ATTACK;
        path.Start = WorldProxy._instance.GetCityCordinate(gp.CityID);
        path.Target = gp.RealTargetPostion;
        path.StartTime = gp.StartTime;
        path.ExpireTime = gp.ExpireTime;
        path.Model = "PathModel";//后面读取英灵模型，暂时写死
        path.Picture = "default";
        path.Param = gp;
        PathProxy._instance.AddPath(path);

        //时间监听
        TimeCallData dataTime = new TimeCallData();
        dataTime._key = gp.Id;
        dataTime._notifaction = NotiDefine.AttackCityExpireReachedNoti;
        dataTime.TimeStep = gp.ExpireTime;
        dataTime._param = gp.Id;
        MediatorUtil.SendNotification(NotiDefine.AddTimestepCallback, dataTime);
        this.DoSaveGroup();
    }

    public void OnFinishMarchAttackCity(string id, bool needSave = true)
    {
        //PathProxy._instance.RemovePath(questid);不删除，停留再城市那里
        Group data;
        if (this._GroupDic.TryGetValue(id, out data) == false)
            return;
        //判断城市是不是已经被占领了
        bool isOwn = WorldProxy._instance.IsOwnCity(data.TargetCityID);
        if (isOwn)
        {
            this.GroupBackCity(id);
        }
        else
        {
            data.Status = (int)GroupStatus.WaitFight;
            this.DoSaveGroup();
            string fName = WorldProxy._instance.GetCityName(data.CityID);
            string tName = WorldProxy._instance.GetCityName(data.TargetCityID);
            VInt2 cityTargetPos = WorldProxy._instance.GetCityCordinate(data.TargetCityID);

            VInt2 gamePos = UtilTools.WorldToGameCordinate(cityTargetPos.x, cityTargetPos.y);
            string notice = LanguageConfig.GetLanguage(LanMainDefine.AttackCityWaitFight, fName, tName);
            PopupFactory.Instance.ShowNotice(notice);
            RoleProxy._instance.AddLog(LogType.AttackCityWaitFight, notice, cityTargetPos);
        }
    }

    public void OnFinishBackHomeAttackCity(string id, bool needSave = true)
    {
        PathProxy._instance.RemovePath(id);
        Group data;
        if (this._GroupDic.TryGetValue(id, out data) == false)
            return;
        foreach (int t in data.Teams)
        {
            Team team = this.GetTeam(t);
            team.Status = (int)TeamStatus.Idle;
        }
        this.DoSaveTeams();


        string fName = WorldProxy._instance.GetCityName(data.CityID);
        VInt2 cityTargetPos = WorldProxy._instance.GetCityCordinate(data.CityID);

        VInt2 gamePos = UtilTools.WorldToGameCordinate(cityTargetPos.x, cityTargetPos.y);
        string notice = LanguageConfig.GetLanguage(LanMainDefine.AttackCityBack, fName);
        PopupFactory.Instance.ShowNotice(notice);
        RoleProxy._instance.AddLog(LogType.AttackCityBack, notice, cityTargetPos);

        this._GroupDic.Remove(id);
        this.DoSaveGroup();
    }

    public void DoSaveGroup()
    {
        //存储
        CloudDataTool.SaveFile(SaveFileDefine.Group, this._GroupDic);
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

        if ((army == 0 || count == 0) && heroid > 0)
        {
            heroid = 0;//下阵
        }

        Hero heroTeam = HeroProxy._instance.GetHero(team.HeroID);
        Hero newHero = HeroProxy._instance.GetHero(heroid);

        if (heroTeam != null && team.HeroID != heroid)
        {
            //卸下兵种
            ArmyProxy._instance.ChangeArmyCount(team.CityID, team.ArmyTypeID, team.Blood);
            team.Blood = 0;
        }
        else if (heroTeam == null && newHero != null)
        {
            ArmyProxy._instance.ChangeArmyCount(team.CityID, army, -count);
            team.Blood = count;
        }
        else if (heroTeam != null && team.HeroID == heroid)
        {
            if (team.ArmyTypeID == army)
            {
                ArmyProxy._instance.ChangeArmyCount(team.CityID, team.ArmyTypeID, team.Blood - count);
            }
            else
            {
                ArmyProxy._instance.ChangeArmyCount(team.CityID, team.ArmyTypeID, team.Blood);//旧的加回去
                ArmyProxy._instance.ChangeArmyCount(team.CityID, army, -count);//新的减少掉
            }
            team.Blood = count;
        }

        team.HeroID = heroid;
        team.ArmyTypeID = heroid > 0 ? army : 0;
        team.ComputeAttribute();
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
            team.Create(cityid, i + 1);
            this._teams[team.Id] = team;
        }
        this.DoSaveTeams();
    }

    public void LoadGroup()
    {
        string json = CloudDataTool.LoadFile(SaveFileDefine.Group);
        if (json.Equals(string.Empty) == false)
        {
            this._GroupDic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Group>>(json);
        }

        List<string> finishs = new List<string>();
        var it = this._GroupDic.Values.GetEnumerator();
        while (it.MoveNext())
        {
            Group data = it.Current;
            if (data.Status == (int)GroupStatus.Back)
                this.AttackOverBackCityAction(data);
            else
                this.AttackCityAction(data);
        }
        it.Dispose();
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

    public int GetHeroTeamID(int heroid)
    {
        foreach (Team t in this._teams.Values)
        {
            if (t.HeroID == heroid)
                return t.Id;
        }
        return 0;
    }

}//end class
