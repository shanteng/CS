using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CostItem : UIBase
{
    public Image _Icon;
    public Text _Text;
    public Animator _NotEnoughtAni;

    public bool SetData(CostData data, bool needMy = false)
    {
        this._Icon.sprite = ResourcesManager.Instance.getAtlasSprite(AtlasDefine.Common, data.id);
        this._Text.text = LanguageConfig.GetLanguage(LanMainDefine.CountValue, data.count);
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());

        if (needMy)
        {
            int myCount = RoleProxy._instance.GetNumberValue(data.id);
            _NotEnoughtAni.enabled = myCount < data.count;
            return myCount >= data.count;
        }

        return true;
    }
}
