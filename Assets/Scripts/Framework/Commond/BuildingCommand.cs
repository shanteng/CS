using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections.Generic;

public class BuildingCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        BuildingProxy proxy = Facade.RetrieveProxy(ProxyNameDefine.BUILDING) as BuildingProxy;
        switch (notification.Name)
        {
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
        }
    }//end func
}