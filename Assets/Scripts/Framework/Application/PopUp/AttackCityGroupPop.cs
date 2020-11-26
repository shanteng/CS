using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AttackCityGroupPop : Popup
{
    public Text _GroupNameTxt;
    public Text _myCountTxt;
    public DataGrid _GroupGrid;
    public Text _MoraleTxt;
    public Text _TargetTxt;
    public CountDownText _StateCDTxt;
    public UIButton _btnBack;

    private string _GroupID;

    void Start()
    {
        this._btnBack.AddEvent(OnBack);
    }

    private void OnBack(UIButton btn)
    {
        TeamProxy._instance.GroupBackCity(this._GroupID);
        this.HidePop();
    }

    public override void setContent(object data)
    {
        this.SetData((string)data);
    }

    private void SetData(string group)
    {
        this._GroupID = group;
        Group data = TeamProxy._instance.GetGroup(this._GroupID);

        string CityName = "";
        if (data.Status == (int)GroupStatus.Back)
            CityName = WorldProxy._instance.GetCityName(data.CityID);
        else
            CityName = WorldProxy._instance.GetCityName(data.TargetCityID);
        this._TargetTxt.text = CityName;

        string cityFromName = WorldProxy._instance.GetCityName(data.CityID);
        this._GroupNameTxt.text = LanguageConfig.GetLanguage(LanMainDefine.CityGroupName, cityFromName);

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
    }//end func


    public void UpdateState()
    {
        Group data = TeamProxy._instance.GetGroup(this._GroupID);
        bool isArrive = GameIndex.ServerTime >= data.ExpireTime;
        if (isArrive == false)
        {
            if(data.Status == (int)GroupStatus.March)
                this._StateCDTxt.DoCountDown(data.ExpireTime, LanMainDefine.Moving);
            else if (data.Status == (int)GroupStatus.Back)
                this._StateCDTxt.DoCountDown(data.ExpireTime, LanMainDefine.Backing);
        }
        else
        {
            this._StateCDTxt.Stop();
            this._StateCDTxt._CDTxt.text = LanguageConfig.GetLanguage(LanMainDefine.ArriveYet);
        }
        this._btnBack.gameObject.SetActive(data.Status != (int)GroupStatus.Back);
    }

    public void OnBackCity(string id)
    {
        if (id.Equals(this._GroupID))
            PopupFactory.Instance.Hide();
    }
}//end class
