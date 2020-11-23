using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;



public class FightTeamItemRender : ItemRender
{
    public BattleTeamRender _teamUi;
    public UITexts _CanNotTxt;
    public Slider _SliderEnegry;
    private int _teamID;

    public int ID => this._teamID;
 
    private void Start()
    {
        
    }

 
    protected override void setDataInner(ScrollData data)
    {
        this.SetData((int)data._Param);
    }

    public void SetData(int id)
    {
        this._teamID = id;
        this._teamUi.SetMyTeam(id);

        Team team =  TeamProxy._instance.GetTeam(id);
        Hero hero = HeroProxy._instance.GetHero(team.HeroID);
        int curEnegry = hero.GetEnegry();
        this._SliderEnegry.value = (float)curEnegry / (float)hero.MaxEnegry;
        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.HeroCostEnegry);
        int needEnegry = cfgconst.IntValues[0];
        this.m_renderData._Key = "";
        if (team.Status != (int)TeamStatus.Idle)
        {
            this._CanNotTxt.gameObject.SetActive(true);
            this._CanNotTxt.FirstLabel.text = LanguageConfig.GetLanguage(LanMainDefine.TeamNotInCity);
        }
        else if (curEnegry < needEnegry)
        {
            this._CanNotTxt.gameObject.SetActive(true);
            this._CanNotTxt.FirstLabel.text = LanguageConfig.GetLanguage(LanMainDefine.EnegryNotEnough);
        }
        else if (team.Blood == 0)
        {
            this._CanNotTxt.gameObject.SetActive(true);
            this._CanNotTxt.FirstLabel.text = LanguageConfig.GetLanguage(LanMainDefine.TeamNoBlood);
        }
        else
        {
            this.m_renderData._Key = "Team";
            this._CanNotTxt.gameObject.SetActive(false);
        }
    }//end func

}


