using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class GmAdd : UIBase
    , IPointerClickHandler
{
    public string key;

    public void OnPointerClick(PointerEventData eventData)
    {
//#if UNITY_EDITOR
        RoleProxy._instance.GMAddValue(this.key);
//#endif
    }

}
