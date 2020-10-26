using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
public enum OpType
{
    Enter=1,
    Info,
    Upgrade,
    Cancel,
    AcceptRes,
}

public class IntStrPair
{
    public OpType OpType;
    public string BtnName;
    public IntStrPair(OpType key, string value)
    {
        this.OpType = key;
        this.BtnName = value;
    }
}


public class InfoCanvas : UIBase, IConfirmListener
{
    public RectTransform _NameRect;
    public Text _spotNameTxt;
    public TextMeshProUGUI _cordinateTxt;
    public List<UIButton> _btnFunList;
    public UIButton _btnRes;
    public Image _resIcon;

    public GameObject _cdCon;
    public CountDownCanvas _countDown;
    public UIButton _btnSpeedUp;

    private BuildingData _data;
    private string _AddAttr = "";
    private void Start()
    {
        foreach (UIButton btn in this._btnFunList)
        {
            btn.AddEvent(this.OnClickFun);
        }
        this._btnRes.AddEvent(this.OnClickFun);
        this._btnRes._param._value = OpType.AcceptRes;
        this._btnSpeedUp.AddEvent(this.OnClickSpeedUp);
    }

    private void GetBtnList( out List<IntStrPair> btnTypeList)
    {
        BuildingConfig _config = BuildingConfig.Instance.GetData(this._data._id);
        btnTypeList = new List<IntStrPair>();
        IntStrPair data = new IntStrPair(OpType.Info,LanguageConfig.GetLanguage(LanMainDefine.OpInfo));
        btnTypeList.Add(data);

        bool isBuildUp = this._data._status == BuildingData.BuildingStatus.BUILD ||
           this._data._status == BuildingData.BuildingStatus.UPGRADE;
        if (this._data._status == BuildingData.BuildingStatus.BUILD || this._data._status == BuildingData.BuildingStatus.UPGRADE)
        {
            data = new IntStrPair(OpType.Cancel, LanguageConfig.GetLanguage(LanMainDefine.OpCancel));
            btnTypeList.Add(data);
        }
        else if(this._data._status == BuildingData.BuildingStatus.NORMAL && this._data._level < _config.MaxLevel)
        {
            data = new IntStrPair(OpType.Upgrade, LanguageConfig.GetLanguage(LanMainDefine.OpUpgrade));
            btnTypeList.Add(data);
        }
    }

    private int _needValueShow = 0;
    public void SetBuildState(BuildingData data)
    {
        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.IncomeShowValue);
        _needValueShow = cfgconst.IntValues[0];

        this._data = data;
        BuildingConfig config = BuildingConfig.Instance.GetData(this._data._id);
        string showName = "";
        if (this._data._status == BuildingData.BuildingStatus.BUILD)
        {
            showName = config.Name;
        }
        else
        {
            showName = LanguageConfig.GetLanguage(LanMainDefine.NameLv, config.Name, this._data._level);
        }

        this._spotNameTxt.text = showName;

        VInt2 kv = UtilTools.WorldToGameCordinate(this._data._cordinate.x, this._data._cordinate.y);
        this._cordinateTxt.text = LanguageConfig.GetLanguage(LanMainDefine.SpotCordinate,kv.x,kv.y);

        List<IntStrPair> btnTypeList;
        this.GetBtnList(out btnTypeList);
        var len = btnTypeList.Count;
        int count = this._btnFunList.Count;
        for (int i = 0; i < count; ++i)
        {
            if (i >= len)
            {
                this._btnFunList[i].Hide();
                continue;
            }
            this._btnFunList[i].Show();
            this._btnFunList[i].Label.text = btnTypeList[i].BtnName;
            this._btnFunList[i]._param._value = btnTypeList[i].OpType;
        }

        //是否有资源可以领取
        BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(this._data._id, this._data._level);
        bool isRes = config.AddType.Equals(ValueAddType.HourTax) && configLv != null;
        this._btnRes.gameObject.SetActive(isRes);
        if (isRes)
        {
            CostData add = new CostData();
            add.Init(configLv.AddValues[0]);
            this._AddAttr = add.id;
            int value = RoleProxy._instance.GetCanAcceptIncomeValue(add.id);
            bool isEnable = value >= this._needValueShow;
            this._btnRes.IsEnable = isEnable;
            this._resIcon.sprite = ResourcesManager.Instance.getAtlasSprite(AtlasDefine.Common, add.id);
            UIRoot.Intance.SetImageGray(this._resIcon, !isEnable);
        }
        else
        {
            this._AddAttr = "";
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_NameRect);

        bool isBuildUp = this._data._status == BuildingData.BuildingStatus.BUILD || this._data._status == BuildingData.BuildingStatus.UPGRADE;
        this._cdCon.gameObject.SetActive(isBuildUp);
        if (isBuildUp)
        {
            this.SetBuildUpgrade();
        }
        else
        {
            this._countDown.Stop();
        }
    }

    private void SetBuildUpgrade()
    {
        this._btnSpeedUp.gameObject.SetActive(this._data._status == BuildingData.BuildingStatus.UPGRADE);
        this._countDown.DoCountDown(this._data._expireTime,this._data._UpgradeSecs);
    }


    private void OnClickFun(UIButton btn)
    {
        OpType op = (OpType)btn._param._value;
        switch (op)
        {
            case OpType.Enter:
                {
                    break;
                }
            case OpType.Info:
                {
                    PopupFactory.Instance.ShowBuildingInfo(this._data._key);
                    break;
                }
            case OpType.Upgrade:
                {
                    PopupFactory.Instance.ShowBuildingUpgrade(this._data._key);
                    break;
                }
            case OpType.Cancel:
                {
                    if(this._data._status == BuildingData.BuildingStatus.UPGRADE)
                        PopupFactory.Instance.ShowConfirm(LanguageConfig.GetLanguage(LanMainDefine.CancelUpgradeNotice), this, "Cancel");
                    else if (this._data._status == BuildingData.BuildingStatus.BUILD)
                        PopupFactory.Instance.ShowConfirm(LanguageConfig.GetLanguage(LanMainDefine.CancelBuildNotice), this, "Cancel");
                    //MediatorUtil.SendNotification(NotiDefine.BuildingCancelDo, this._data._key);
                    break;
                }
            case OpType.AcceptRes:
                {
                    MediatorUtil.SendNotification(NotiDefine.AcceptHourAwardDo, this._AddAttr);
                    break;
                }
        }
        HomeLandManager.GetInstance().HideInfoCanvas();
    }

    public void OnConfirm(ConfirmData data)
    {
        if (data.userKey.Equals("Cancel"))
        {
            MediatorUtil.SendNotification(NotiDefine.BuildingCancelDo, this._data._key);
        }
    }

    private void OnClickSpeedUp(UIButton btn)
    {
        MediatorUtil.SendNotification(NotiDefine.BuildingSpeedUpDo, this._data._key);
    }

   

}
