using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BuildingInfoPop : Popup
{
    public Text _titleTxt;
    public Text _descTxt;
    public Text _addOnTxt;
    private string _key;
    void Start()
    {
       
    }

    public override void setContent(object data)
    {
        this._key = (string)data;
        BuildingData bd = WorldProxy._instance.GetBuilding(this._key);
        BuildingConfig config = BuildingConfig.Instance.GetData(bd._id);

        this._titleTxt.text = LanguageConfig.GetLanguage(LanMainDefine.ConfirmTitle,config.Name,bd._level);
        this._descTxt.text = config.Desc;
        BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(bd._id, bd._level);
        this._addOnTxt.text = BuildingConfig.GetAddOnDesc(config.AddType, configLv.AddValues);
    }//end func
}//end class
