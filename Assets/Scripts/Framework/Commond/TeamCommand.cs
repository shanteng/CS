﻿using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections.Generic;

public class TeamCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        TeamProxy proxy = Facade.RetrieveProxy(ProxyNameDefine.TEAM) as TeamProxy;
        switch (notification.Name)
        {
            case NotiDefine.SetTeamHeroDo:
                {
                    proxy.SetTeamHero(notification.Body as Dictionary<string, object>);
                    break;
                }
        }
    }//end func
}