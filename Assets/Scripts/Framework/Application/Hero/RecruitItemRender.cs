using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
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
    public Transform _FavorUp;

    public Text _conditionTxt;
    public List<CostBig> _costs;
 
    public UIButton _btnTalk;
    public UIButton _btnGive;
    public UIButton _btnRecruit;
    public UIButton _btnInfo;

    private int _id;

    public int ID => this._id;
    private void Start()
    {
        this._btnTalk.AddEvent(OnClickTalk);
        this._btnGive.AddEvent(OnClickGive);
        this._btnRecruit.AddEvent(OnClickRecruit);
        this._btnInfo.AddEvent(OnClickInfo);
        this._FavorUp.gameObject.SetActive(false);
    }

    private void OnClickTalk(UIButton btn)
    {
        MediatorUtil.SendNotification(NotiDefine.TalkToHeroDo, this._id);
    }

    private void OnClickGive(UIButton btn)
    {

    }

    private void OnClickRecruit(UIButton btn)
    {
        MediatorUtil.SendNotification(NotiDefine.RecruitHeroDo, this._id);
    }

    private void OnClickInfo(UIButton btn)
    {
        
    }

    public void FavorLevelChange()
    {
        Hero hero = HeroProxy._instance.GetHero(this._id);
        FavorLevelConfig configNeed = HeroProxy._instance.GetFaovrConfig(hero.Favor);
        this._FavorTxt.text = LanguageConfig.GetLanguage(LanMainDefine.FavorCurrent, configNeed.Name);
        this._FavorUp.gameObject.SetActive(true);
        Vector3 pos = this._FavorUp.localPosition;
        this._FavorUp.DOPunchPosition(new Vector3(0, 5, 0), 2f, 3, 1).onComplete = () =>
        {
            this._FavorUp.gameObject.SetActive(false);
        };
        this.SetRecruitState();
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
        this.SetRecruitState();
    }

    public void SetRecruitState()
    {
        Hero hero = HeroProxy._instance.GetHero(this._id);
        HeroConfig config = HeroConfig.Instance.GetData(this._id);
        FavorLevelConfig configLv = HeroProxy._instance.GetFaovrConfig(hero.Favor);
        FavorLevelConfig configNeed = FavorLevelConfig.Instance.GetData(config.FavorLevel);
        int reputationNeed = config.NeedPower;
        int myPower = RoleProxy._instance.Role.Power;
        this._btnRecruit.IsEnable = myPower >= reputationNeed || configLv.ID >= configNeed.ID;
    }

}


