using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;



public class NpcTeamRender : ItemRender
{
    public Image _careerSp;
    public Text _countTxt;
    public TextMeshProUGUI _rateTxt;
    public TeamAttributeUi _teamAttrUi;
    public HeroHead _HeadUi;

    private int _NpcTeamID;

    public int ID => this._NpcTeamID;
 
    private void Start()
    {
        
    }


 
    protected override void setDataInner(ScrollData data)
    {
        this.SetData((int)data._Param);
    }

    public void SetData(int id)
    {
        this._NpcTeamID = id;
        NpcTeamConfig configTeam = NpcTeamConfig.Instance.GetData(id);
        Hero hero = HeroProxy._instance.GetHero(configTeam.Hero);
        ArmyConfig config = ArmyConfig.Instance.GetData(configTeam.Army);
        this._countTxt.text = configTeam.Count.ToString();
        int rate = HeroProxy._instance.GetHeroCareerRate(configTeam.Hero, config.Career);
        this._rateTxt.text = Hero.GetCareerEvaluateName(rate);
        this._careerSp.sprite = ResourcesManager.Instance.GetArmySprite(configTeam.Army);
        this._teamAttrUi.SetNpcData(configTeam);
        this._HeadUi.SetData(configTeam.Hero);
        this._HeadUi._levelTxt.text = LanguageConfig.GetLanguage(LanMainDefine.RoleLevel, configTeam.Level);
    }//end func

}


