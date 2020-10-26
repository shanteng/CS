using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuidItemData : ScrollData
{
    public BuildingConfig _config;
    public BuidItemData(BuildingConfig cg)
    {
        this._config = cg;
        this._Key = "BuidItemRender";
    }
}


public class BuidItemRender : ItemRender
{
    public Image _Bg;
    public Text _nameTxt;
    public Text _descTxt;

    public GameObject _open;
    public List<CostItem> _costs;
    public Text _timeTxt;
    public Text _countTxt;

    public GameObject _notOpen;
    public Text _conditionTxt;
    private void Start()
    {
        
    }

    protected override void setDataInner(ScrollData data)
    {
        BuidItemData curData = (BuidItemData)data;
        this._nameTxt.text = curData._config.Name;
        this._descTxt.text = curData._config.Desc;
        bool isOpen =WorldProxy._instance.IsBuildingOpen(curData._config.ID);
        this._open.SetActive(isOpen);
        this._notOpen.SetActive(!isOpen);
        bool canClick = false;
        if (isOpen)
        {
            BuildingUpgradeConfig configLevel = BuildingUpgradeConfig.GetConfig(curData._config.ID, 1);
            UtilTools.SetCostList(this._costs, configLevel.Cost,true);
            string cdStr = UtilTools.GetCdString(configLevel.NeedTime);
            this._timeTxt.text = LanguageConfig.GetLanguage(LanMainDefine.BuildCD, cdStr);
            int count = WorldProxy._instance.GetBuildingCount(curData._config.ID);
            this._countTxt.text = LanguageConfig.GetLanguage(LanMainDefine.HasBuild, count,curData._config.BuildMax);
            canClick = count < curData._config.BuildMax;
        }
        else
        {
            BuildingConfig configNeed = BuildingConfig.Instance.GetData(curData._config.Condition[0]);
            this._conditionTxt.text = LanguageConfig.GetLanguage(LanMainDefine.BuildOpenCondition, configNeed.Name, curData._config.Condition[1]);
        }

        UIRoot.Intance.SetImageGray(this._Bg, !canClick);
    }

    
}


