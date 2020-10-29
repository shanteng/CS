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
        Facade.RegisterCommand(NotiDefine.RemoveTimestepCallback, typeof(TimeCenterCommand));

        //BuildingCommand
        Facade.RegisterCommand(NotiDefine.GenerateMySpotDo, typeof(BuildingCommand));
    
        Facade.RegisterCommand(NotiDefine.CreateOneBuildingDo, typeof(BuildingCommand));
        Facade.RegisterCommand(NotiDefine.BuildingRelocateDo, typeof(BuildingCommand));
        Facade.RegisterCommand(NotiDefine.BuildingCancelDo, typeof(BuildingCommand));
        Facade.RegisterCommand(NotiDefine.UpgradeOneBuildingDo, typeof(BuildingCommand));
        Facade.RegisterCommand(NotiDefine.BuildingSpeedUpDo, typeof(BuildingCommand));

        Facade.RegisterCommand(NotiDefine.BuildingExpireReachedNoti, typeof(BuildingCommand));
        //RoleCommand
        Facade.RegisterCommand(NotiDefine.LoadRoleDo, typeof(RoleCommand));
        Facade.RegisterCommand(NotiDefine.AcceptHourAwardDo, typeof(RoleCommand));
    
        //HeroCommand
        Facade.RegisterCommand(NotiDefine.LoadAllHeroDo, typeof(HeroCommand));
        Facade.RegisterCommand(NotiDefine.ChangeHeroBelongDo, typeof(HeroCommand));

        //ArmyCommand
        Facade.RegisterCommand(NotiDefine.LoadAllArmyDo, typeof(ArmyCommand));
        Facade.RegisterCommand(NotiDefine.ArmyRecruitExpireReachedNoti, typeof(ArmyCommand));
        Facade.RegisterCommand(NotiDefine.RecruitArmyDo, typeof(ArmyCommand));
    }
}