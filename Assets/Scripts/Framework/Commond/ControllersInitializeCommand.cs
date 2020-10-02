using System;
using System.Collections.Generic;
using SMVC.Interfaces;
using SMVC.Patterns;

public class ControllersInitializeCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        //Facade.RegisterCommand(ApplicationNotificationCenter.MVC_STARTED, typeof(LocalDataCommand));
    }
}