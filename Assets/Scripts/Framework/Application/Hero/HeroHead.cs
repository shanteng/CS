using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class HeroHead : UIBase
{
    public HeroStar _Star;
    public Image _Icon;
    public Image _Element;
    public TextMeshProUGUI _EleValueTxt;
    public Image _ElementBar;
    public Text _nameTxt;
    public GameObject _lvCon;
    public Text _levelTxt;
    public GameObject _ArmyCon;
    public Image _ArmyIcon;

    private int _id;

    public int ID => this._id;

    public void SetData(int id,int army=0)
    {
        this._id = id;
        Hero hero = HeroProxy._instance.GetHero(id);
        HeroConfig config = HeroConfig.Instance.GetData(id);
        this._Star.SetData(hero);
        this._Icon.sprite = ResourcesManager.Instance.GetHeroHeadSprite(this.ID);
        this._nameTxt.text = config.Name;
        this._levelTxt.text = LanguageConfig.GetLanguage(LanMainDefine.RoleLevel, hero.Level);
        this._Element.sprite = ResourcesManager.Instance.GetCommonSprite(config.Element);
        string bar = UtilTools.combine(config.Element, "_bar");
        this._ElementBar.sprite = ResourcesManager.Instance.GetCommonSprite(bar);
        this._EleValueTxt.text = hero.ElementValue.ToString();

        this._ArmyCon.SetActive(army > 0);
        if (army > 0)
        {
            this._ArmyIcon.sprite = ResourcesManager.Instance.GetArmySprite(army);
        }
    }

}


