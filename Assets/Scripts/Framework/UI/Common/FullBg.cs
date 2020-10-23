using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//所有滑动列表单项的基类
public class FullBg : UIBase
{
    public UIButton _btnClose;
    public Text _TitleTxt;
    public MediatorDefine _mediator;
    private void Awake()
    {
        this._btnClose.AddEvent(HideMediator);
    }

    private void HideMediator(UIButton btn)
    {
        MediatorUtil.HideMediator(this._mediator);
    }
}


