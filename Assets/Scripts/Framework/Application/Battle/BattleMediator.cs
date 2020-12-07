
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class BattleMediator : BaseWindowMediator<BattleView>
{
    public BattleMediator() : base(MediatorDefine.BATTLE, WindowLayer.Main)
    {
        
    }

   
    protected override void InitListNotificationInterestsInner()
    {
        m_HideNoHandleNotifations.Add(NotiDefine.BattleBornUpdate);
        m_HideNoHandleNotifations.Add(NotiDefine.PreBattleStartNoti);
        m_HideNoHandleNotifations.Add(NotiDefine.BattleStartNoti);
        m_HideNoHandleNotifations.Add(NotiDefine.BattleStateChangeNoti);
        m_HideNoHandleNotifations.Add(NotiDefine.ShowSureFight);
        m_HideNoHandleNotifations.Add(NotiDefine.AttackPlayerEndJudge);
        m_HideNoHandleNotifations.Add(NotiDefine.BattleAiEnd);
        m_HideNoHandleNotifations.Add(NotiDefine.BattleEffectChange);
    }

    protected override void HandheldNotificationInner(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.BattleBornUpdate:
                {
                    this.m_view._preUi.UpdateList();
                    this.m_view.SetUpPlayerList();
                    break;
                }
            case NotiDefine.PreBattleStartNoti:
                {
                    this.m_view.OnPreFight();
                    break;
                }
            case NotiDefine.BattleStartNoti:
                {
                    this.m_view.OnStartFight();
                    break;
                }
            case NotiDefine.BattleStateChangeNoti:
                {
                    this.m_view.OnStateChange((BattleStatus)notification.Body);
                    break;
                }
            case NotiDefine.ShowSureFight:
                {
                    this.m_view.ShowAttackSure();
                    break;
                }
            case NotiDefine.AttackPlayerEndJudge:
                {
                    this.m_view.OnAttackEnd();
                    break;
                }
            case NotiDefine.BattleAiEnd:
                {
                    this.m_view.OnAiEnd();
                    break;
                }
            case NotiDefine.BattleEffectChange:
                {
                    this.m_view.UpdatePlayerBuffs();
                    this.m_view.UpdateActionPlayerBuff();
                    break;
                }
        }
    }//end func

    protected override void InitViewComponent(GameObject view)
    {

    }

    protected override void DoInitializeInner()
    {
        this.m_view.Init();
    }

}//end class