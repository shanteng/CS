using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//所有滑动列表单项的基类
public class UIPress : UIBase
    ,IPointerDownHandler
     , IPointerUpHandler
{
    private bool _isPress = false;
    public float _notiDelta = 0.01f;
    private float _timeDelta = 0;
    private UnityAction _callBackFun;
    public void AddEvent(UnityAction callBack)
    {
        this._callBackFun = callBack;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPress = true;
        this._timeDelta = 0;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPress = false;
        this._timeDelta = 0;
    }

    void Update()
    {
        if (_isPress)
        {
            this._timeDelta += Time.deltaTime;
            if (this._timeDelta >= this._notiDelta && this._callBackFun != null)
            {
                this._timeDelta = 0;
                _callBackFun.Invoke();
            }
        }
    }
}


