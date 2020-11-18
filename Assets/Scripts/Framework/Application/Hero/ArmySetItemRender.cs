using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
public class ArmySetItemData : ScrollData
{
    public int _armyId;
    public int _heroId;
    public int _originalCount;
    public int _useCount;
    public int _heroArmy;
    public ArmySetItemData(int id,int oldCount)
    {
        this._armyId = id;
        this._originalCount = oldCount;
        this._useCount = 0;
        this._heroId = 0;
        this._heroArmy = 0;
        this._Key = "Army";
    }
}

public class ArmySetItemRender : ItemRender
{
    public Image _Icon;
    public TextMeshProUGUI _RateTxt;
    public TextMeshProUGUI _CountTxt;
    public GameObject _current;
    private int _id;

    public int ID => this._id;

    protected override void setDataInner(ScrollData data)
    {
        ArmySetItemData curData = (ArmySetItemData)data;
        this._id = curData._armyId;
        ArmyConfig config = ArmyConfig.Instance.GetData(curData._armyId);
        this._Icon.sprite = ResourcesManager.Instance.GetCareerIcon(config.Career);
        this.UpdateRate();
        this.UpdateCount();
    }

    public void UpdateRate()
    {
        ArmySetItemData data = (ArmySetItemData)this.m_renderData;
        ArmyConfig config = ArmyConfig.Instance.GetData(data._armyId);
        int rate = HeroProxy._instance.GetHeroCareerRate(data._heroId, config.Career);
        this._RateTxt.text = Hero.GetCareerEvaluateName(rate);
        this._current.SetActive(data._armyId == data._heroArmy);
    }

    public void UpdateCount()
    {
        ArmySetItemData data = (ArmySetItemData)this.m_renderData;
        int current = data._originalCount - data._useCount;
        this._CountTxt.text = current.ToString();
    }
}


