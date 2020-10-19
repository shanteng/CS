using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections.Generic;

public class RoleCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        RoleProxy proxy = Facade.RetrieveProxy(ProxyNameDefine.ROLE) as RoleProxy;
        switch (notification.Name)
        {
            case NotiDefine.LoadRoleDo:
                {
                    proxy.LoadOrGenerateRole();
                    break;
                }
            case NotiDefine.InitOutComeDo:
                {
                    proxy.InitBuildingEffect();
                    break;
                }
            case NotiDefine.BuildingStatusChanged:
                {
                    proxy.UpdateBuildingEffectComeBy((string)notification.Body);
                    break;
                }
            case NotiDefine.AcceptHourAwardDo:
                {
                    proxy.AcceptHourAward((string)notification.Body);
                    break;
                }
        }
    }//end func
}