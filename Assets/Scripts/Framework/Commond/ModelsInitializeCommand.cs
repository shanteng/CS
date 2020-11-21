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
        Facade.RegisterProxy(new RoleProxy());
        Facade.RegisterProxy(new HeroProxy());
        Facade.RegisterProxy(new ArmyProxy());
        Facade.RegisterProxy(new TeamProxy());
        Facade.RegisterProxy(new PathProxy());
        Facade.RegisterProxy(new BattleProxy());
    }
}