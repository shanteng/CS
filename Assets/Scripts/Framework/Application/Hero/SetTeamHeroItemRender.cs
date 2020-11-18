using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class SetTeamHeroItemData : ScrollData
{
    public Hero _hero;
    public int _teamHeroID;
    public SetTeamHeroItemData(Hero cg,int th)
    {
        this._hero = cg;
        this._teamHeroID = th;
        this._Key = "Hero";
    }
}

public class SetTeamHeroItemRender : ItemRender
{
    public HeroHead _Head;
    public GameObject _Current;
    private int _id;

    public int ID => this._id;

    protected override void setDataInner(ScrollData data)
    {
        SetTeamHeroItemData curData = (SetTeamHeroItemData)data;
        this.SetData(curData._hero);
        this._Current.SetActive(this._id == curData._teamHeroID);
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


