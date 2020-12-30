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

    public int GetFightArmyExpAdd(int armyID, int deadCount)
    {
        ArmyConfig configArmy = ArmyConfig.Instance.GetData(armyID);
        return Mathf.CeilToInt(configArmy.FightOneExpAdd * (float)deadCount);
    }

    public void GetFightAwardTotleExp(int npcCity, out int count, out int addExp)
    {
        count = 0;
        addExp = 0;
        CityConfig config = CityConfig.Instance.GetData(npcCity);
        int[] npcs = config.NpcTeams;
        for (int i = 0; i < npcs.Length; ++i)
        {
            NpcTeamConfig configNpc = NpcTeamConfig.Instance.GetData(npcs[i]);
            count += configNpc.Count;

            int exp = this.GetFightArmyExpAdd(configNpc.Army, configNpc.Count);
            addExp += exp;
        }
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

    public bool AttackCityDo(int TargetCityID)
    {
        //通知battleproxy
        CityConfig configCIty = CityConfig.Instance.GetData(TargetCityID);
        BattleData battleData = new BattleData();
        battleData.Id = configCIty.BattleSceneID;
        battleData.Type = BattleType.AttackCity;
        battleData.MyPlace = BattlePlace.Attack;
        battleData.Round = 0;
        battleData.IsGameOver = false;
        battleData.IsWin = false;
        battleData.Status = BattleStatus.PreStart;
        battleData.Players = new Dictionary<int, BattlePlayer>();
        battleData.Param = TargetCityID;

        bool hasTeam = false;
        foreach (Group group in this._GroupDic.Values)
        {
            if (group.TargetCityID == TargetCityID && group.Status == (int)GroupStatus.WaitFight)
            {
                group.Status = (int)GroupStatus.Fight;//临时状态不存储
                foreach (int teamid in group.Teams)
                {
                    Team team = this.GetTeam(teamid);
                    if (team.ArmyCount <= 0)
                        continue;//只上阵有兵力的
                    BattlePlayer player = new BattlePlayer();
                    player.InitMy(team, group.GetMorale(), BattlePlace.Attack);
                    battleData.Players.Add(teamid, player);
                    hasTeam = true;
                }
            }
        }//end foreach

        if (hasTeam == false)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.NoTeamCanFight);
            return false;
        }

        //构造敌方
        int index = 1;
        foreach (int npcTeam in configCIty.NpcTeams)
        {
            int npcBorn = configCIty.NpcBorns[index - 1];
            BattlePlayer player = new BattlePlayer();
            player.InitNpc(npcTeam, npcBorn, index, configCIty.BattleSceneID, BattlePlace.Defense);
            battleData.Players.Add(-index, player);
            ++index;
        }
        bool canDo = BattleProxy._instance.EnterBattle(battleData);
        return canDo;
    }

    public void GroupBackCity(string groupid)
    {
        Group gp;
        if (this._GroupDic.TryGetValue(groupid, out gp) == false)
            return;
        if (gp.Status == (int)GroupStatus.Back)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.GroupBacking);
            return;
        }

        if (gp.Status == (int)GroupStatus.Fight)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.GroupFighting);
            return;
        }

        VInt2 StartPos;
        if (HomeLandManager.GetInstance() != null)
        {
            StartPos = HomeLandManager.GetInstance().GetMovingGroupPathPostion(groupid);
        }
        else
        {
            if(gp.Status == (int)GroupStatus.March)
                StartPos = WorldProxy._instance.GetCityCordinate(gp.CityID);
            else
                StartPos = WorldProxy._instance.GetCityCordinate(gp.TargetCityID);
        }


        if (StartPos == null)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.GroupBacking);
            return;
        }

        gp.Status = (int)GroupStatus.Back;
        gp.StartTime = GameIndex.ServerTime;
        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.MoveBackDeltaSces);
        int SecsDelta = cfgconst.IntValues[0];
        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects(gp.CityID);
        float deltaFinal = (1f - effect.TeamMoveReduceRate) * (float)SecsDelta;

        VInt2 Goto = WorldProxy._instance.GetCityCordinate(gp.CityID);
        gp.ExpireTime = WorldProxy._instance.GetMoveExpireTime(StartPos.x, StartPos.y, Goto.x, Goto.y, deltaFinal);//栈道来提升速度百分比
        gp.RealTargetPostion = new VInt2();
        gp.RealTargetPostion.x = Goto.x;
        gp.RealTargetPostion.y = Goto.y;

        gp.RealFromPostion = new VInt2();
        gp.RealFromPostion.x = StartPos.x;
        gp.RealFromPostion.y = StartPos.y;

        PathProxy._instance.RemovePath(groupid);
        this.AttackOverBackCityAction(gp);
        this.DoSaveGroup();
    }

    private void SetGroupTeamResult(BattleData result)
    {
        float woundRecoverRate = (float)ConstConfig.Instance.GetData(ConstDefine.WoundRecoverRate).IntValues[0] * 0.01f;
        bool isWin = result.IsWin;
        //经验获取机制为上阵的英雄平分总击杀数量
        int exp = 0;
        List<int> UpHeroID = new List<int>();
        foreach (BattlePlayer pl in result.Players.Values)
        {
            ArmyConfig config = ArmyConfig.Instance.GetData(pl.ArmyID);
            if (pl.TeamID < 0)
            {
                float loseBlood = pl.Attributes[AttributeDefine.OrignalBlood] - pl.Attributes[AttributeDefine.Blood];
                if (isWin)//胜利的话，认为全部击杀
                    loseBlood = pl.Attributes[AttributeDefine.OrignalBlood];
                int deadCount = Mathf.RoundToInt(loseBlood / config.Blood);
                exp += this.GetFightArmyExpAdd(pl.ArmyID, deadCount);
            }
            else
            {
                Team myTeam = this.GetTeam(pl.TeamID);
                if (myTeam == null || pl.BornIndex == 0)//未上阵的不计算
                    continue;
                UpHeroID.Add(myTeam.HeroID);

                Hero hero = HeroProxy._instance.GetHero(myTeam.HeroID);
                int armyCount = Mathf.RoundToInt(pl.Attributes[AttributeDefine.Blood] / config.Blood);
                if (armyCount > hero.MaxBlood)
                    armyCount = hero.MaxBlood;

                myTeam.ArmyCount = armyCount;
                //50%的伤病可以被医院治疗
                myTeam.WoundCount += Mathf.RoundToInt(pl.Attributes[AttributeDefine.WoundedBlood] * woundRecoverRate / config.Blood);
                myTeam.ComputeAttribute();
            }
        }

        //平分经验
        int heroCount = UpHeroID.Count;
        if (heroCount > 0)
        {
            int eachExp = exp / heroCount;
            for (int i = 0; i < heroCount; ++i)
            {
                HeroProxy._instance.AddExpToHero(UpHeroID[i], eachExp,false);
            }
            HeroProxy._instance.DoSaveHeros();
        }

        this.DoSaveTeams();
    }

    public int GetGroupArmyCount(string gpid)
    {
        int totle = 0;
        Group gp = this.GetGroup(gpid);
        foreach (int id in gp.Teams)
        {
            Team t = this.GetTeam(id);
            totle += t.ArmyCount;
        }
        return totle;
    }

    public void AttackCityEnd(int TargetCityID, BattleData battleResult)
    {
        //更新队伍血量\获得经验\伤病数量
        this.SetGroupTeamResult(battleResult);

        List<string> waitAndFightList = this.GetAttackCityGroups(TargetCityID);
        foreach (string gpid in waitAndFightList)
        {
            Group curGp = this.GetGroup(gpid);

            if (curGp.Status == (int)GroupStatus.Fight)
                curGp.Status = (int)GroupStatus.WaitFight;

            if (curGp.Status == (int)GroupStatus.WaitFight)
            {
                if (battleResult.IsWin)
                {
                    //胜利后所有当前等待的Group都返回城市
                    this.GroupBackCity(gpid);
                }
                else
                {
                    //失败了继续留在城市等待继续战斗,判断当前军团是否有可以出战的，没有就返回，有就留在现场
                    int ArmyCount = this.GetGroupArmyCount(curGp.Id);
                    if(ArmyCount <= 0)
                        this.GroupBackCity(gpid);
                }
            }
        }
        this.DoSaveGroup();

        VInt2 StartPos = WorldProxy._instance.GetCityCordinate(TargetCityID);
        CityConfig config = CityConfig.Instance.GetData(TargetCityID);
        string tName = WorldProxy._instance.GetCityName(TargetCityID);
        VInt2 gamePos = UtilTools.WorldToGameCordinate(StartPos.x, StartPos.y);
        string notice = "";
        if (battleResult.IsWin)
        {
            //发奖励
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
            notice = LanguageConfig.GetLanguage(LanMainDefine.AttackCitySuccess, tName, awardStr);
        }
        else
        {
            notice = LanguageConfig.GetLanguage(LanMainDefine.AttackCityFail, tName);
        }

        //PopupFactory.Instance.ShowNotice(notice);
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
        gp.Status = (int)GroupStatus.March;
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

        gp.RealFromPostion = new VInt2();
        gp.RealFromPostion.x = StartPos.x;
        gp.RealFromPostion.y = StartPos.y;

        long NeedSecs = gp.ExpireTime - GameIndex.ServerTime;
        gp.MoraleExpire = gp.ExpireTime + NeedSecs;
        gp.Teams = new List<int>();
        gp.Teams.AddRange(teams);

        cfgconst = ConstConfig.Instance.GetData(ConstDefine.HeroCostEnegry);
        int CostEnegry = cfgconst.IntValues[0];

        foreach (int teamid in teams)
        {
            Team t = this.GetTeam(teamid);
            t.Status = (int)TeamStatus.Fight;
            HeroProxy._instance.ChangeHeroEnegry(t.HeroID, CostEnegry);
        }
        HeroProxy._instance.DoSaveHeros();

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
        path.Start = gp.RealFromPostion;
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
        path.Start = gp.RealFromPostion;// WorldProxy._instance.GetCityCordinate(gp.CityID);
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
            string fName = WorldProxy._instance.GetCityName(data.CityID);
            string tName = WorldProxy._instance.GetCityName(data.TargetCityID);
            VInt2 cityTargetPos = WorldProxy._instance.GetCityCordinate(data.TargetCityID);
            VInt2 gamePos = UtilTools.WorldToGameCordinate(cityTargetPos.x, cityTargetPos.y);
            string notice = LanguageConfig.GetLanguage(LanMainDefine.AttackCityWaitFight, fName, tName);
            PopupFactory.Instance.ShowNotice(notice);
            RoleProxy._instance.AddLog(LogType.AttackCityWaitFight, notice, cityTargetPos);
        }
        this.SendNotification(NotiDefine.GroupReachCityNoti, id);
        this.DoSaveGroup();
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
            team.WoundCount = 0;//之后给到医院去治疗
        }
        this.DoSaveTeams();

        string fName = WorldProxy._instance.GetCityName(data.CityID);
        VInt2 cityTargetPos = WorldProxy._instance.GetCityCordinate(data.CityID);

        VInt2 gamePos = UtilTools.WorldToGameCordinate(cityTargetPos.x, cityTargetPos.y);
        string notice = LanguageConfig.GetLanguage(LanMainDefine.AttackCityBack, fName);
        PopupFactory.Instance.ShowNotice(notice);
        RoleProxy._instance.AddLog(LogType.AttackCityBack, notice, cityTargetPos);

        this.SendNotification(NotiDefine.GroupBackCityNoti, id);

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
            ArmyProxy._instance.ChangeArmyCount(team.CityID, team.ArmyTypeID, team.ArmyCount);
            team.ArmyCount = 0;
        }
        else if (heroTeam == null && newHero != null)
        {
            ArmyProxy._instance.ChangeArmyCount(team.CityID, army, -count);
            team.ArmyCount = count;
        }
        else if (heroTeam != null && team.HeroID == heroid)
        {
            if (team.ArmyTypeID == army)
            {
                ArmyProxy._instance.ChangeArmyCount(team.CityID, team.ArmyTypeID, team.ArmyCount - count);
            }
            else
            {
                ArmyProxy._instance.ChangeArmyCount(team.CityID, team.ArmyTypeID, team.ArmyCount);//旧的加回去
                ArmyProxy._instance.ChangeArmyCount(team.CityID, army, -count);//新的减少掉
            }
            team.ArmyCount = count;
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

        List<string> backGroups = new List<string>();
        var it = this._GroupDic.Values.GetEnumerator();
        while (it.MoveNext())
        {
            Group data = it.Current;
            if (data.Status == (int)GroupStatus.Back)
                backGroups.Add(data.Id);
            else
                this.AttackCityAction(data);
        }
        it.Dispose();

        foreach (string backid in backGroups)
        {
            Group data = this._GroupDic[backid];
            this.AttackOverBackCityAction(data);
        }
    }

    public void LoadAllTeam()
    {
        this._teams.Clear();
        string fileName = UtilTools.combine(SaveFileDefine.Team);
        string json = CloudDataTool.LoadFile(fileName);
        if (json.Equals(string.Empty) == false)
        {
            this._teams = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, Team>>(json);
            //计算一下存储
            foreach (Team t in this._teams.Values)
            {
                t.ComputeAttribute();
            }
            this.DoSaveTeams();
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
