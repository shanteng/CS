using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

public class TeamAttributeUi : UIBase
    ,IPointerClickHandler
{
    public UITexts _AttackTxt;
    public UITexts _DefenseTxt;
    public UITexts _SpeedTxt;
    public UITexts _BloodTxt;

    public void OnPointerClick(PointerEventData eventData)
    {
        PopupFactory.Instance.ShowText(LanguageConfig.GetLanguage(LanMainDefine.TeamAttrTips));    
    }

    public void SetNpcData(NpcTeamConfig configNpc)
    {
        ArmyConfig armyConfig = ArmyConfig.Instance.GetData(configNpc.Army);
        HeroConfig config = HeroConfig.Instance.GetData(configNpc.Hero);
        int rateID = HeroProxy._instance.GetHeroCareerRate(configNpc.Hero, armyConfig.Career);

        CareerEvaluateConfig configRate = CareerEvaluateConfig.Instance.GetData(rateID);
        float RateValue = 1f + (float)configRate.Percent / 100f;
        Dictionary<string, float> Attributes = Hero.GetNpcAttribute(configNpc.Hero, configNpc.Level);

        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.AttackRate);
        float atk = (Attributes[AttributeDefine.Attack]  * armyConfig.Attack * RateValue * (float)cfgconst.IntValues[0] * 0.01f);

        cfgconst = ConstConfig.Instance.GetData(ConstDefine.DefenseRate);
        float def = (Attributes[AttributeDefine.Defense]  * armyConfig.Defense * RateValue * (float)cfgconst.IntValues[0] * 0.01f);
        float speed = config.Speed * (1f + (float)armyConfig.SpeedRate / 100f);
        int blood = Mathf.RoundToInt(configNpc.Count * armyConfig.Blood);

        this._AttackTxt.FirstLabel.text = atk.ToString("0.#");
        this._DefenseTxt.FirstLabel.text = def.ToString("0.#");
        this._SpeedTxt.FirstLabel.text = speed.ToString("0.#");
        this._BloodTxt.FirstLabel.text = blood.ToString();
    }

    public void SetData(int teamid)
    {
        Team team = TeamProxy._instance.GetTeam(teamid);
        float atk = 0;
        float def = 0;
        float speed = 0;
        float blood = 0;
        this.gameObject.SetActive(team.HeroID > 0);
        Hero hero = HeroProxy._instance.GetHero(team.HeroID);
        if (hero != null && team.Attributes != null)
        {
            foreach (string key in team.Attributes.Keys)
            {
                if (key.Equals(AttributeDefine.Attack))
                    atk = team.Attributes[key] / (float)hero.Blood;
                else if (key.Equals(AttributeDefine.Defense))
                    def = team.Attributes[key] / (float)hero.Blood;
                if (key.Equals(AttributeDefine.Speed))
                    speed = team.Attributes[key];
                if (key.Equals(AttributeDefine.Blood))
                    blood = team.Attributes[key];
            }
        }

        this._AttackTxt.FirstLabel.text = atk.ToString("0.#");
        this._DefenseTxt.FirstLabel.text = def.ToString("0.#");
        this._SpeedTxt.FirstLabel.text = speed.ToString("0.#");
        this._BloodTxt.FirstLabel.text = blood.ToString();

    }//end func

}


