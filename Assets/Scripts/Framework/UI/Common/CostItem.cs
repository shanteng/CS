using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CostData
{
    public string id;
    public int count;

    public void Init(string keyValueStr)
    {
        string[] list = keyValueStr.Split(':');
        this.id = list[0];
        this.count = UtilTools.ParseInt(list[1]);
    }
}


public class CostItem : UIBase
{
    public Image _Icon;
    public Text _Text;

    public void SetData(CostData data)
    {
        this._Icon.sprite = ResourcesManager.Instance.getAtlasSprite(AtlasDefine.Common, data.id);
        this._Text.text = LanguageConfig.GetLanguage(LanMainDefine.PlusValue, data.count);
    }

}
