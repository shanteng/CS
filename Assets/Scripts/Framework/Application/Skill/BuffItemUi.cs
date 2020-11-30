using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class BuffItemUi : UIBase
{
    public Text _NameTxt;
    public Text _RoundTxt;
    private UIClickHandler _handler;

    private BattleEffectBuff _buff;
    private string EffectStr;
    private void Awake()
    {
        this._handler = this.GetComponent<UIClickHandler>();
        this._handler.AddListener(OnClick);
    }

  

    private void OnClick(object obj)
    {
        PopupFactory.Instance.ShowText(this.EffectStr);
    }

    public void SetData(BattleEffectBuff buff)
    {
        this._buff = buff;
        SkillEffectConfig configEffect = SkillEffectConfig.Instance.GetData(buff.ID);
        this._NameTxt.text = configEffect.Name;
        this._RoundTxt.text = buff.Duration.ToString();
        //tips里面展示
        string effectstr = UtilTools.formatCustomize(configEffect.Desc, "{Value}", buff.EffectValue, "{Rate}", buff.Rate, "{Active_Rate}", buff.Active_Rate, "{Duration}", buff.Duration);

        string roundStr = buff.Duration.ToString();
        if (buff.Duration > 99)
        {
            roundStr = LanguageConfig.GetLanguage(LanMainDefine.Forever);
            this._RoundTxt.text = "∞";
        }

        this.EffectStr = LanguageConfig.GetLanguage(LanMainDefine.BuffEffectTips, effectstr, roundStr);

    }
}


