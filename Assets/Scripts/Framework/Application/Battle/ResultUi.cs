using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ResultUi : UIBase
{
    public UIScreenHideHandler _click;
    public GameObject Win;
    public GameObject Lose;

    void Awake()
    {
        _click.AddListener(this.OnBgClick);
    }

    private void OnBgClick()
    {
        //打开结算详情面板，临时退
        MediatorUtil.HideMediator(MediatorDefine.BATTLE);
        MediatorUtil.SendNotification(NotiDefine.DoLoadScene, SceneDefine.Home);
    }

    public void SetData(bool isWin)
    {
        this.Win.SetActive(isWin);
        this.Lose.SetActive(isWin == false);
    }

}
