using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RecruitItemData : ScrollData
{
    public Hero _hero;
    public RecruitItemData(Hero cg)
    {
        this._hero = cg;
        this._Key = "Recruit";
    }
}


public class RecruitItemRender : ItemRender
{
    public Image _Frame;
    public Image _Element;
    public Text _nameTxt;
    public HeroStar _starUI;
    public HeroCareerRates _rateUi;

    public Text _FavorTxt;

    public Text _conditionTxt;
    public List<CostBig> _costs;
 
    public UIButton _btnTalk;
    public UIButton _btnGive;
    public UIButton _btnRecruit;
    public UIButton _btnInfo;

    private int _id;
    private void Start()
    {
        this._btnTalk.AddEvent(OnClickTalk);
        this._btnGive.AddEvent(OnClickGive);
        this._btnRecruit.AddEvent(OnClickRecruit);
        this._btnInfo.AddEvent(OnClickInfo);
    }

    private void OnClickTalk(UIButton btn)
    {
        
    }

    private void OnClickGive(UIButton btn)
    {

    }

    private void OnClickRecruit(UIButton btn)
    {

    }

    private void OnClickInfo(UIButton btn)
    {

    }

    protected override void setDataInner(ScrollData data)
    {
        RecruitItemData curData = (RecruitItemData)data;
        this._id = curData._hero.Id;
        HeroConfig config = HeroConfig.Instance.GetData(curData._hero.Id);
        this._nameTxt.text = config.Name;
        this._Frame.sprite = ResourcesManager.Instance.GetCommonFrame(config.Star);
        this._Element.sprite = ResourcesManager.Instance.GetCommonSprite(config.Element);
        this._starUI.SetData(curData._hero);

        FavorLevelConfig configNeed = HeroProxy._instance.GetFaovrConfig(curData._hero.Favor);

        this._FavorTxt.text = LanguageConfig.GetLanguage(LanMainDefine.FavorCurrent, configNeed.Name);
        this._rateUi.SetData(this._id);

        configNeed = FavorLevelConfig.Instance.GetData(config.FavorLevel);
        this._conditionTxt.text = LanguageConfig.GetLanguage(LanMainDefine.RecruitCondition, config.NeedPower, configNeed.Name, configNeed.Name);
        UtilTools.SetCostList(this._costs, config.Cost, true);

    }

    
}


