﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

///所有滑动列表单项数据的基类
public class ScrollData
{
    public string _Key = "";
    public object _Param;

}

//所有滑动列表单项的基类
public class ItemRender : UIBase,IPointerClickHandler
{
    [HideInInspector]
    public DataGrid _view;
    public ScrollData m_renderData;
    public void SetData(ScrollData data)
    {
        m_renderData = data;
        this.setDataInner(data);
    }

    protected virtual void setDataInner(ScrollData data)
    {

    }

    public void updateData()
    {
        if (this.m_renderData == null || this.gameObject.activeSelf == false)
            return;
        this.setDataInner(this.m_renderData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        this._view.CallBackClick(this.gameObject);
    }
}


