using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class TeamHeroItem : ItemRender
{
    public Text _teamIdTxt;
    public HeroCareerRates _rateUi;
    public UIButton _BtnHero;
    public HeroHead _HeadUi;
    public UITexts _lockTxt;
    public GameObject _Plus;

    public Slider _SliderBlood;
    public UIButton _BtnArmy;
    public Image _IconArmy;
    public UITexts _troopTxt;

    public UITexts _AttackTxt;
    public UITexts _DefenseTxt;
    public UITexts _SpeedTxt;

    public UITexts _StateTxt;
    private int _teamID;
    

    public int ID => this._teamID;

    public void SetData(int id)
    {
       
        
    }

}


