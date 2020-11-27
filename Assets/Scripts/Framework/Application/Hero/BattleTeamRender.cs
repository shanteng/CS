using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;



public class BattleTeamRender : ItemRender
{
    public Image _careerSp;
    public Text _countTxt;
    public TextMeshProUGUI _rateTxt;
    public TeamAttributeUi _teamAttrUi;
    public HeroHead _HeadUi;

    private int _TeamID;

    public int ID => this._TeamID;
 
    private void Start()
    {
        
    }


 
    protected override void setDataInner(ScrollData data)
    {
        int id = (int)data._Param;
        if (id < 0)
            this.SetNpcTeam(-id);
        else
            this.SetMyTeam(id);
    }

    public void SetNpcTeam(int id)
    {
        this._TeamID = id;
        NpcTeamConfig configTeam = NpcTeamConfig.Instance.GetData(id);
        Hero hero = HeroProxy._instance.GetHero(configTeam.Hero);
        ArmyConfig config = ArmyConfig.Instance.GetData(configTeam.Army);
        this._countTxt.text = configTeam.Count.ToString();
        int rate = HeroProxy._instance.GetHeroCareerRate(configTeam.Hero, config.Career);
        this._rateTxt.text = Hero.GetCareerEvaluateName(rate);
        this._careerSp.sprite = ResourcesManager.Instance.GetArmySprite(configTeam.Army);
        this._teamAttrUi.SetNpcData(configTeam);
        this._HeadUi.SetData(configTeam.Hero,configTeam.Army);
        this._HeadUi._levelTxt.text = LanguageConfig.GetLanguage(LanMainDefine.RoleLevel, configTeam.Level);
    }//end func

    public void SetMyTeam(int id)
    {
        Team team = TeamProxy._instance.GetTeam(id);
        Hero hero = HeroProxy._instance.GetHero(team.HeroID);
        ArmyConfig config = ArmyConfig.Instance.GetData(team.ArmyTypeID);
        this._countTxt.text = team.ArmyCount.ToString();
        int rate = HeroProxy._instance.GetHeroCareerRate(team.HeroID, config.Career);
        this._rateTxt.text = Hero.GetCareerEvaluateName(rate);
        this._careerSp.sprite = ResourcesManager.Instance.GetArmySprite(team.ArmyTypeID);
        this._teamAttrUi.SetData(id);
        this._HeadUi.SetData(team.HeroID, team.ArmyTypeID);
        UIRoot.Intance.SetImageGray(this._HeadUi._Icon, team.ArmyCount <= 0);
    }

}


