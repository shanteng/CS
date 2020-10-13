using System;
using System.Collections.Generic;
using SMVC.Interfaces;
using SMVC.Patterns;

public class ModelsInitializeCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        Facade.RegisterProxy(new TimeCenterProxy());
        Facade.RegisterProxy(new WorldProxy());
    }
}