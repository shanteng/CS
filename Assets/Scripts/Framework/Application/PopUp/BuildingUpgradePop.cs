using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BuildingUpgradePop : Popup
{
    public Text _titleTxt;
    public Text _curLvTxt;
    public Text _nextLvTxt;

    public Text _curDescTxt;
    public Text _nextDescTxt;

    public GameObject _ResCon;
    public List<CostItem> _costs;
    public UIButton _BtnSure;
    public UIButton _BtnCancel;

    private string _key;
    void Start()
    {
        _BtnSure.AddEvent(this.OnClick);
        _BtnCancel.AddEvent(this.OnCancelClick);
        _BtnClose.AddEvent(this.OnCancelClick);
    }

    private void OnCancelClick(UIButton btn)
    {
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
        this._nextLvTxt.text = LanguageConfig.GetLanguage(LanMainDefine.Level, next);

      //  this._curDescTxt.text = BuildingConfig.GetAddOnDesc(config.AddType, configLv.AddValues);
     //   this._nextDescTxt.text = BuildingConfig.GetAddOnDesc(config.AddType, configNext.AddValues);

        bool isStisfy = UtilTools.SetCostList(this._costs, configNext.Cost,true);
        this._BtnSure.IsEnable = isStisfy;
        LayoutRebuilder.ForceRebuildLayoutImmediate(_ResCon.GetComponent<RectTransform>());
    }//end func
}//end class
