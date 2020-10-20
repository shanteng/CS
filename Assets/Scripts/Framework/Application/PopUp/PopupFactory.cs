using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum PopType
{
    COMFIRM,
    BUILDING,
};

public class PopupFactory : SingletonFactory<PopupFactory>
{
    private Popup _curShowWin = null;

    public void Hide()
    {
        if (this._curShowWin != null)
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

    private void ShowPop(PopType type, object content)
    {
        this.Hide();
        this._curShowWin = GetPopUi(type);
        this._curShowWin.setContent(content);
        if (this._curShowWin._ShowInCenter)
        {
            UIRoot.Intance.ShowUIInCenter(this._curShowWin.gameObject, this._curShowWin._layer);
        }
    }

    private Popup GetPopUi(PopType type)
    {
        Popup ui = null;
        switch (type)
        {
            case PopType.COMFIRM:
                {
                    ui = InitConfirm();
                    break;
                }
            case PopType.BUILDING:
                {
                    ui = InitBuilding();
                    break;
                }
        }
        return ui;
    }//end func

    protected Popup InitConfirm()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("ConfirmPop");
        ConfirmPop script = GameObject.Instantiate(view).GetComponent<ConfirmPop>();
        return script;
    }

    protected Popup InitBuilding()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("BuildingInfoPop");
        BuildingInfoPop script = GameObject.Instantiate(view).GetComponent<BuildingInfoPop>();
        return script;
    }
}//end class
