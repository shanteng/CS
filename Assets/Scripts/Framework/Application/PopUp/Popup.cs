using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Popup : UIBase
{
    public UIButton _BtnClose;
    private UIScreenHideHandler _ClickHide;
    void Awake()
    {
        if (this._BtnClose != null)
        {
            _BtnClose.AddEvent(this.OnHide);
        }

        Transform click = this.transform.Find("BlurBg");
        if (click != null)
        {
            _ClickHide = click.GetComponent<UIScreenHideHandler>();
            _ClickHide.AddListener(this.HidePop);
        }

        click = this.transform.Find("BlurBg");
        if (click != null)
        {
            _ClickHide = click.GetComponent<UIScreenHideHandler>();
            _ClickHide.AddListener(this.HidePop);
        }
    }

    private void OnHide(UIButton btn)
    {
        this.HidePop();
    }

    private void HidePop()
    {
        //
    }

    public virtual void setContent(object content)
    {

    }
}
