using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemKey
{
    public const string gold = "gold";
    public const string food = "food";
    public const string wood = "wood";
    public const string metal = "metal";
    public const string stone = "stone";
};

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

    public void Init(CostData d)
    {
        this.id = d.id;
        this.count = d.count;
    }

}

public class HourAwardData
{
    public string id;
    public int add_up_value;//当前已经累计量
    public float base_secs_value;//每秒可产出的量
    public long generate_time;//开始计算的时间
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
