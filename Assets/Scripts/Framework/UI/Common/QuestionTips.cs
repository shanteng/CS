using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionTips : UIBase
{
    public string LanKey = "";
    private void Awake()
    {
        this.GetComponent<UIButton>().AddEvent(OnClick);
    }

    private void OnClick(UIButton btn)
    {
        if (LanKey.Equals("") == false)
        {
            PopupFactory.Instance.ShowText(LanguageConfig.GetLanguage(LanKey));
        }
    }
}
