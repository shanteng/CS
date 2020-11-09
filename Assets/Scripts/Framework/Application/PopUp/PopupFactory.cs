using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum PopType
{
    COMFIRM,
    BUILDING,
    NOTICE,
    BUILDING_UPGRADE,
    BUILDING_LEVEL_EFFECT,
    CAREER_RATE,
    ATTRADD,
};

public class PopupFactory : SingletonFactory<PopupFactory>
{
    private Popup _curShowWin = null;

    public void Hide()
    {
        if (this._curShowWin != null && this._curShowWin.gameObject != null)
        {
            GameObject.Destroy(this._curShowWin.gameObject);
            this._curShowWin = null;
        }
        
    }//end func

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

    public void ShowCareerRate()
    {
        this.ShowPop(PopType.CAREER_RATE,0);
    }

    public void ShowBuildingInfo(string bdKey)
    {
        this.ShowPop(PopType.BUILDING, bdKey);
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
        this.ShowPop(PopType.NOTICE, kv);
    }

    public void ShowAttrAdd(AttrAddData data)
    {
        this.ShowPop(PopType.ATTRADD, data);
    }

    public void ShowErrorNotice(string errorCode, params object[] paramName)
    {
        string notice =  LanErrorConfig.GetLanguage(errorCode,paramName);
        this.ShowNotice(notice);
        MediatorUtil.SendNotification(NotiDefine.ErrorCode, errorCode);
    }

    private void ShowPop(PopType type, object content)
    {
        if (this._curShowWin != null && this._curShowWin._DestorySecs == 0)
        {
            this.Hide();
        }
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
            case PopType.NOTICE:
                {
                    _curShowWin = InitNotice();
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
            case PopType.ATTRADD:
                {
                    _curShowWin = InitAttrAdd();
                    break;
                }
        }

        this._curShowWin.setContent(content);
        if (this._curShowWin._DestorySecs == 0)
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
}//end class
