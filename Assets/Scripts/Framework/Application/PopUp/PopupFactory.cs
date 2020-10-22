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
            if (this._cor != null)
            {
                CoroutineUtil.GetInstance().Stop(this._cor);
                this._cor = null;
            }
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

    public void ShowNotice(string notice)
    {
        this.ShowPop(PopType.NOTICE, notice);
    }

    public void ShowErrorNotice(string errorCode, params object[] paramName)
    {
        string notice =  LanErrorConfig.GetLanguage(errorCode,paramName);
        this.ShowNotice(notice);
    }

    private Coroutine _cor;
    private void ShowPop(PopType type, object content)
    {
        this.Hide();
        this._curShowWin = GetPopUi(type);
        this._curShowWin.setContent(content);
        UIRoot.Intance.ShowUIInCenter(this._curShowWin.gameObject, this._curShowWin._layer, this._curShowWin._ShowInCenter);
        if (this._curShowWin._DestorySecs > 0)
            _cor = CoroutineUtil.GetInstance().WaitTime(this._curShowWin._DestorySecs, true, WaitDestory);
    }

    private void WaitDestory(object[] param)
    {
        if (this._curShowWin != null)
            this.Hide();
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
            case PopType.NOTICE:
                {
                    ui = InitNotice();
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

    protected Popup InitNotice()
    {
        GameObject view = ResourcesManager.Instance.LoadPopupRes("NoticePop");
        NoticePop script = GameObject.Instantiate(view).GetComponent<NoticePop>();
        return script;
    }
}//end class
