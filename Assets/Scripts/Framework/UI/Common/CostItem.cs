using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CostItem : UIBase
{
    public Image _Icon;
    public Text _Text;
  
    public bool SetData(CostData data, bool needMy = false)
    {
        this._Icon.sprite = ResourcesManager.Instance.getAtlasSprite(AtlasDefine.Common, data.id);
        this._Text.text = UtilTools.NumberFormat(data.count) ;// LanguageConfig.GetLanguage(LanMainDefine.CountValue, data.count);
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());

        if (needMy)
        {
            int myCount = RoleProxy._instance.GetNumberValue(data.id);
            bool isEnough = myCount >= data.count;
            this._Text.GetComponent<TextColorPingPong>().SetEnable(isEnough == false);
            return isEnough;
        }

        return true;
    }
}
