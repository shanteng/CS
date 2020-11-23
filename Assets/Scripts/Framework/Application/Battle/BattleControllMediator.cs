
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BattleControllMediator : BaseNoWindowMediator
{
    BattleController _battleController;
    private bool _isLoaded = false;
    private string _loadName = "";
    public BattleControllMediator() : base(MediatorDefine.BATTLE_CONTROL)
    {
    }

    protected override void InitListNotificationInterestsInner()
    {
        m_lInterestNotifications.Add(NotiDefine.LoadSceneFinish);
        m_lInterestNotifications.Add(NotiDefine.EnterBattleSuccess);
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotiDefine.EnterBattleSuccess:
                {
                    this._isLoaded = false;
                    int id = (int)notification.Body;
                    BattleSceneConfig config = BattleSceneConfig.Instance.GetData(id);
                    this._loadName = config.Scene;
                    this.SendNotification(NotiDefine.DoLoadScene, _loadName);
                    break;
                }
            case NotiDefine.LoadSceneFinish:
                {
                    string name = (string)notification.Body;
                    if (name.Equals(_loadName))
                    {
                        this.InitScene();
                    }
                    break;
                }
        }
    }//end 

  

    void InitScene()
    {
        //UIRoot.Intance.SetHomeSceneEnable(false);
        GameObject[] allObj = SceneManager.GetSceneByName(_loadName).GetRootGameObjects();
        _battleController = null;
        foreach (GameObject obj in allObj)
        {
            if (obj.name.Equals("BattleController"))
            {
                _battleController = obj.GetComponent<BattleController>();
                break;
            }
        }

        MediatorUtil.ShowMediator(MediatorDefine.BATTLE);
        this._battleController.InitScene();
        this._isLoaded = true;
    }
}//end class