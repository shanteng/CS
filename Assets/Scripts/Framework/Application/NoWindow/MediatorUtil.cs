
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using System;

public class MediatorUtil
{
    public static void ShowMediator(MediatorDefine mediatorName,object param=null)
    {
        string name = MediatorUtil.GetName(mediatorName);
        var noti = $"{NotiDefine.WINDOW_DO_SHOW}_{name}";
        SendNotification(noti, param);
    }

    public static void HideMediator(MediatorDefine mediatorName)
    {
        string name = MediatorUtil.GetName(mediatorName);
        var noti = $"{NotiDefine.WINDOW_DO_HIDE}_{name}";
        SendNotification(noti);
    }

    public static void SendNotification(string notify, object obj = null)
    {
        ApplicationFacade.instance.SendNotification(notify, obj);
    }

    public static string GetName(MediatorDefine me)
    {
       return System.Enum.GetName(typeof(MediatorDefine), me);
    }
    
}