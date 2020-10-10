using System;
using System.Collections.Generic;
public abstract class BaseFullScreenWindowMediator<T> : BaseWindowMediator<T>
{
    
    protected BaseFullScreenWindowMediator(MediatorDefine mediatorName) : base(mediatorName, WindowLayer.FullScreen)
    {

    }

    protected override void DoInitialize()
    {
        UIRoot.CurFullWindow = this.m_mediatorName;
        SendNotification(NotiDefine.FULLSCREEN_WINDOW_SHOW, this);
        base.DoInitialize();
    }

    protected override void OnFullScreenWindowShow(object windownMediator)
    {
        if (windownMediator != this)
        {
            HideWindow();
        }
    }

    protected override void HideWindow()
    {
        if (UIRoot.CurFullWindow.Equals(m_mediatorName))
            UIRoot.CurFullWindow = string.Empty;
        base.HideWindow();
    }
}//end class