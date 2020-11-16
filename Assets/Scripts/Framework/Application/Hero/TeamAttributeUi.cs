using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

public class TeamAttributeUi : UIBase
{
    public UITexts _AttackTxt;
    public UITexts _DefenseTxt;
    public UITexts _SpeedTxt;
    public UITexts _BloodTxt;
    public void SetData(int heroid,int armyid,int count)
    {
        Hero hero = HeroProxy._instance.GetHero(heroid);
        ArmyConfig armyConfig = ArmyConfig.Instance.GetData(armyid);


        if (hero == null || armyConfig == null)
        {
            this.gameObject.SetActive(false);
            this._AttackTxt.FirstLabel.text = "0";
            this._DefenseTxt.FirstLabel.text = "0";
            this._SpeedTxt.FirstLabel.text = "0";
            this._BloodTxt.FirstLabel.text = "0";
        }
        else
        {
            HeroConfig config = HeroConfig.Instance.GetData(heroid);
            int rateID = 0;
            for (int i = 0; i < config.CareerRates.Length; ++i)
            {
                int career = i + 1;
                if (career == armyConfig.Career)
                {
                    rateID = config.CareerRates[i];
                    break;
                }
            }//end for

            CareerEvaluateConfig configRate = CareerEvaluateConfig.Instance.GetData(rateID);
            float RateValue = 1f + (float)configRate.Percent / 100f;

            int atk = Mathf.RoundToInt(hero.Attributes[AttributeDefine.Attack] * count * armyConfig.Attack * RateValue);
            int def = Mathf.RoundToInt(hero.Attributes[AttributeDefine.Defense] * count * armyConfig.Defense * RateValue);
            float speed = config.Speed * (1f + (float)armyConfig.SpeedRate / 100f);
            int blood = count * armyConfig.Blood;

            this._AttackTxt.FirstLabel.text = atk.ToString();
            this._DefenseTxt.FirstLabel.text = def.ToString();
            this._SpeedTxt.FirstLabel.text = speed.ToString("f1");
            this._BloodTxt.FirstLabel.text = blood.ToString();
        }

      

    }//end func

}


