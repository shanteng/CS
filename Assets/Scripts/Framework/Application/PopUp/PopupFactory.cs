﻿using SMVC.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum PopType
{
    NONE = 0,
    COMFIRM,
    BUILDING,
    NOTICE,
    BUILDING_UPGRADE,
    BUILDING_LEVEL_EFFECT,
    CAREER_RATE,
    ATTRADD,
    PATROL,
    CITY_INFO,
    QUEST_CITY,
    HERO_TALENT,
    TEXT,
    ATTACK_GROUP,
    HERO_DETAILS,
    SKILL,
};

public class PopupFactory : SingletonFactory<PopupFactory>
{
    private Popup _curShowWin = null;
    private PopType _curShowType = PopType.NONE;
    private Popup _ClickScreenHideWin;
    public void Hide()
    {
        if (this._curShowWin != null && this._curShowWin.gameObject != null)
        {
            GameObject.Destroy(this._curShowWin.gameObject);
            this._curShowWin = null;
            _curShowType = PopType.NONE;
        }
    }//end func


    public void HideSingle()
    {
        if (this._ClickScreenHideWin != null)
        {
            GameObject.Destroy(this._ClickScreenHideWin.gameObject);
            this._ClickScreenHideWin = null;
        }
    }

    public GameObject ClickHideWin => this._ClickScreenHideWin != null ? this._ClickScreenHideWin.gameObject : null;


    public void HandleNoti(INotification notification)
    {
        if (this._curShowWin == null || this._curShowWin.gameObject.activeSelf == false)
            return;
        switch (notification.Name)
        {
            case NotiDefine.PatrolFinishNoti:
                {
                    if (this._curShowType == PopType.PATROL)
                    {
                        (this._curShowWin as PatrolPop).SetList();
                    }
                    break;
                }
            case NotiDefine.GroupReachCityNoti:
                {
                    if (this._curShowType == PopType.ATTACK_GROUP)
                    {
                        (this._curShowWin as AttackCityGroupPop).UpdateState();
                    }
                    break;
                }
            case NotiDefine.GroupBackCityNoti:
                {
                    if (this._curShowType == PopType.ATTACK_GROUP)
                    {
                        string gpid = (string)notification.Body;
                        (this._curShowWin as AttackCityGroupPop).OnBackCity(gpid);
                    }
                    break;
                }
        }
    }
    public void ShowConfirmBy(ConfirmData data)
    {
        this.ShowPop(PopType.COMFIRM, data);
    }

    public void ShowConfirm(string content, IConfirmListener listener = null, string userKey = "", object param = null)
    {
        ConfirmData data = new ConfirmData();
        data.contentText = content;
        data.listener = listener;
        data.param = param;
        data.userKey = userKey;
        this.ShowConfirmBy(data);
    }


    public void ShowText(string content)
    {
        this.ShowSinglePop(PopType.TEXT, content);
    }

    public void ShowSkill(BattleSkill data)
    {
        this.ShowPop(PopType.SKILL, data);
    }

    public void ShowHeroDetails(int id)
    {
        this.ShowPop(PopType.HERO_DETAILS, id);
    }

    public void ShowAttackGroup(string id)
    {
        this.ShowPop(PopType.ATTACK_GROUP, id);
    }

    public void ShowHeroTalent(int id)
    {
        this.ShowPop(PopType.HERO_TALENT, id);
    }

    public void ShowCareerRate()
    {
        this.ShowPop(PopType.CAREER_RATE,0);
    }

    public void ShowQuestNpcCity(int city)
    {
        this.ShowPop(PopType.QUEST_CITY, city);
    }

 
    public void ShowNpcCityInfo(int city)
    {
        this.ShowPop(PopType.CITY_INFO, city);
    }


    public void ShowBuildingInfo(string bdKey)
    {
        this.ShowPop(PopType.BUILDING, bdKey);
    }

    public void ShowPatrol(VInt2 Target)
    {
        this.ShowPop(PopType.PATROL, Target);
    }

    public void ShowBuildingLevelEffect(string bdKey, PopType lastPop)
    {
        Dictionary<string, object> vo = new Dictionary<string, object>();
        vo["key"] = bdKey;
        vo["last"] = lastPop;
        this.ShowPop(PopType.BUILDING_LEVEL_EFFECT, vo);
    }

