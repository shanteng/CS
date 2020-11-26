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

    public GameObject _ActionCon;
    public UIButton _btnFight;
    public UIButton _btnEndRound;
    public UIButton _btnCancelFight;
    public UIButton _btnSureFight;

    public ResultUi _resultUi;
    private Dictionary<int, SpeedPlayerUi> _AliveWaitPlayerDic;
    private Dictionary<BattlePlace, BattleInfoUi> _InfoUiDic;

    void Awake()
    {
        this._FightAni.SetActive(false);
        this._btnQuit.AddEvent(OnQuit);

        this._btnFight.AddEvent(OnFight);
        this._btnEndRound.AddEvent(OnEndRound);
        this._btnCancelFight.AddEvent(OnCancelFight);
        this._btnSureFight.AddEvent(OnSureFight);

        this._waitPlayerTemplete.gameObject.SetActive(false);
    }

    private void OnFight(UIButton btn)
    {
        this._btnFight.Hide();
        this._btnCancelFight.Show();
        BattleController.Instance.SetAttackRange(0);
    }

    private void OnEndRound(UIButton btn)
    {
        BattleProxy._instance.DoNextRound();
    }

    private void OnCancelFight(UIButton btn)
    {
        this._btnFight.Show();
        this._btnCancelFight.Hide();
        this._btnSureFight.Hide();
        BattleController.Instance.BackToMoveState();
    }

    private void OnSureFight(UIButton btn)
    {
        BattleController.Instance.DoAttack();
        this._btnSureFight.Hide();
        this._btnCancelFight.Hide();
        this._btnEndRound.Hide();
    }

    public void ShowAttackSure()
    {
        this._btnSureFight.Show();
    }

    public void OnAttackEnd()
    {
        //更新列表状态和血量
        foreach (BattleInfoUi ui in this._InfoUiDic.Values)
        {
            ui.UpdateBlood();
            ui.UpdateList();
        }

        //删除速度进度条上的玩家
        List<int> rms = new List<int>();
        foreach (SpeedPlayerUi pl in this._AliveWaitPlayerDic.Values)
        {
            BattlePlayer data = BattleProxy._instance.GetPlayer(pl.ID);
            if (data.Status == PlayerStatus.Dead)
            {
                rms.Add(data.TeamID);
            }
        }

        foreach (int rmindex in rms)
        {
            Destroy(this._AliveWaitPlayerDic[rmindex].gameObject);
            _AliveWaitPlayerDic.Remove(rmindex);
        }

        if (BattleProxy._instance.Data.IsGameOver)
        {
            this.ShowGameOver();
        }
        else
        {
            this._btnEndRound.Show();
        }
    }

    private void ShowGameOver()
    {
        this._resultUi.Show();
        this._resultUi.SetData(BattleProxy._instance.Data.IsWin);
    }

    private void OnQuit(UIButton btn)
    {
        BattleProxy._instance.DoEndBattleResult(false);
        this.ShowGameOver();
    }

    public void Init()
    {
        this._resultUi.Hide();
        this._preUi.gameObject.SetActive(true);
        this._preUi.SetList();
        this._FightAni.SetActive(false);
        this._WaitLine.SetActive(false);
        this._ActionCon.SetActive(false);

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
        CoroutineUtil.GetInstance().WaitTime(2f, true, WaitInitEnd);
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

    private void PlayerDoAction(bool isMove)
    {
        foreach (SpeedPlayerUi pl in this._AliveWaitPlayerDic.Values)
        {
            pl.SetMove(isMove);
        }
    }

    private void JudegeMyAction()
    {
        BattlePlayer player = BattleProxy._instance.GetActionPlayer();
        if (player == null || player.TeamID < 0)
            return;
        this._ActionCon.SetActive(true);
        this._btnFight.Show();
        this._btnEndRound.Show();
        this._btnSureFight.Hide();
        this._btnCancelFight.Hide();
    }

    public void OnStateChange(BattleStatus state)
    {
        if (state == BattleStatus.Judge)
        {
            this._ActionCon.SetActive(false);
            this.PlayerDoAction(true);
        }
        if (state == BattleStatus.Action)
        {
            this.PlayerDoAction(false);
            this.JudegeMyAction();
        }
    }
}
