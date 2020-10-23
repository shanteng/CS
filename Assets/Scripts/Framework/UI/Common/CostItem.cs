using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CostItem : UIBase
{
    public Image _Icon;
    public Text _Text;

    public void SetData(CostData data)
    {
        this._Icon.sprite = ResourcesManager.Instance.getAtlasSprite(AtlasDefine.Common, data.id);
        this._Text.text = LanguageConfig.GetLanguage(LanMainDefine.CountValue, data.count);
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
    }
}
