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
        Facade.RegisterMediator(new ArmyMediator());
        Facade.RegisterMediator(new CreateMediator());
        Facade.RegisterMediator(new HeroMediator());
        Facade.RegisterMediator(new GameLogMediator());
        Facade.RegisterMediator(new TeamMediator());
        Facade.RegisterMediator(new SetTeamHeroMediator());
        Facade.RegisterMediator(new TeamAttackMediator());
        Facade.RegisterMediator(new BattleControllMediator());
        Facade.RegisterMediator(new BattleMediator());
        Facade.RegisterMediator(new CityAttackGroupMediator());
        Facade.RegisterMediator(new QuestionMediator());
        Facade.RegisterMediator(new LotteryMediator());
    }
}