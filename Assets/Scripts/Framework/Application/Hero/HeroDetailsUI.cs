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
    public Slider _expSlider;
    public UIButton _btnExp;
    public TextMeshProUGUI _expTxt;
    public TextMeshProUGUI _lvTxt;
    public Text _MaxBloodTxt;

    private int _id;
    public  void SetData(int id)
    {
        this._id = id;
        Hero hero = HeroProxy._instance.GetHero(id);
        HeroConfig config = HeroConfig.Instance.GetData(id);
        this._NameTxt.text = config.Name;
        this._Element.sprite = ResourcesManager.Instance.GetCommonSprite(config.Element);
        this._StarUi.SetData(hero);
        int canRankCount = hero.Level / 10;
        this._btnStar.gameObject.SetActive(hero.StarRank < canRankCount);
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
    }

    

}


