﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

///所有滑动列表单项数据的基类
public class ScrollData
{
    public string _Key = "";
    public object _Param;
    public bool _IsSelect = false;
}

//所有滑动列表单项的基类
[RequireComponent(typeof(NonDrawingGraphic))]
[RequireComponent(typeof(LayoutElement))]
public class ItemRender : UIBase, IPointerClickHandler
{
    public IScollItemClickListener _listener;
    public ScrollData m_renderData;
    public GameObject _Select;

    public void SetSelectState()
    {
        if (this._Select != null)
            this._Select.SetActive(this.m_renderData._IsSelect);
    }

    public void SetData(ScrollData data)
    {
        m_renderData = data;
        this.setDataInner(data);
        this.SetSelectState();
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
        if (this.m_renderData != null && this._listener != null)
        {
            this.transform.DOKill();
            this.transform.localScale = Vector3.one;
            this.transform.DOPunchScale(new Vector3(-0.05f, -0.05f, -0.05f), 0.1f, 2, 0);
            this._listener.onClickScrollItem(this.m_renderData);
        }
            
    }
}