    public void ShowBuildingUpgrade(string bdKey)
    {
        this.ShowPop(PopType.BUILDING_UPGRADE, bdKey);
    }

    public void ShowNotice(string notice,string icon= "")
    {
        StringKeyValue kv = new StringKeyValue();
        kv.key = notice;
        kv.value = icon;
        this.ShowDestoryPop(PopType.NOTICE, kv);
    }

    public void ShowAttrAdd(AttrAddData data)
    {
        this.ShowDestoryPop(PopType.ATTRADD, data);
    }

    public void ShowErrorNotice(string errorCode, params object[] paramName)
    {
        string notice =  LanErrorConfig.GetLanguage(errorCode,paramName);
        this.ShowNotice(notice);
        MediatorUtil.SendNotification(NotiDefine.ErrorCode, errorCode);
    }

    Coroutine _cor;
    private void ShowSinglePop(PopType type, object content)
    {
        Popup ShowWin = null;
        switch (type)
        {
            case PopType.TEXT:
                {
                    ShowWin = InitText();
                    break;
                }
        }

        this.HideSingle();
        if (ShowWin != null)
        {
            ShowWin.setContent(content);
            ShowWin.gameObject.SetActive(true);
            ShowWin.GetComponent<CanvasGroup>().alpha = 0;
            //等待下一帧
            if (this._cor != null)
            {
                CoroutineUtil.GetInstance().Stop(this._cor);
                this._cor = null;
            }
            _cor = CoroutineUtil.GetInstance().WaitTime(0, true, OnWaitEnd,ShowWin);
        }
    }

    private void OnWaitEnd(object[] param)
    {
        Popup ShowWin = (Popup)param[0];
        ShowWin.GetComponent<CanvasGroup>().alpha =1f;
        UIRoot.Intance.AdjustUIInMouseInputPos(ShowWin.gameObject, ShowWin._layer);
        this._ClickScreenHideWin = ShowWin;
    }

    private void ShowDestoryPop(PopType type, object content)
    {
        Popup ShowWin = null;
        switch (type)
        {
            case PopType.NOTICE:
                {
                    ShowWin = InitNotice();
                    break;
                }
            case PopType.ATTRADD:
                {
                    ShowWin = InitAttrAdd();
                    break;
                }
        }
        if (ShowWin != null)
        {
            ShowWin.gameObject.SetActive(true);
            ShowWin.setContent(content);
        }
    }

    private void ShowPop(PopType type, object content)
    {
        this.Hide();
        switch (type)
        {
            case PopType.COMFIRM:
                {
                    _curShowWin = InitConfirm();
                    break;
                }
            case PopType.BUILDING:
                {
                    _curShowWin = InitBuilding();
                    break;
                }
            case PopType.BUILDING_UPGRADE:
                {
                    _curShowWin = InitBuildingUpgrade();
                    break;
                }
            case PopType.BUILDING_LEVEL_EFFECT:
                {
                    _curShowWin = InitBuildingLevelEffect();
                    break;
                }
            case PopType.CAREER_RATE:
                {
                    _curShowWin = InitCareerRate();
                    break;
                }
            case PopType.PATROL:
                {
                    _curShowWin = InitPatrol();
                    break;
                }
            case PopType.HERO_TALENT:
                {
                    _curShowWin = InitTalent();
                    break;
                }
            case PopType.QUEST_CITY:
                {
                    _curShowWin = InitQuestCity();
                    break;
                }
            case PopType.ATTACK_GROUP:
                {
                    _curShowWin = InitAttackGroup();
                    break;
                }
            case PopType.HERO_DETAILS:
                {
                    _curShowWin = InitHeroDetails();
                    break;
                }
            case PopType.SKILL:
                {
                    _curShowWin = InitSkill();
                    break;
                }
        }
        this._curShowType = type;
        this._curShowWin.gameObject.SetActive(true);
        this._curShowWin.setContent(content);
        MediatorUtil.SendNotification(NotiDefine.WINDOW_HAS_SHOW);
    }



    protected Popup InitConfirm()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("ConfirmPop");
        Popup script = view.GetComponent<Popup>();
        
