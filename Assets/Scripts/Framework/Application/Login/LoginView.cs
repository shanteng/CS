﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginView : MonoBehaviour
{
    public UIButton _BtnStart;
    public UIButton _BtnClear;
    public UIButton _BtnReloadSkill;

    void Start()
    {
        _BtnStart.AddEvent(this.OnClickStart);
        _BtnClear.AddEvent(this.OnClear);
        _BtnReloadSkill.AddEvent(this.OnReloadSkill);
    }

    private void OnClickStart(UIButton btn)
    {
        MediatorUtil.SendNotification(NotiDefine.EnterGameDo);
    }

    private void OnClear(UIButton btn)
    {
        SdkView.Intance.WipeOut();
    }

    private void OnReloadSkill(UIButton btn)
    {
        HeroProxy._instance.ReloadAllHerosSkills();
    }
}
