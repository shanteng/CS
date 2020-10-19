using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class BuildCanvas : UIBase
{
    public RectTransform _layoutRect;
    public UIButton _btnSure;
    public UIButton _btnCancel;
    

    private void Start()
    {
        _btnSure.AddEvent(this.OnClick);
        _btnCancel.AddEvent(this.OnClick);
    }

    public void SetState(bool canBuild)
    {
        this._btnSure.IsEnable = canBuild;
    }

    private void OnClick(UIButton btn)
    {
        bool isConfirm = btn.Equals(this._btnSure);
        MediatorUtil.SendNotification(NotiDefine.ConfirmBuild, isConfirm);
    }

}
