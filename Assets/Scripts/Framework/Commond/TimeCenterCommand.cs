using SMVC.Interfaces;
using SMVC.Patterns;


public class TimeCenterCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        TimeCenterProxy proxy = Facade.RetrieveProxy(ProxyNameDefine.TIME_CENTER) as TimeCenterProxy;
        switch (notification.Name)
        {
            case NotiDefine.AddTimestepCallback:
                {
                    proxy.AddCallBack(notification.Body as TimeCallData);
                    break;
                }
            case NotiDefine.RemoveTimestepCallback:
                {
                    proxy.RemoveCallBack(notification.Body as string);
                    break;
                }
        }
    }//end func
}