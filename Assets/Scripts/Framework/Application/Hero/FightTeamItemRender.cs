using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;



public class FightTeamItemRender : ItemRender
{
    public Image _careerSp;
    public TextMeshProUGUI _rateTxt;
    public Text _countTxt;
    public TeamAttributeUi _teamAttrUi;
    public HeroHead _HeadUi;
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
        Team team =  TeamProxy._instance.GetTeam(id);
        Hero hero = HeroProxy._instance.GetHero(team.HeroID);
        ArmyConfig config = ArmyConfig.Instance.GetData(hero.ArmyTypeID);

        this._countTxt.text = hero.Blood.ToString();
        int rate = HeroProxy._instance.GetHeroCareerRate(team.HeroID, config.Career);
        this._rateTxt.text = Hero.GetCareerEvaluateName(rate);
        this._careerSp.sprite = ResourcesManager.Instance.GetArmySprite(hero.ArmyTypeID);
        this._teamAttrUi.SetData(_teamID);
        this._HeadUi.SetData(team.HeroID);
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
        else if (hero.Blood == 0)
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


