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
    public HeroHead _Head;
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
        this._Head.SetData(this.ID);

        bool isMy = hero.IsMy;
        this._Head._Star.gameObject.SetActive(isMy);
        this._Head._lvCon.SetActive(isMy);
        UIRoot.Intance.SetImageGray(this._Head._Icon, !isMy);
    }
}


