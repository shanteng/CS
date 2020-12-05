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
    public List<BattleSkillItem> _btnSkills;

    public List<SkillPassiveUi> _btnPassiveSkills;

    public GameObject _buffCon;
    public List<BuffItemUi> _buffs;

    public UIButton _btnEndRound;
    public UIButton _btnCancelFight;
    public UIButton _btnSureFight;

    public GameObject _TeamCon;
    public Slider _HpSlider;
    public Text _HpTxt;
    public Slider _MpSlider;
    public Text _MpTxt;


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

        foreach (SkillPassiveUi btn in this._btnPassiveSkills)
        {
            btn.AddEvent(OnPassiveSkill);
        }

        foreach (BattleSkillItem btn in this._btnSkills)
        {
            btn._Ui.AddEvent(OnSkillFight);
        }

        this._waitPlayerTemplete.gameObject.SetActive(false);
    }

    private void OnFight(UIButton btn)
    {
        this.SetSKillBtnVisible(false);
        BattleController.Instance.SetAttackRange(0);
    }

    private void OnPassiveSkill(int skillID)
    {
        BattlePlayer pl = BattleProxy._instance.GetActionPlayer();
        BattleSkill data = pl._SkillDatas[skillID];
        PopupFactory.Instance.ShowSkill(data);
    }

    private void OnSkillFight(int skillID)
    {
        this.SetSKillBtnVisible(false);
        BattleController.Instance.SetAttackRange(skillID);
    }

    private void SetSKillBtnVisible(bool vis)
    {
        this._btnFight.gameObject.SetActive(vis);
        this._btnCancelFight.gameObject.SetActive(!vis);

        int len = this._btnSkills.Count;
        for (int i = 0; i < len; ++i)
        {
            int ID = this._btnSkills[i].ID;
            this._btnSkills[i].gameObject.SetActive(vis && ID > 0);
        }//end for
    }

    private void OnEndRound(UIButton btn)
    {
        BattleProxy._instance.DoNextRound();
    }

    private void OnCancelFight(UIButton btn)
    {
        this.SetSKillBtnVisible(true);
        this._btnSureFight.Hide();
        BattleController.Instance.BackToMoveState();
    }

    private void OnSureFight(UIButton btn)
    {
        BattleController.Instance.DoManualReleaseAction();
        this.SetSKillBtnVisible(false);
        this._btnSureFight.Hide();
        this._btnCancelFight.Hide();
        this._btnEndRound.IsEnable = false;
        this.SetHpAndMp();
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
            this._btnEndRound.IsEnable = true;
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
        this._buffCon.SetActive(false);
        this._TeamCon.SetActive(false);

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
        this._WaitLine.SetActive(true);
        this.InitWaitPlayer();
        CoroutineUtil.GetInstance().WaitTime(2f, true, WaitInitEnd);
    }

    public void OnPreFight()
    {
        this._preUi.gameObject.SetActive(false);
        this._FightAni.SetActive(true);
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

    private void SetPlayerMove(bool isMove)
    {
        foreach (SpeedPlayerUi pl in this._AliveWaitPlayerDic.Values)
        {
            pl.SetMove(isMove);
        }
    }

    private void SetHpAndMp()
    {
        BattlePlayer player = BattleProxy._instance.GetActionPlayer();
        float Blood = player.Attributes[AttributeDefine.Blood];
        float BloodOr = player.Attributes[AttributeDefine.OrignalBlood];

        float Mp = player.Attributes[AttributeDefine.Mp];
        float MpOr = player.Attributes[AttributeDefine.OrignalMp];

        this._HpSlider.value = Blood / BloodOr;
        this._MpSlider.value = Mp / MpOr;

        this._HpTxt.text = LanguageConfig.GetLanguage(LanMainDefine.Progress, Blood, BloodOr);
        this._MpTxt.text = LanguageConfig.GetLanguage(LanMainDefine.Progress, Mp, MpOr);
    }


    public void OnAiEnd()
    {
        this._btnEndRound.IsEnable = true;
    }

    public void UpdateActionPlayerBuff()
    {
        int len = this._buffs.Count;
        for (int i = 0; i < len; ++i)
        {
            this._buffs[i].Hide();
        }//end for

       
        BattlePlayer player = BattleProxy._instance.GetActionPlayer();
        if (player == null)
            return;
        int index = 0;
        foreach (BattleEffectBuff buff in player._Buffs.Values)
        {
            //先全部显示，后面改为0round不显示
            if (buff.Duration < 0)
                continue;
            this._buffs[index].Show();
            this._buffs[index].SetData(buff);
            index++;
        }

        this._buffCon.SetActive(index > 0);

    }

    private void JudegeAction()
    {
        BattlePlayer player = BattleProxy._instance.GetActionPlayer();
        this._TeamCon.SetActive(true);
        this.UpdateActionPlayerBuff();
        this.SetHpAndMp();

        int len = this._btnPassiveSkills.Count;
        for (int i = 0; i < len; ++i)
        {
            this._btnPassiveSkills[i].Hide();
        }//end for

        //设置普攻图标和技能图标，以及技能状态
        int skillCount = player._SkillDatas.Count;
        len = this._btnSkills.Count;
        for (int i = 0; i < len; ++i)
        {
            this._btnSkills[i].SetData(0,0);
            this._btnSkills[i].Hide();
        }//end for



        int index = 0;
        int indexPass = 0;
        foreach (BattleSkill data in player._SkillDatas.Values)
        {
            SkillConfig config = SkillConfig.Instance.GetData(data.ID);
            if (config.MpCost > 0)
            {
                this._btnSkills[index].SetData(data.ID, data.Level);
                this._btnSkills[index].Show();
                index++;
            }
            else
            {
                this._btnPassiveSkills[indexPass].SetData(data.ID, data.Level);
                this._btnPassiveSkills[indexPass].Show();
                indexPass++;
            }
        }



        //我自己
        this._ActionCon.SetActive(player.TeamID > 0);
        this._btnEndRound.IsEnable = player.TeamID > 0;
        if (player.TeamID > 0)
        {
            //普攻图标
            this._btnFight.Icon.sprite = ResourcesManager.Instance.GetArmySprite(player.ArmyID);
            this.SetSKillBtnVisible(true);
            this._btnSureFight.Hide();
            this._btnCancelFight.Hide();
        }//end if
    
    }//end func

    public void OnStateChange(BattleStatus state)
    {
        if (state == BattleStatus.Judge)
        {
            this._TeamCon.SetActive(false);
            this._ActionCon.SetActive(false);
            this.SetPlayerMove(true);
        }
        if (state == BattleStatus.Action)
        {
            this.SetPlayerMove(false);
            this.JudegeAction();
        }
    }
}
