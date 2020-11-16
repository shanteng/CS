using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TeamCityItemData : ScrollData
{
    public CityData _info;
    public TeamCityItemData(CityData cg)
    {
        this._info = cg;
        this._Key = "TeamCityItemData";
    }
}


public class TeamCityItemRender : ItemRender
{
    public Text _nameTxt;
    private int _cityID;
    public int ID => this._cityID;
    private void Start()
    {
        
    }

    protected override void setDataInner(ScrollData data)
    {
        TeamCityItemData curData = (TeamCityItemData)data;
        this._cityID = curData._info.ID;
        this._nameTxt.text = WorldProxy._instance.GetCityName(curData._info.ID);
    }

    
}


