using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;


//所有滑动列表单项的基类
public class UIMediatorHideHandler : MonoBehaviour
    ,IPointerClickHandler
{
    public MediatorDefine _MediatorName;
    public void OnPointerClick(PointerEventData eventData)
    {
        MediatorUtil.HideMediator(this._MediatorName);
    }
}


