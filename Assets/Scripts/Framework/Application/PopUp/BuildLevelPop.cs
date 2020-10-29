using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BuildLevelPop : Popup
{
    public Text _titleTxt;
    public List<Text> _funTexts;
    public UIButton _btnBack;
    public DataGrid _vGrid;
    private string _key;
    private PopType _lastWin;
    void Start()
    {
        this._btnBack.AddEvent(OnClickBack);
    }

    private void OnClickBack(UIButton btn)
    {
        if(this._lastWin == PopType.BUILDING)
            PopupFactory.Instance.ShowBuildingInfo(this._key);
        else if (this._lastWin == PopType.BUILDING_UPGRADE)
            PopupFactory.Instance.ShowBuildingUpgrade(this._key);
    }

    public override void setContent(object data)
    {
        Dictionary<string, object> vo = (Dictionary<string, object>)data;
        this._key = (string)vo["key"];
        this._lastWin = (PopType)vo["last"];

        BuildingData bd = WorldProxy._instance.GetBuilding(this._key);
        BuildingConfig config = BuildingConfig.Instance.GetData(bd._id);
        this._titleTxt.text = LanguageConfig.GetLanguage(LanMainDefine.NameLv, config.Name, bd._level);

        List<StringKeyValue> list = WorldProxy._instance.GetAddOnDesc(bd._id, bd._level);
        int count = this._funTexts.Count;
        int len = list.Count;
        for (int i = 0; i < count; ++i)
        {
            if (i >= len)
            {
                this._funTexts[i].gameObject.SetActive(false);
                continue;
            }
            this._funTexts[i].gameObject.SetActive(true);
            this._funTexts[i].text = list[i].key;
        }


        Dictionary<int, BuildingUpgradeConfig> dic =  BuildingUpgradeConfig.Instance.getDataArray();
        _vGrid.Data.Clear();
        foreach (BuildingUpgradeConfig configLv in dic.Values)
        {
            if (configLv.BuildingID == bd._id)
            {
                BuildLevelData dataitem = new BuildLevelData(configLv, bd);
                this._vGrid.Data.Add(dataitem);
            }
        }
        _vGrid.ShowGrid();
    }

}
