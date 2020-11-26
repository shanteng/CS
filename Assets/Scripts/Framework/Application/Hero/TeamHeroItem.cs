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
    public GameObject _plusArmy;
    public UITexts _troopTxt;

    public TeamAttributeUi _TeamAttrUi;
    public GameObject _Mask;

    public UITexts _StateTxt;
    private int _teamID;
    private int _heroID;
    private bool _isOpen;

    public int ID => this._teamID;

    private void Start()
    {
        this._BtnArmy.AddEvent(OnArmyClick);
        this._BtnHero.AddEvent(OnHeroClick);
    }

    private void OnArmyClick(UIButton btn)
    {
        if (this._isOpen == false)
            return;
        MediatorUtil.ShowMediator(MediatorDefine.SET_TEAM_HERO, this.ID);
    }

    private void OnHeroClick(UIButton btn)
    {
        if (this._isOpen == false || _heroID == 0)
            return;
        PopupFactory.Instance.ShowHeroDetails(this._heroID);
        //MediatorUtil.ShowMediator(MediatorDefine.SET_TEAM_HERO, this.ID);
    }

    protected override void setDataInner(ScrollData data)
    {
        this.SetData((int)data._Param);
    }

    public void SetData(int id)
    {
        this._teamID = id;
        this._teamIdTxt.text = id.ToString();
        Team team =  TeamProxy._instance.GetTeam(id);
        int openLevel = 0;
        _isOpen = TeamProxy._instance.IsTeamOpen(id, out openLevel);
        _heroID = team.HeroID;
        Hero hero = HeroProxy._instance.GetHero(_heroID);

        bool isIdleState = _isOpen && team.Status == (int)TeamStatus.Idle;

        this._lockTxt.gameObject.SetActive(_isOpen == false);
        this._HeadUi.gameObject.SetActive(_isOpen && hero != null);
        this._Plus.SetActive(_isOpen && hero==null);

        this._BtnArmy.IsEnable = isIdleState;
        this._BtnHero.IsEnable = isIdleState;

       
        this._TeamAttrUi.gameObject.SetActive(_isOpen && hero != null);
        int blood = team.ArmyCount;
        int maxBlood = 1;
        int armyId = 0;
        if (_isOpen == false)
        {
            this._lockTxt.FirstLabel.text = LanguageConfig.GetLanguage(LanMainDefine.TeamOpenCondition, openLevel);
            this._rateUi.SetUnSet();
        }
        else if (_isOpen && hero != null)
        {
            this._rateUi.SetData(_heroID);
            this._HeadUi.SetData(_heroID);
            armyId = team.ArmyTypeID;
            maxBlood = hero.MaxBlood;
        }
        else
        {
            this._rateUi.SetUnSet();
        }

        this._SliderBlood.value = (float)blood / (float)maxBlood;
        this._troopTxt.FirstLabel.text = LanguageConfig.GetLanguage(LanMainDefine.TeamBlood, blood,maxBlood);
        this._troopTxt.FirstLabel.gameObject.SetActive(maxBlood > 1);
        this._IconArmy.gameObject.SetActive(armyId > 0 && _isOpen);
        this._plusArmy.SetActive(armyId == 0 && _isOpen);
        if (armyId > 0)
            this._IconArmy.sprite = ResourcesManager.Instance.GetArmySprite(armyId);

        this._TeamAttrUi.SetData(_teamID);
        this._TeamAttrUi.gameObject.SetActive(_isOpen);
        this._StateTxt.gameObject.SetActive(team.Status != (int)TeamStatus.Idle);
        this._StateTxt.FirstLabel.text = LanguageConfig.GetLanguage(UtilTools.combine(LanMainDefine.TeamStatus, team.Status));
        this._Mask.SetActive(isIdleState == false);
    }//end func

}


