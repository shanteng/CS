using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildLevelData : ScrollData
{
    public BuildingData _bd;
    public BuildingUpgradeConfig _config;
    public BuildLevelData(BuildingUpgradeConfig cg, BuildingData b)
    {
        this._bd = b;
        this._config = cg;
        this._Key = "BuidLevelRender";
    }
}



public class BuildLevelRender : ItemRender
{
    public GameObject _current;
    public List<Text> _funTexts;
    public Text _levelTxt;
    public Text _powText;
 
    private void Start()
    {
        
    }

    protected override void setDataInner(ScrollData data)
    {
        BuildLevelData curData = (BuildLevelData)data;
       
        this._current.SetActive(curData._bd._level == curData._config.Level);

        this._levelTxt.text = curData._config.Level.ToString();
        this._powText.text = curData._config.Power.ToString();

        List<StringKeyValue> list =  WorldProxy._instance.GetAddOnDesc(curData._bd._id,curData._config.Level);
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
            this._funTexts[i].text = list[i].value;
        }
    }//end func

    
}


