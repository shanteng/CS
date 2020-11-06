using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//所有滑动列表单项的基类
public class UIDrag : UIBase
    ,IBeginDragHandler
    , IDragHandler
    ,IEndDragHandler
{

    protected UnityAction<PointerEventData> _begin;
    protected UnityAction<PointerEventData> _drag;
    protected UnityAction<PointerEventData> _end;

    public void AddEvent(UnityAction<PointerEventData> begin, UnityAction<PointerEventData> drag, UnityAction<PointerEventData> end)
    {
        this._begin = begin;
        this._drag = drag;
        this._end = end;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (this._begin != null)
            this._begin.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (this._drag != null)
            this._drag.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (this._end != null)
            this._end.Invoke(eventData);
    }

}


