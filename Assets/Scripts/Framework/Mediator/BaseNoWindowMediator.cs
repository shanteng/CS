using SMVC.Patterns;
using System;
using System.Collections.Generic;
public abstract class BaseNoWindowMediator : Mediator
{
    protected BaseNoWindowMediator(MediatorDefine mediatorName)
    {
        this.m_mediatorName = MediatorUtil.GetName(mediatorName);
    }

    protected List<string> m_lInterestNotifications;

    public override IEnumerable<string> ListNotificationInterests
    {
        get
        {
            if (null == m_lInterestNotifications)
            {
                InitListNotificationInterests();
            }
            return m_lInterestNotifications;
        }
    }


    public void InitListNotificationInterests()
    {
        m_lInterestNotifications = new List<string>();

        InitListNotificationInterestsInner();
    }

    protected virtual void InitListNotificationInterestsInner()
    {

    }
}//end class