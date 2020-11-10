using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//所有滑动列表单项的基类
public class UIClickHandler : UIBase
    ,IPointerClickHandler
{
    private UnityAction<object> _listener;
    private object _param;

    public void AddListener(UnityAction<object> ls, object param = null)
    {
        this._listener = ls;
        this._param = param;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (this._listener != null)
            this._listener.Invoke(this._param);
    }
}


