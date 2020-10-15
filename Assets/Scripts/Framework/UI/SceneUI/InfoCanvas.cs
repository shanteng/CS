using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


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
    public Text _nameLv;
    public List<UIButton> _btnFunList;
    public CountDownCanvas _countDown;
    private BuildingData _data;
    private void Start()
    {
        foreach (UIButton btn in this._btnFunList)
        {
            btn.AddEvent(this.OnClickFun);
        }
    }

    private void GetBtnList( out List<IntStrPair> btnTypeList)
    {
        btnTypeList = new List<IntStrPair>();
        IntStrPair data = new IntStrPair(OpType.Info,LanguageConfig.GetLanguage(LanMainDefine.OpInfo));
        btnTypeList.Add(data);

        bool isBuildUp = this._data._status == BuildingData.BuildingStatus.BUILD ||
           this._data._status == BuildingData.BuildingStatus.UPGRADE;
        if (this._data._status == BuildingData.BuildingStatus.BUILD || this._data._status == BuildingData.BuildingStatus.UPGRADE)
        {
            data = new IntStrPair(OpType.Info, LanguageConfig.GetLanguage(LanMainDefine.OpCancel));
            btnTypeList.Add(data);
        }
        else if(this._data._status == BuildingData.BuildingStatus.NORMAL)
        {
            data = new IntStrPair(OpType.Info, LanguageConfig.GetLanguage(LanMainDefine.OpUpgrade));
            btnTypeList.Add(data);
        }
    }

    public void SetBuildState(BuildingData data)
    {
        this._data = data;
        BuildingConfig config = BuildingConfig.Instance.GetData(this._data._id);
        if (this._data._status == BuildingData.BuildingStatus.BUILD)
        {
            this._nameLv.text = config.Name;
        }
        else
        {
            this._nameLv.text = LanguageConfig.GetLanguage(LanMainDefine.NameLv, config.Name, this._data._level);
        }


        List<IntStrPair> btnTypeList;
        this.GetBtnList(out btnTypeList);


        bool isBuildUp = this._data._status == BuildingData.BuildingStatus.BUILD ||this._data._status == BuildingData.BuildingStatus.UPGRADE;
        this._countDown.gameObject.SetActive(isBuildUp);
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
        this._countDown.DoCountDown(this._data._expireTime,this._data._UpgradeSecs);
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
