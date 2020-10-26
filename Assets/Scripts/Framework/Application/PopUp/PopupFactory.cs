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

    public void ShowBuildingInfo(string bdKey)
    {
        this.ShowPop(PopType.BUILDING, bdKey);
    }

    public void ShowBuildingUpgrade(string bdKey)
    {
        this.ShowPop(PopType.BUILDING_UPGRADE, bdKey);
    }

    public void ShowNotice(string notice)
    {
        this.ShowPop(PopType.NOTICE, notice);
    }

    public void ShowErrorNotice(string errorCode, params object[] paramName)
    {
        string notice =  LanErrorConfig.GetLanguage(errorCode,paramName);
        this.ShowNotice(notice);
    }

    private void ShowPop(PopType type, object content)
    {
        this._curShowWin = null;
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
        }

        this._curShowWin.setContent(content);
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

    protected Popup InitNotice()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("NoticePop");
        Popup script = view.GetComponent<Popup>();

        NoticePop scriptClone = UIRoot.Intance.InstantiateUIInCenter(view, script._layer, script._SetAnchor).GetComponent<NoticePop>();
        GameObject.Destroy(scriptClone.gameObject, 1.5f);
        return scriptClone;
    }
}//end class
