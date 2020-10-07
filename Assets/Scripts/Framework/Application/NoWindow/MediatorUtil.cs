
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;

public class MediatorUtil
{
    public static void ShowMediator(string mediatorName)
    {
        var noti = $"{NotiDefine.WINDOW_DO_SHOW}_{mediatorName}";
        SendNotification(noti);
    }

    public static void HideMediator(string mediatorName)
    {
        var noti = $"{NotiDefine.WINDOW_DO_HIDE}_{mediatorName}";
        SendNotification(noti);
    }

    public static void SendNotification(string notify, object obj = null)
    {
        ApplicationFacade.instance.SendNotification(notify, obj);
    }
}