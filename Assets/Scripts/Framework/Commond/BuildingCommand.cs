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
            case NotiDefine.GenerateMyBuildingDo:
                {
                    proxy.GenerateAllBuilding((int)notification.Body);
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
            case NotiDefine.BuildingExpireReachedNoti:
                {
                    proxy.OnBuildExpireFinsih(notification.Body as string);
                    break;
                }
        }
    }//end func
}