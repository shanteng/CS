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
    public Text _levelTxt;
   
    private int _id;

    public int ID => this._id;

    public void SetData(int id)
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
    }

}


