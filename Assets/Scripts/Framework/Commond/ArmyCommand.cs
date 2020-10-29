using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections.Generic;

public class ArmyCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        ArmyProxy proxy = Facade.RetrieveProxy(ProxyNameDefine.ARMY) as ArmyProxy;
        switch (notification.Name)
        {
            case NotiDefine.LoadAllArmyDo:
                {
                    proxy.LoadAllArmys();
                    break;
                }
            case NotiDefine.ArmyRecruitExpireReachedNoti:
                {
                    proxy.OnRecruitExpireFinish((int)notification.Body);
                    break;
                }
            case NotiDefine.RecruitArmyDo:
                {
                    proxy.RecruitArmy((Dictionary<string,object>)notification.Body);
                    break;
                }
        }
    }//end func
}