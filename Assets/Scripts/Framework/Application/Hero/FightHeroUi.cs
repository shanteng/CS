using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;



public class FightHeroUi : UIBase
{
    public UIButton _btnDel;
    public RoleHead _head;
    public Text _nameTxt;
    private int _teamid;
    public int ID => this._teamid;
    private void Awake()
    {
       
    }


    public int SetData(int teamid)
    {
        this._teamid = teamid;
        Team team = TeamProxy._instance.GetTeam(teamid);
        HeroConfig config = HeroConfig.Instance.GetData(team.HeroID);
        Hero hero = HeroProxy._instance.GetHero(team.HeroID);
        this._head.SetHero(config.ID);
        this._nameTxt.text = config.Name;
        this._btnDel._param._value = this._teamid;
        return team.Blood;
    }
}


