using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BuildingUpgradePop : Popup
{
    public RectTransform _LvCon;
    public Text _titleTxt;
    public Text _curLvTxt;
    public Text _nextLvTxt;

    public List<UITexts> _FunTexts;
    public UITexts _powerText;

    public RectTransform _ResCon;
    public List<CostItem> _costs;
   
    public Text _CdTxt;
    public UIButton _BtnSure;

    public UIButton _btnGo;
    public UIButton _btnLvEffect;
    public Text _NeedTxt;

    private string _key;
    private int _needId;
    void Start()
    {
        _BtnSure.AddEvent(this.OnClick);
        this._btnGo.AddEvent(OnClickGo);
        this._btnLvEffect.AddEvent(OnClickLevelEffect);
    }

    private void OnClickLevelEffect(UIButton btn)
    {
        PopupFactory.Instance.ShowBuildingLevelEffect(this._key, PopType.BUILDING_UPGRADE);
    }

    private void OnClickGo(UIButton btn)
    {
        MediatorUtil.SendNotification(NotiDefine.GO_TO_SELEC_BUILDING_BY_ID, this._needId);
        this.HidePop();
    }

    private void OnClick(UIButton btn)
    {
        MediatorUtil.SendNotification(NotiDefine.UpgradeOneBuildingDo, this._key);
        this.HidePop();
    }

    public override void setContent(object data)
    {
        this._key = (string)data;
        BuildingData bd = WorldProxy._instance.GetBuilding(this._key);
        BuildingConfig config = BuildingConfig.Instance.GetData(bd._id);
        this._titleTxt.text = config.Name;

        int next = bd._level + 1;

        BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(bd._id, bd._level);
        BuildingUpgradeConfig configNext = BuildingUpgradeConfig.GetConfig(bd._id, next);

        this._curLvTxt.text = LanguageConfig.GetLanguage(LanMainDefine.Level, bd._level);
        this._nextLvTxt.text = next.ToString();

        List<StringKeyValue> list = WorldProxy._instance.GetAddOnDesc(bd._id, bd._level);
        List<StringKeyValue> listNext = WorldProxy._instance.GetAddOnDesc(bd._id, bd._level+1);
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
            this._FunTexts[i]._texts[1].text = LanguageConfig.GetLanguage(LanMainDefine.CurNextValue, list[i].value, listNext[i].value);
        }//end for

        this._powerText._texts[0].text = LanguageConfig.GetLanguage(LanMainDefine.Power);
        this._powerText._texts[1].text = LanguageConfig.GetLanguage(LanMainDefine.CurNextValue, configLv.Power, configNext.Power);

        this._CdTxt.text = UtilTools.GetCdString(configNext.NeedTime);

        bool isConditonOK = WorldProxy._instance.IsUpgradeConditonStaisfy(this._key);
        bool isStisfy = UtilTools.SetCostList(this._costs, configNext.Cost,true);
        this._BtnSure.IsEnable = isStisfy;
        this._BtnSure.gameObject.SetActive(isConditonOK);
        this._CdTxt.gameObject.SetActive(isConditonOK);
        this._btnGo.gameObject.SetActive(isConditonOK == false);
        this._NeedTxt.gameObject.SetActive(isConditonOK == false);
        if (isConditonOK == false && configNext.Condition.Length > 1)
        {
            this._needId = configNext.Condition[0];
            BuildingConfig configNeed = BuildingConfig.Instance.GetData(this._needId);
            this._NeedTxt.text = LanguageConfig.GetLanguage(LanMainDefine.NeedBuildingLevel, configNext.Condition[1], configNeed.Name);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_LvCon);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_ResCon);
    }//end func
}//end class
