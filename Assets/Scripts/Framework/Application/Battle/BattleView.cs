using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleView : MonoBehaviour
{
    public UIButton _btnQuit;
    public GameObject _FightAni;
    public PreBattleUi _preUi;
    public GameObject _WaitLine;
    public SpeedPlayerUi _waitPlayerTemplete;
    private Dictionary<int, SpeedPlayerUi> _AliveWaitPlayerDic;
    private Dictionary<BattlePlace, BattleInfoUi> _InfoUiDic;
    void Awake()
    {
        this._FightAni.SetActive(false);
        this._btnQuit.AddEvent(OnQuit);
        this._waitPlayerTemplete.gameObject.SetActive(false);
    }

    private void OnQuit(UIButton btn)
    {
        MediatorUtil.HideMediator(MediatorDefine.BATTLE);
        MediatorUtil.SendNotification(NotiDefine.DoLoadScene, SceneDefine.Home);
    }

    public void Init()
    {
        this._preUi.gameObject.SetActive(true);
        this._preUi.SetList();
        this._FightAni.SetActive(false);
        this._WaitLine.SetActive(false);

        this._InfoUiDic = new Dictionary<BattlePlace, BattleInfoUi>();
        BattleInfoUi ui = this.transform.Find("Top/Attack").GetComponent<BattleInfoUi>();
        this._InfoUiDic[BattlePlace.Attack] = ui;

        ui = this.transform.Find("Top/Defense").GetComponent<BattleInfoUi>();
        this._InfoUiDic[BattlePlace.Defense] = ui;
        this.SetUpPlayerList();
    }

    public void SetUpPlayerList()
    {
        BattleData data = BattleProxy._instance.Data;
        List<int> npcteam = new List<int>();
        List<int> myteam = new List<int>();
        foreach (BattlePlayer pl in data.Players.Values)
        {
            if (pl.BornIndex == 0)
                continue;
            if (pl.TeamID > 0)
                myteam.Add(pl.TeamID);
            else
                npcteam.Add(pl.TeamID);
        }

        if (data.MyPlace == BattlePlace.Attack)
        {
            this._InfoUiDic[BattlePlace.Attack].SetList(myteam);
            this._InfoUiDic[BattlePlace.Defense].SetList(npcteam);
        }
        else
        {
            this._InfoUiDic[BattlePlace.Attack].SetList(npcteam);
            this._InfoUiDic[BattlePlace.Defense].SetList(myteam);
        }
    }//end func

    public void OnStartFight()
    {
        this._preUi.gameObject.SetActive(false);
        this._FightAni.SetActive(true);
        this._WaitLine.SetActive(true);
        this.InitWaitPlayer();
        CoroutineUtil.GetInstance().WaitTime(3f, true, WaitInitEnd);
    }

    private void InitWaitPlayer()
    {
        this._AliveWaitPlayerDic = new Dictionary<int, SpeedPlayerUi>();
        foreach (BattlePlayer pl in BattleProxy._instance.Data.Players.Values)
        {
            if (pl.BornIndex == 0)
                continue;
            SpeedPlayerUi waitP = GameObject.Instantiate<SpeedPlayerUi>(this._waitPlayerTemplete, this._WaitLine.transform, false);
            waitP.transform.localScale = Vector3.one;
            waitP.SetData(pl.TeamID);
            waitP.gameObject.SetActive(true);
            this._AliveWaitPlayerDic.Add(pl.TeamID, waitP);
        }
    }

    private void WaitInitEnd(object[] param)
    {
        foreach (SpeedPlayerUi pl in this._AliveWaitPlayerDic.Values)
        {
            pl.StartWaitMove();
        }
    }

    private void PlayerDoAction()
    {
        foreach (SpeedPlayerUi pl in this._AliveWaitPlayerDic.Values)
        {
            pl.SetMove(false);
        }
    }

    public void OnStateChange(BattleStatus state)
    {
        if (state == BattleStatus.Action)
        {
            this.PlayerDoAction();
        }
    }
}
