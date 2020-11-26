using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TeamCityItemData : ScrollData
{
    public CityData _info;
    public VInt2 _Target;
    public TeamCityItemData(CityData cg, VInt2 Target = null)
    {
        this._info = cg;
        this._Target = Target;
        this._Key = "TeamCityItemData";
    }
}


public class TeamCityItemRender : ItemRender
{
    public Text _nameTxt;
    public Text _KmTxt;
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
        if (curData._Target != null && this._KmTxt != null)
        {
            VInt2 StartPos = WorldProxy._instance.GetCityCordinate(this._cityID);
            int distance = WorldProxy._instance.GetMoveDistance(StartPos.x, StartPos.y, curData._Target.x, curData._Target.y);
            this._KmTxt.text = LanguageConfig.GetLanguage(LanMainDefine.Distance, distance);
        }
    }

    
}


