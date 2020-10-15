using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class InfoCanvas : UIBase
{
    public GameObject _Normal;
    public UIButton _btnInfo;
    public UIButton _btnUpgrade;
    public List<UIButton> _btnFunList;
    private BuildingData _data;

    public GameObject _BuildUpgrade;
    public CountDownCanvas _countDown;
    public UIButton _btnCancel;

    private void Start()
    {
        _btnInfo.AddEvent(this.OnClickInfo);
        _btnUpgrade.AddEvent(this.OnClickUpgrade);
        _btnCancel.AddEvent(this.OnCancel);

        foreach (UIButton btn in this._btnFunList)
        {
            btn.AddEvent(this.OnClickFun);
        }
      
    }

    public void SetBuildState(BuildingData data)
    {
        this._data = data;

        bool isBuildUp = this._data._status == BuildingData.BuildingStatus.BUILD ||
           this._data._status == BuildingData.BuildingStatus.UPGRADE;

        this._Normal.SetActive(data._status == BuildingData.BuildingStatus.NORMAL);
        this._BuildUpgrade.SetActive(isBuildUp);

        if (this._data._status == BuildingData.BuildingStatus.NORMAL)
        {
            this.SetNormal();
        }
        else if (isBuildUp)
        {
            this.SetBuildUpgrade();
        }
    }

    private void SetBuildUpgrade()
    {
        this._countDown.DoCountDown(this._data._expireTime,this._data._UpgradeSecs);
    }

    private void SetNormal()
    {
        
    }

    private void OnClickInfo(UIButton btn)
    {
      
    }

    private void OnClickUpgrade(UIButton btn)
    {
        MediatorUtil.SendNotification(NotiDefine.UpgradeOneBuildingDo, this._data._key);
    }

    private void OnClickFun(UIButton btn)
    {

    }

    private void OnCancel(UIButton btn)
    {
        MediatorUtil.SendNotification(NotiDefine.BuildingCancelDo, this._data._key);
    }

}
