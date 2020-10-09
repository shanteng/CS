using System;
using System.Collections.Generic;
using SMVC.Interfaces;
using SMVC.Patterns;

public class ControllersInitializeCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        //TimeCenterCommand
        Facade.RegisterCommand(NotiDefine.AddTimestepCallback, typeof(TimeCenterCommand));

        //BuildingCommand
        Facade.RegisterCommand(NotiDefine.CreateOneBuildingDo, typeof(BuildingCommand));
        Facade.RegisterCommand(NotiDefine.BuildingRelocateDo, typeof(BuildingCommand));
    }
}