

using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

public class GameIndex : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        ApplicationFacade.instance = GameFacade.instance;
        this.InitMvc();
        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame()
    {
        ShowLogin();
        yield return 0;
    }

   
    private  void ShowLogin()
    {
        MediatorUtil.ShowMediator(MediatorDefine.LOGIN);
    }

    private void InitMvc()
    {
        ApplicationFacade.instance.RegisterCommand(NotiDefine.APP_START_UP, new StartupCommand());
        ApplicationFacade.instance.SendNotification(NotiDefine.APP_START_UP);
        ApplicationFacade.instance.SendNotification(NotiDefine.MVC_STARTED);
    }
}