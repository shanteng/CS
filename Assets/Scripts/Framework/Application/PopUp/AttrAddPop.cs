using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class AttrAddData
{
    public Vector2 uiPos;
    public string Key;
}

public class AttrAddPop : Popup
{
    public Image _icon;
 
    public override void setContent(object data)
    {
        AttrAddData kv = (AttrAddData)data;

        this._icon.sprite = ResourcesManager.Instance.GetCommonSprite(kv.Key);
        this._icon.SetNativeSize();
        this.transform.localPosition = kv.uiPos;
        Vector2 uiPos = MainView.GetInstance().GetIncomePostion(kv.Key);
        this.transform.DOLocalMove(uiPos, 0.6f).onComplete = () =>
        {
            MediatorUtil.SendNotification(NotiDefine.AcceptHourAwardDo, kv.Key);
        };
    }//end func
}//end class
