﻿using System;
using System.Collections.Generic;
using SMVC.Interfaces;
using SMVC.Patterns;

public class ViewsInitializeCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        Facade.RegisterMediator(new DataCenterMediator());
        Facade.RegisterMediator(new LoginMediator());
        Facade.RegisterMediator(new SceneLoaderMediator());
        Facade.RegisterMediator(new HomeLandMediator());
        Facade.RegisterMediator(new BuildCenterMediator());
        Facade.RegisterMediator(new MainMediator());
        Facade.RegisterMediator(new RecruitMediator());
    }
}