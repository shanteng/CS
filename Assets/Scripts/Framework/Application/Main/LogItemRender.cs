using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class LogItemData : ScrollData
{
    public LogData _data;
    public LogItemData(LogData d)
    {
        this._data = d;
        this._Key = "Log";
    }
}

public class LogItemRender : ItemRender
{
    public GameObject _new;
    public Text TypeTxt;
    public Text Content;
    public Image FunIcon;
    private string _id;
    
    public string IDs => this._id;

    private void Start()
    {
      
    }

 

    protected override void setDataInner(ScrollData dataScroll)
    {
        LogData data = ((LogItemData)dataScroll)._data;
        this._id = data.ID;
        string key = UtilTools.combine(LanMainDefine.LogType, data.Type);
        this.TypeTxt.text = LanguageConfig.GetLanguage(key);
        this._new.SetActive(data.New);
        string TimeStr = UtilTools.getDateFromNowOn(data.Time);
        this.Content.text = LanguageConfig.GetLanguage(LanMainDefine.LogItemContent, TimeStr, data.Content);
        BuildingData bd = WorldProxy._instance.GetBuilding(data.BdKey);
        this.FunIcon.gameObject.SetActive(data.Position != null || bd != null);
    }

}