        ConfirmPop scriptClone  =   UIRoot.Intance.InstantiateUIInCenter(view, script._layer, script._SetAnchor).GetComponent<ConfirmPop>();
        return scriptClone;
    }

    protected Popup InitBuilding()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("BuildingInfoPop");
        Popup script = view.GetComponent<Popup>();

        BuildingInfoPop scriptClone = UIRoot.Intance.InstantiateUIInCenter(view, script._layer, script._SetAnchor).GetComponent<BuildingInfoPop>();
        return scriptClone;
    }

    protected Popup InitBuildingUpgrade()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("BuildingUpgradePop");
        Popup script = view.GetComponent<Popup>();

        BuildingUpgradePop scriptClone = UIRoot.Intance.InstantiateUIInCenter(view, script._layer, script._SetAnchor).GetComponent<BuildingUpgradePop>();
        return scriptClone;
    }

    protected Popup InitBuildingLevelEffect()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("BuildingLevelInfoPop");
        Popup script = view.GetComponent<Popup>();

        BuildLevelPop scriptClone = UIRoot.Intance.InstantiateUIInCenter(view, script._layer, script._SetAnchor).GetComponent<BuildLevelPop>();
        return scriptClone;
    }

    protected Popup InitCareerRate()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("CareerEvaluatePop");
        Popup script = view.GetComponent<Popup>();

        CareerEvaluatePop scriptClone = UIRoot.Intance.InstantiateUIInCenter(view, script._layer, script._SetAnchor).GetComponent<CareerEvaluatePop>();
        return scriptClone;
    }

    protected Popup InitNotice()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("NoticePop");
        Popup script = view.GetComponent<Popup>();

        NoticePop scriptClone = UIRoot.Intance.InstantiateUIInCenter(view, script._layer, script._SetAnchor).GetComponent<NoticePop>();
        GameObject.Destroy(scriptClone.gameObject, scriptClone._DestorySecs);
        return scriptClone;
    }

    protected Popup InitAttrAdd()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("AttrAddPop");
        Popup script = view.GetComponent<Popup>();

        AttrAddPop scriptClone = UIRoot.Intance.InstantiateUIInCenter(view, script._layer, script._SetAnchor).GetComponent<AttrAddPop>();
        GameObject.Destroy(scriptClone.gameObject, scriptClone._DestorySecs);
        return scriptClone;
    }

    protected Popup InitPatrol()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("PatrolPop");
        Popup script = view.GetComponent<Popup>();
        PatrolPop scriptClone = UIRoot.Intance.InstantiateUIInCenter(view, script._layer, script._SetAnchor).GetComponent<PatrolPop>();
        return scriptClone;
    }

    protected Popup InitTalent()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("TalentPop");
        Popup script = view.GetComponent<Popup>();
        TalentPop scriptClone = UIRoot.Intance.InstantiateUIInCenter(view, script._layer, script._SetAnchor).GetComponent<TalentPop>();
        return scriptClone;
    }

    protected Popup InitQuestCity()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("QuestCityPop");
        Popup script = view.GetComponent<Popup>();
        QuestCityPop scriptClone = UIRoot.Intance.InstantiateUIInCenter(view, script._layer, script._SetAnchor).GetComponent<QuestCityPop>();
        return scriptClone;
    }

    protected Popup InitAttackGroup()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("AttackGroupPop");
        Popup script = view.GetComponent<Popup>();
        AttackCityGroupPop scriptClone = UIRoot.Intance.InstantiateUIInCenter(view, script._layer, script._SetAnchor).GetComponent<AttackCityGroupPop>();
        return scriptClone;
    }

    protected Popup InitText()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("TextPop");
        Popup script = view.GetComponent<Popup>();
        TextPop scriptClone = UIRoot.Intance.InstantiateUIInCenter(view, script._layer, script._SetAnchor).GetComponent<TextPop>();
        return scriptClone;
    }

    protected Popup InitHeroDetails()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("HeroDetailsPop");
        Popup script = view.GetComponent<Popup>();
        HeroDetailsPop scriptClone = UIRoot.Intance.InstantiateUIInCenter(view, script._layer, script._SetAnchor).GetComponent<HeroDetailsPop>();
        return scriptClone;
    }

    protected Popup InitSkill()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("SkillDetailsPop");
        Popup script = view.GetComponent<Popup>();
        SkillDetalisPop scriptClone = UIRoot.Intance.InstantiateUIInCenter(view, script._layer, script._SetAnchor).GetComponent<SkillDetalisPop>();
        return scriptClone;
    }


}//end class
