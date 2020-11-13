using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TalentPop : Popup
{
    public Text _descTxt;
    public List<UITexts> _FunTexts;
    public UITexts _LuckyTxt;


    public override void setContent(object data)
    {
        int heroid = (int)data;
        HeroConfig config = HeroConfig.Instance.GetData(heroid);

        this._descTxt.text = LanguageConfig.GetLanguage(LanMainDefine.HeroTalentDesc);

        int index = 0;
        foreach (string str in config.Talents)
        {
            string[] kv = config.Talents[index ].Split(':');
            string key = UtilTools.combine(LanMainDefine.Talent, kv[0]);
            _FunTexts[index]._texts[0].text = LanguageConfig.GetLanguage(key);
            _FunTexts[index]._texts[1].text = kv[1];
            index++;
        }
        this._LuckyTxt._texts[0].text = LanguageConfig.GetLanguage(LanMainDefine.LuckyValue, config.Lucky);
        this._LuckyTxt._texts[1].text = LanguageConfig.GetLanguage(LanMainDefine.LuckyDesc);
    }//end func
}//end class
