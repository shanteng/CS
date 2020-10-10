﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public delegate void ScreenClickHideDelegate();
//所有滑动列表单项的基类
public class UIScreenHideHandler : UIBase
    ,IPointerClickHandler
{
    private ScreenClickHideDelegate _listener;

    public void AddListener(ScreenClickHideDelegate ls)
    {
        this._listener = ls;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (this._listener != null)
            this._listener.Invoke();
        else
            this.Hide();
    }
}

