using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BuildingInfoPop : Popup
{
    public RectTransform _rect;
    public Text _titleTxt;
    public Text _descTxt;
    public List<UITexts> _FunTexts;
    public UITexts _powerText;
    public UIButton _btnLevel;

    private string _key;
    void Start()
    {
        this._btnLevel.AddEvent(OnClickLevel);
    }

    private void OnClickLevel(UIButton btn)
    {
        PopupFactory.Instance.ShowBuildingLevelEffect(this._key);
    }

    public override void setContent(object data)
    {
        this._key = (string)data;
        BuildingData bd = WorldProxy._instance.GetBuilding(this._key);
        BuildingConfig config = BuildingConfig.Instance.GetData(bd._id);

        this._titleTxt.text = LanguageConfig.GetLanguage(LanMainDefine.NameLv, config.Name,bd._level);
        this._descTxt.text = config.Desc;
        BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(bd._id, bd._level);
        this._powerText._texts[0].text = LanguageConfig.GetLanguage(LanMainDefine.Power);
        this._powerText._texts[1].text = configLv.Power.ToString();

        List<StringKeyValue> list = WorldProxy._instance.GetAddOnDesc(bd._id, bd._level);
        int count = this._FunTexts.Count;
        int len = list.Count;
        for (int i = 0; i < count; ++i)
        {
            if (i >= len)
            {
                this._FunTexts[i].Hide();
                continue;
            }
            this._FunTexts[i].Show();
            this._FunTexts[i]._texts[0].text = list[i].key;
            this._FunTexts[i]._texts[1].text = list[i].value;
        }//end for

        //等级效果
        this._btnLevel.gameObject.SetActive(config.MaxLevel > 1);
        LayoutRebuilder.ForceRebuildLayoutImmediate(this._rect);
    }//end func
}//end class
