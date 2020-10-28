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

        VInt2 Kv = WorldProxy._instance.GetBuildingMaxAndLimitCount(curData._config.ID);
        int count = WorldProxy._instance.GetBuildingCount(curData._config.ID);
        int max = Kv.x;
        int limt = Kv.y;
        bool canClick = count < max;
        this._open.SetActive(canClick);
        this._notOpen.SetActive(!canClick);

        if (canClick)
        {
            //可以建造
            BuildingUpgradeConfig configLevel = BuildingUpgradeConfig.GetConfig(curData._config.ID, 1);
            UtilTools.SetCostList(this._costs, configLevel.Cost, true);
            string cdStr = UtilTools.GetCdString(configLevel.NeedTime);
            this._timeTxt.text = LanguageConfig.GetLanguage(LanMainDefine.BuildCD, cdStr);
            this._countTxt.text = LanguageConfig.GetLanguage(LanMainDefine.HasBuild, count, Kv.x);
        }
        else if (count == limt)
        {
            //到达上限
            this._conditionTxt.text = LanguageConfig.GetLanguage(LanMainDefine.LimitReach);
        }
        else if (count == 0)
        {
            //建造开启条件
            string[] firstlist = curData._config.Condition[0].Split('|');
            int id = UtilTools.ParseInt(firstlist[0]);
            int level = UtilTools.ParseInt(firstlist[1]);
            int bdCount = UtilTools.ParseInt(firstlist[2]);
            BuildingConfig configNeed = BuildingConfig.Instance.GetData(id);
            this._conditionTxt.text = LanguageConfig.GetLanguage(LanMainDefine.BuildOpenCondition, configNeed.Name, level);
        }
        else
        {
            //判断下个数量所需等级
            VInt2 needKv = WorldProxy._instance.GetBuildingNextOpenCondition(curData._config.ID);
            BuildingConfig configNeed = BuildingConfig.Instance.GetData(needKv.x);
            this._conditionTxt.text = LanguageConfig.GetLanguage(LanMainDefine.BuildNextCondition, configNeed.Name, needKv.y);
        }
        UIRoot.Intance.SetImageGray(this._Bg, !canClick);
    }

    
}


