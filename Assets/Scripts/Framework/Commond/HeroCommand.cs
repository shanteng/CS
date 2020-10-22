using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections.Generic;

public class HeroCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        HeroProxy proxy = Facade.RetrieveProxy(ProxyNameDefine.HERO) as HeroProxy;
        switch (notification.Name)
        {
            case NotiDefine.LoadAllHeroDo:
                {
                    proxy.LoadAllHeros();
                    break;
                }
            case NotiDefine.ChangeHeroBelongDo:
                {
                    proxy.ChangeHeroBelong(notification.Body as Dictionary<string, object>);
                    break;
                }
        }
    }//end func
}