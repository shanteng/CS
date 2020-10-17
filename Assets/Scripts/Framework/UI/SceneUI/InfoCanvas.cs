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


public class InfoCanvas : UIBase
{
    public Text _spotNameTxt;
    public TextMeshProUGUI _cordinateTxt;
    public List<UIButton> _btnFunList;

    public GameObject _cdCon;
    public CountDownCanvas _countDown;
    public UIButton _btnSpeedUp;

    private BuildingData _data;
    private void Start()
    {
        foreach (UIButton btn in this._btnFunList)
        {
            btn.AddEvent(this.OnClickFun);
        }
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

    public void SetBuildState(BuildingData data)
    {
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
                    MediatorUtil.SendNotification(NotiDefine.ShowBuildingInfo, this._data._key);
                    break;
                }
            case OpType.Upgrade:
                {
                    MediatorUtil.SendNotification(NotiDefine.UpgradeOneBuildingDo, this._data._key);
                    break;
                }
            case OpType.Cancel:
                {
                    MediatorUtil.SendNotification(NotiDefine.BuildingCancelDo, this._data._key);
                    break;
                }
        }
    }

    private void OnClickSpeedUp(UIButton btn)
    {
        MediatorUtil.SendNotification(NotiDefine.BuildingSpeedUpDo, this._data._key);
    }

   

}
