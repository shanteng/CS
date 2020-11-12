using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections.Generic;

public class BuildingCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        WorldProxy proxy = Facade.RetrieveProxy(ProxyNameDefine.WORLD) as WorldProxy;
        switch (notification.Name)
        {
            case NotiDefine.GenerateMySpotDo:
                {
                    proxy.GenerateAllBaseSpot((int)notification.Body);
                    break;
                }
            case NotiDefine.CreateOneBuildingDo:
                {
                    proxy.Create(notification.Body as Dictionary<string,object>);
                    break;
                }
            case NotiDefine.BuildingRelocateDo:
                {
                    proxy.Relocate(notification.Body as Dictionary<string, object>);
                    break;
                }
            case NotiDefine.UpgradeOneBuildingDo:
                {
                    proxy.Upgrade(notification.Body as string);
                    break;
                }
            case NotiDefine.BuildingCancelDo:
                {
                    proxy.CancelUpgrade(notification.Body as string);
                    break;
                }
            case NotiDefine.BuildingSpeedUpDo:
                {
                    proxy.SpeedUpUpgrade(notification.Body as string);
                    break;
                }
            case NotiDefine.BuildingExpireReachedNoti:
                {
                    proxy.OnBuildExpireFinsih(notification.Body as string);
                    break;
                }
            case NotiDefine.PatrolExpireReachedNoti:
                {
                    proxy.OnPatrolExpireFinsih(notification.Body as string);
                    break;
                }
            case NotiDefine.QuestCityExpireReachedNoti:
                {
                    proxy.OnFinishQuestCity(notification.Body as string);
                    break;
                }
            case NotiDefine.PatrolDo:
                {
                    proxy.DoPatrol(notification.Body as Dictionary<string, object>);
                    break;
                }
            case NotiDefine.QuestCityDo:
                {
                    proxy.DoQuestCity(notification.Body as Dictionary<string, object>);
                    break;
                }
        }
    }//end func
}