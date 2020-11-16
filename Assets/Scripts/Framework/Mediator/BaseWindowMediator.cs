using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum WindowState
{
    UNINIT = 0,
    SHOW,
    HIDE,
    SHOWING_WAIT,
    HIDEING_WAIT,
}


public abstract class BaseWindowMediator<T> : Mediator
{
    protected object ShowData;
    protected WindowLayer m_eWindowLayer;
    protected WindowState m_eWindowState;
    protected List<string> m_lInterestNotifications;
    protected List<string> m_HideNoHandleNotifations;

    protected string m_sShowNoity;
    protected string m_sHideNoity;
    protected bool DestroyWhenHide = true;//默认关闭就销毁
    protected GameObject _viewObj;
    protected T m_view;
    protected string _prefabName = "";

    public bool windowVisible => (this.m_eWindowState == WindowState.SHOW);

    protected BaseWindowMediator(MediatorDefine mediatorName, WindowLayer layer)
    {
        this.m_mediatorName = MediatorUtil.GetName(mediatorName);
        this._prefabName = typeof(T).ToString();
        m_eWindowLayer = layer;
        m_eWindowState = WindowState.UNINIT;
       
        m_sShowNoity = $"{NotiDefine.WINDOW_DO_SHOW}_{mediatorName}";
        m_sHideNoity = $"{NotiDefine.WINDOW_DO_HIDE}_{mediatorName}";
    }

    string viewResource()
    {
        return this._prefabName;
    }

    public override IEnumerable<string> ListNotificationInterests
    {
        get
        {
            if (null == m_lInterestNotifications)
            {
                InitListNotificationInterests();
            }

            return m_lInterestNotifications;
        }
    }

    
    public void InitListNotificationInterests()
    {
        this.m_HideNoHandleNotifations = new List<string>();
        m_lInterestNotifications = new List<string>();
        m_lInterestNotifications.Add(m_sShowNoity);
        m_lInterestNotifications.Add(m_sHideNoity);
        m_lInterestNotifications.Add(NotiDefine.FULLSCREEN_WINDOW_SHOW);
        m_lInterestNotifications.Add(NotiDefine.WorldGoToStart);
        m_lInterestNotifications.Add(NotiDefine.GAME_RESET);
        InitListNotificationInterestsInner();
        m_lInterestNotifications.AddRange(this.m_HideNoHandleNotifations);
    }

    public override void HandleNotification(INotification notification)
    {
        if (notification.Name.Equals(m_sShowNoity))
        {
            ShowData = notification.Body;
            ShowWindow();
            return;
        }
        else if (notification.Name.Equals(m_sHideNoity))
        {
            HideWindow();
            return;
        }
        else
        {
            switch (notification.Name)
            {
                case NotiDefine.FULLSCREEN_WINDOW_SHOW:
                    {
                        OnFullScreenWindowShow(notification.Body);
                    }
                    break;
                case NotiDefine.WorldGoToStart:
                    {
                        if (WindowLayer.Window == this.m_eWindowLayer ||
                            WindowLayer.FullScreen == this.m_eWindowLayer)
                        {
                            //全屏界面打开时关闭所有已经打开过的窗口
                            HideWindow();
                        }
                    }
                    break;
                case NotiDefine.GAME_RESET:
                    {
                        this.DestroyWindow();
                    }
                    break;
            }
        }//end else


        if (this.m_HideNoHandleNotifations.Contains(notification.Name) && !this.windowVisible)
            return;
        this.HandheldNotificationInner(notification);

    }

    protected  virtual void HideWindow()
    {
        if (this.m_viewComponent == null)
            return;
        if ((m_eWindowState == WindowState.HIDE || m_eWindowState == WindowState.HIDEING_WAIT) && this._viewObj.activeSelf == false)
            return;

        m_eWindowState = WindowState.HIDE;
        this._viewObj.SetActive(false);
        this.hideWindowInner();

        if (this.DestroyWhenHide)
        {
            DestroyWindow();
        }
    }

    protected void DestroyWindow()
    {
        if (this.m_viewComponent == null)
            return;

        if (windowVisible)
            hideWindowInner();
        
        this.m_eWindowState = WindowState.UNINIT;
        Object.Destroy(m_viewComponent as Object);

        this.m_viewComponent = null;
        this._viewObj = null;
        DestroyWindowInner();
    }

    protected void ShowWindow()
    {
        if (WindowState.SHOWING_WAIT == m_eWindowState)
            return;
        m_eWindowState = WindowState.SHOWING_WAIT;
        if (null == m_viewComponent)
        {
            LoadViewComponent();
        }
        else
        {
            DoInitialize();
        }
        this.SendNotification(NotiDefine.WINDOW_HAS_SHOW, this.MediatorName);
        
    }

    public T GetView()
    {
        return this.m_view;
    }

    protected void LoadViewComponent()
    {
        GameObject obj = ResourcesManager.Instance.LoadUIRes(viewResource());
        if (null == obj)
            return;

        m_viewComponent = UIRoot.Intance.InstantiateUIInCenter(obj,this.m_eWindowLayer,true,false);
        this._viewObj = (GameObject)m_viewComponent;
        m_view = _viewObj.GetComponent<T>();


        InitViewComponent(_viewObj);
        DoInitialize();
    }

    protected virtual void DoInitialize()
    {
        this.SetAsLastSibling();
        if (WindowLayer.FullScreen == m_eWindowLayer)
            SendNotification(NotiDefine.FULLSCREEN_WINDOW_SHOW, this);
        m_eWindowState = WindowState.SHOW;
        this.DoInitializeInner();
    }

    public void SetAsLastSibling()
    {
         this._viewObj.transform.SetAsLastSibling();
    }

    protected virtual void OnFullScreenWindowShow(object windownMediator)
    {
        if (WindowLayer.Window == this.m_eWindowLayer)
        {
            //全屏界面打开时关闭所有已经打开过的窗口
            HideWindow();
        }
    }


    #region VirtualFunction
    protected virtual void InitListNotificationInterestsInner() { }
    protected virtual void HandheldNotificationInner(INotification notification) { }
  
    protected abstract void InitViewComponent(GameObject view);
    protected virtual void DoInitializeInner() { }

    protected virtual void hideWindowInner() { }

    protected virtual void DestroyWindowInner() { }
    #endregion
}//end class

