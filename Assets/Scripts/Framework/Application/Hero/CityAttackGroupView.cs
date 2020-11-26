using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CityAttackGroupView : UIBase
{
    public Text _GroupNameTxt;
    public Text _myCountTxt;
    public DataGrid _GroupGrid;
    public Text _MoraleTxt;
    public CountDownText _StateCDTxt;
    public UIButton _BtnPre;
    public UIButton _BtnNext;
    public Text _PageTxt;


    public Text _CityNameTxt;
    public DataGrid _NpcGrid;
    public Text _NpcCountTxt;
    public UITexts _ExpAddTxt;
    public List<CostBig> _awards;

    public UIButton _BtnFight;

    private int _TotlePage;
    private int _CurPage;
    private string _GroupID;
    private int _CityID;
    private List<string> _groups;

    private string _key;
    void Start()
    {
        this._BtnPre.AddEvent(OnPre);
        this._BtnNext.AddEvent(OnNext);
        this._BtnFight.AddEvent(OnFight);
    }

    private void OnFight(UIButton btn)
    {
        bool isSuccess = TeamProxy._instance.AttackCityDo(this._CityID);
        if (isSuccess)
            MediatorUtil.HideMediator(MediatorDefine.ATTACK_CITY_GROUP);
    }

    private void OnPre(UIButton btn)
    {
        int pre = this._CurPage - 1;
        if (pre < 1)
            return;
        this.SetCurPage(pre);
    }

    private void OnNext(UIButton btn)
    {
        int next = this._CurPage + 1;
        if (next > this._TotlePage)
            return;
        this.SetCurPage(next);
    }

    public  void SetData(int cityid)
    {
        this._CityID = cityid;
        CityConfig config = CityConfig.Instance.GetData(_CityID);
        this._CityNameTxt.text = config.Name;

        int[] npcs = config.NpcTeams;
        int count = 0;
        int addExp = 0;
        TeamProxy._instance.GetFightAwardTotleExp(_CityID, out count, out addExp);

        for (int i = 0; i < npcs.Length; ++i)
        {
            ScrollData teamData = new ScrollData();
            teamData._Param = -npcs[i];
            this._NpcGrid.Data.Add(teamData);
        }
        this._NpcGrid.ShowGrid(null);

        this._NpcCountTxt.text = count.ToString();
        this._ExpAddTxt.FirstLabel.text = addExp.ToString();

        UtilTools.SetFullAwardtList(this._awards, config.AttackDrops);

        this.InitMyCitys();
    }//end func

    private void InitMyCitys()
    {
        _groups = TeamProxy._instance.GetAttackCityGroups(this._CityID);
        this._TotlePage = _groups.Count;
        this.SetCurPage(1);
    }

    private void SetCurPage(int page)
    {
        this._CurPage = page;
        this._BtnNext.IsEnable = (this._CurPage < this._TotlePage);
        this._BtnPre.IsEnable = (this._CurPage > 1);
        this._PageTxt.text = LanguageConfig.GetLanguage(LanMainDefine.Progress, this._CurPage, this._TotlePage);

        this._GroupID = this._groups[this._CurPage - 1];
        Group data = TeamProxy._instance.GetGroup(this._GroupID);
        int fromcityid = data.CityID;
        string cityName = WorldProxy._instance.GetCityName(fromcityid);
        this._GroupNameTxt.text = LanguageConfig.GetLanguage(LanMainDefine.CityGroupName, cityName);

        this._GroupGrid.Data.Clear();
        List<int> teams = data.Teams;
        int totleCount = 0;
        for (int i = 0; i < teams.Count; ++i)
        {
            ScrollData teamData = new ScrollData();
            teamData._Param = teams[i];
            this._GroupGrid.Data.Add(teamData);
            Team team = TeamProxy._instance.GetTeam(teams[i]);
            totleCount += team.ArmyCount;
        }
        this._GroupGrid.ShowGrid(null);

        this._myCountTxt.text = totleCount.ToString();
        int morale = data.GetMorale();
        this._MoraleTxt.text = LanguageConfig.GetLanguage(LanMainDefine.Percent, morale);

        this.UpdateState();
    }

    public void UpdateState()
    {
        Group data = TeamProxy._instance.GetGroup(this._GroupID);
        bool isArrive = GameIndex.ServerTime >= data.ExpireTime;
        if (isArrive == false)
        {
            this._StateCDTxt.DoCountDown(data.ExpireTime, LanMainDefine.Moving);
        }
        else
        {
            this._StateCDTxt.Stop();
            this._StateCDTxt._CDTxt.text = LanguageConfig.GetLanguage(LanMainDefine.ArriveYet);
        }
    }
}//end class
