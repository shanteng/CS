using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CareerEvaluatePop : Popup
{
    public Text _descTxt;
    public List<UITexts> _FunTexts;



    public override void setContent(object data)
    {
        this._descTxt.text = LanguageConfig.GetLanguage(LanMainDefine.CareerEvaluateDesc);
        Dictionary<int, CareerEvaluateConfig> dic = CareerEvaluateConfig.Instance.getDataArray();
        int index = 0;
        foreach (CareerEvaluateConfig config in dic.Values)
        {
            _FunTexts[index]._texts[0].text = Hero.GetCareerEvaluateName(config.ID);
            _FunTexts[index]._texts[1].text = UtilTools.GetPercentAddOn(config.Percent);
            index++;
        }
    }//end func
}//end class
