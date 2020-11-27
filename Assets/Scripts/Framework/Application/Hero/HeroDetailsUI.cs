using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;



public class HeroDetailsUI : UIBase
{
    public Text _NameTxt;
    public Image _Element;
    public TextMeshProUGUI _ElValueTxt;
    public HeroStar _StarUi;
    public UIButton _btnStar;
    public HeroCareerRates _rateUi;

    public Slider _EnegrySlider;
    public GameObject _EnegryTxtCon;
    public Text _EnegryTxt;

    public GameObject _expCon;
    public Slider _expSlider;
    public UIButton _btnExp;
    public TextMeshProUGUI _expTxt;
    public TextMeshProUGUI _lvTxt;
    public Text _MaxBloodTxt;

    public HeroTalents _talentUi;
    public SkillDetailUi _skillUi;


    private int _id;
    private bool _justShow;
    public void SetData(int id,bool justShow = false)
    {
        this._id = id;
        this._justShow = justShow;
        Hero hero = HeroProxy._instance.GetHero(id);
        HeroConfig config = HeroConfig.Instance.GetData(id);
        this._NameTxt.text = config.Name;
        this._Element.sprite = ResourcesManager.Instance.GetCommonSprite(config.Element);
        this._StarUi.SetData(hero);
        int canRankCount = hero.Level / 10;
        this._btnStar.gameObject.SetActive(hero.StarRank < canRankCount && !justShow);
        this._btnExp.gameObject.SetActive(!justShow);
        this._rateUi.SetData(id);
        this._lvTxt.text = LanguageConfig.GetLanguage(LanMainDefine.RoleLevel, hero.Level);
        HeroLevelConfig configNext = HeroLevelConfig.Instance.GetData(hero.Level + 1);
        if (configNext == null)
        {
            this._expTxt.text = LanguageConfig.GetLanguage(LanMainDefine.FullLevel);
            this._expSlider.value = 1f;
        }
        else
        {
            this._expTxt.text = LanguageConfig.GetLanguage(LanMainDefine.RoleLevelExp,hero.Exp,configNext.Exp);
            this._expSlider.value = (float)hero.Exp / configNext.Exp;
        }
        this._MaxBloodTxt.text = LanguageConfig.GetLanguage(LanMainDefine.HeroMaxBlood,hero.MaxBlood);

        this._EnegryTxt.text = LanguageConfig.GetLanguage(LanMainDefine.EnegryCurMax, hero.GetEnegry(), hero.MaxEnegry);
        this._EnegrySlider.value = (float)hero.GetEnegry() / (float)hero.MaxEnegry;

        this._StarUi.gameObject.SetActive(hero.IsMy);
        this._expCon.SetActive(hero.IsMy);
        this._EnegryTxtCon.SetActive(hero.IsMy);
        if (hero.IsMy == false)
            this._EnegrySlider.value = 1f;

        this._talentUi.SetData(this._id);

        HeroSkillData skData = null;
        if (config.Skills.Length > 0)
        {
            skData = hero.Skills[config.Skills[0]];
            this._skillUi.SetData(skData.ID, skData.Level, skData.Open);
        }
        this._skillUi.gameObject.SetActive(skData != null);
    }
}


