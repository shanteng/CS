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
            case NotiDefine.EnterGameDo:
                {
                    proxy.DoEnterGame();
                    break;
                }
            case NotiDefine.CreateRoleDo:
                {
                    proxy.DoCreateRole(notification.Body as Dictionary<string, object>);
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