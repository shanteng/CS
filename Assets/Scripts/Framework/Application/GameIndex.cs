

using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System;

public class GameIndex : MonoBehaviour
{
    public static bool InWorld = false;
    public static bool InBattle = false;
    public static string UID = "";
    private static long _serverTime;
    private static long _serverTimeOnSync;
    private static float _gameTimeOnServertime;
    public static long ServerTime => _serverTime;
    public static int COL = 1000;
    public static int ROW = 1000;


    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        ApplicationFacade.instance = GameFacade.instance;
        this.InitMvc();
        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame()
    {
        TimeSpan nowStep = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        _serverTimeOnSync =  Convert.ToInt64(nowStep.TotalSeconds);
        _gameTimeOnServertime = Time.realtimeSinceStartup;


        ShowLogin();
        yield return 0;
    }

    public static void UpdateServerTime()
    {
        _serverTime = _serverTimeOnSync + Mathf.FloorToInt(Time.realtimeSinceStartup - _gameTimeOnServertime);
    }

    private void Update()
    {
        UpdateServerTime();
    }

    private  void ShowLogin()
    {
        //MediatorUtil.ShowMediator(MediatorDefine.LOGIN);
    }

    private void InitMvc()
    {
        ApplicationFacade.instance.RegisterCommand(NotiDefine.APP_START_UP, new StartupCommand());
        ApplicationFacade.instance.SendNotification(NotiDefine.APP_START_UP);
        ApplicationFacade.instance.SendNotification(NotiDefine.MVC_STARTED);
    }
}