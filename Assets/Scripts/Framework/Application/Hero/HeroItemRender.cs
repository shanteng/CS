using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class HeroItemRData : ScrollData
{
    public Hero _hero;
    public HeroItemRData(Hero cg)
    {
        this._hero = cg;
        this._Key = "Hero";
    }
}

public class HeroItemRender : ItemRender
{
    public HeroStar _Star;
    public Image _Icon;
    public Image _Element;
    public UITexts _levelTxt;
    public HeroQualityBG _Bg;
    private int _id;

    public int ID => this._id;

    protected override void setDataInner(ScrollData data)
    {
        HeroItemRData curData = (HeroItemRData)data;
        this.SetData(curData._hero);
    }
    public void SetData(Hero hero)
    {
        this._id = hero.Id;
        HeroConfig config = HeroConfig.Instance.GetData(_id);
        this._Bg.SetData(config.Star);
        bool isMy = hero.Belong == (int)HeroBelong.My;
        this._Bg.gameObject.SetActive(isMy);
        this._levelTxt.gameObject.SetActive(isMy);
        UIRoot.Intance.SetImageGray(this._Icon, !isMy);
        this._Star.SetData(hero);
        this._levelTxt.FirstLabel.text = hero.Level.ToString();
        this._Icon.sprite = ResourcesManager.Instance.GetHeroSprite(this.ID);
        this._Element.sprite = ResourcesManager.Instance.GetCommonSprite(config.Element);
    }
}


