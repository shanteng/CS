using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using UnityEngine.Events;

public class BattleController : MonoBehaviour
{
    public GameObject _battleSpots;
    public GameObject _Base;
    public List<GameObject> _prefabs;
    private Dictionary<string, GameObject> _prefabDic;

    public List<GameObject> _EffectPrefabs;
    private Dictionary<string, GameObject> _EffectPrefabDic;

    public Transform _BornRoot;
    private Dictionary<int,Vector3> _BornPostions;
    private Dictionary<int,BornSpot> _AttackBorns;
    private Dictionary<int, BornSpot> _DefenseBorns;

    public Transform _PlayerRoot;
    private Dictionary<int, BattlePlayerUi> _PlayerDic;

    public Transform _SpotRoot;
    private Dictionary<string, BattleSpot> _SpotDic;

    private BattleSceneConfig _config;
    private bool _isMyAttack;
    private int _curSelectBornIndex = 0;
    private static BattleController ins;
    public static BattleController Instance => ins;

    void Awake()
    {
        ins = this;
    }

    private void OnClickBorn(BornSpot script)
    {
        this._curSelectBornIndex = script.ID;
        Dictionary<int, BornSpot> Borns;
        if (this._isMyAttack)
            Borns = this._AttackBorns;
        else
            Borns = this._DefenseBorns;
        foreach (BornSpot spot in Borns.Values)
        {
            spot.SetSelect(spot.ID == _curSelectBornIndex);
        }
    }

    public int GetEmptyBornIndex()
    {
        for (int i = 0; i < this._MyBorns.Count; ++i)
        {
            bool isEmpty = BattleProxy._instance.IsEmptyBornIndex(this._MyBorns[i]);
            if (isEmpty)
                return this._MyBorns[i];
        }

        return 0;
    }

    public void SetSelctBornTeam(int teamid)
    {
        if (this._curSelectBornIndex == 0)
        {
            this._curSelectBornIndex = this.GetEmptyBornIndex();
        }

        BattleProxy._instance.SetMyPlayerBorn(teamid, this._curSelectBornIndex);
        this.UpdateMyPlayerBorn();
        this._curSelectBornIndex = 0;
    }

    public void UnSetSelctBornTeam(int teamid)
    {
        BattleProxy._instance.UnMySetPlayerBorn(teamid);
        this.UpdateMyPlayerBorn();
    }

    public void UpdateMyPlayerBorn()
    {
        foreach (BattlePlayerUi player in this._PlayerDic.Values)
        {
            if (player.ID < 0)
                continue;
            BattlePlayer data = BattleProxy._instance.GetPlayer(player.ID);
            player.gameObject.SetActive(data.BornIndex > 0);
            player.SetPostion();
        }
    }

    public void StartBattle()
    {
        this._battleSpots.SetActive(true);
        this._Base.SetActive(false);
        this._BornRoot.gameObject.SetActive(false);
    }

    public void OnStateChange(BattleStatus state)
    {
        if (state == BattleStatus.Judge)
        {
            //清除所有的地块状态
            foreach (BattleSpot spot in this._SpotDic.Values)
            {
                spot.AddEvent(null);
                spot.ChangeColor(BattleSpotStatus.Normal);
            }
        }
    }

    public void PlayerDoAction()
    {
        foreach (BattlePlayerUi pl in this._PlayerDic.Values)
        {
            pl.SetDoAction();
        }
        this.SetCurrentPlayerMoveRange();

        //如果是Npc执行一下Ai
        BattlePlayer player = BattleProxy._instance.GetActionPlayer();
        if (player != null)
        {
            if (player.TeamID < 0)
                this.DoAiAction(player);
            else
                this.DoMyAction(player);
        }
    }

    private void DoMyAction(BattlePlayer pl)
    {
      //  this.DoAiAction(pl);
    }

    Coroutine _cor;
    private Queue<AiStep> _AiSteps;
    private bool _isPlayAi = false;
    private void DoAiAction(BattlePlayer player)
    {
        _AiSteps = BattleProxy._instance.DoAi(player);
        this._isPlayAi = false;
        _cor = StartCoroutine(PlayAiSteps());
    }

    IEnumerator PlayAiSteps()
    {
        while (this._AiSteps.Count > 0)
        {
            if (this._isPlayAi)
                yield return 0;
            else
            {
                AiStep effect = this._AiSteps.Dequeue();
                this.PlayCurrentAiStep(effect);
                yield return 0;
            }
        }

        while (_isPlayAi)
        {
            yield return 0;
        }
        //全部执行完毕了
        this.OnAiEnd();
    }


    private void PlayCurrentAiStep(AiStep step)
    {
        this._isPlayAi = step._Step != AiStepType.End;
        if (step._Step == AiStepType.Move)
        {
            this.PlayerDoMoveTo(step._Postion, OnAiCurrentStepEnd);
        }
        else if (step._Step == AiStepType.ReleaseSkill)
        {
            this.SetAttackRange(step._SkillID, step._Postion);
        }
    }

    private void OnAiCurrentStepEnd()
    {
        this._isPlayAi = false;
    }

    private void OnAiEnd()
    {
        this._cor = null;
        MediatorUtil.SendNotification(NotiDefine.BattleAiEnd);
    }

    public void StopAi()
    {
        if (this._cor != null)
        {
            CoroutineUtil.GetInstance().Stop(this._cor);
            this._cor = null;
        }
    }

    private void SetSpotAttackEffects(BattlePlayer player)
    {
        string effectRes = BattleEffect.Attack;
        SkillConfig configSkill = SkillConfig.Instance.GetData(player._AttackSkillID);
        if (configSkill != null)
            effectRes = configSkill.EffectRes;

        if (player.SkillDemageCordinates != null)
        {
            BattleSpot spot;
            foreach (VInt2 attackPos in player.SkillDemageCordinates)
            {
                string key = UtilTools.combine(attackPos.x, "|", attackPos.y);
                if (this._SpotDic.TryGetValue(key, out spot))
                {
                    spot.AddEvent(null);
                    BattleEffect effect = this.CreateBattleEffect(effectRes, new Vector3(attackPos.x, 0, attackPos.y));
                    effect.gameObject.SetActive(true);
                    GameObject.Destroy(effect.gameObject, effect.Sces);
                }
            }
        }
    }

    private void OnAttackEnd()
    {
        //删除已经死亡的模型
        List<int> rms = new List<int>();
        foreach (BattlePlayerUi pl in this._PlayerDic.Values)
        {
            BattlePlayer data = BattleProxy._instance.GetPlayer(pl.ID);
            if (data.Status == PlayerStatus.Dead)
            {
                rms.Add(data.TeamID);
            }
        }

        foreach (int rmindex in rms)
        {
            Destroy(this._PlayerDic[rmindex].gameObject);
            _PlayerDic.Remove(rmindex);
        }

        //清除所有地块状态，等待玩家点击结束回合进入下一轮
        BattlePlayer player = BattleProxy._instance.GetActionPlayer();
        player._AttackSkillID = -1;

        //通知UI更新
        MediatorUtil.SendNotification(NotiDefine.AttackPlayerEndJudge);
    
        if (_maunalCallBack != null)
        {
            _maunalCallBack.Invoke();
            _maunalCallBack = null;
        }
    }




    private UnityAction _maunalCallBack;
    public void DoManualReleaseAction(UnityAction callBack = null)
    {
        this._maunalCallBack = callBack;
        BattlePlayer actionPlayer = BattleProxy._instance.GetActionPlayer();
        int attackerTeamID = actionPlayer.TeamID;
        int skillID = actionPlayer._AttackSkillID;
        this.DoReleaseSkillCallAction(actionPlayer, skillID);

        if (skillID == 0)
        {
            //连携判断
            foreach (BattleSkill skill in actionPlayer._SkillDatas.Values)
            {
                SkillConfig config = SkillConfig.Instance.GetData(skill.ID);
                if (config.ReleaseTerm.Equals(SkillReleaseTerm.AfterAttack))
                {
                    this.DoReleaseSkillCallAction(actionPlayer, skill.ID);
                    break;
                }
            }//end for
        }

        actionPlayer.HasDoRoundActionFinish = true;

        //去掉地块点击事件
        BattleSpot spot;
        foreach (VInt2 attackPos in actionPlayer.SkillFightRangeCordinates)
        {
            string key = UtilTools.combine(attackPos.x, "|", attackPos.y);
            if (this._SpotDic.TryGetValue(key, out spot))
            {
                spot.AddEvent(null);
            }
        }

        BattlePlayer player = BattleProxy._instance.GetActionPlayer();
      //  this.SetSpotAttackEffects(player);
        //移除可移动状态
        if (player.ActionMoveCordinates != null)
        {
            foreach (VInt2 Pos in player.ActionMoveCordinates)
            {
                string key = UtilTools.combine(Pos.x, "|", Pos.y);
                if (this._SpotDic.TryGetValue(key, out spot))
                {
                    spot.ChangeColor(BattleSpotStatus.Normal);
                    spot.AddEvent(null);
                }
            }
        }

        if (player.SkillFightRangeCordinates != null)
        {
            foreach (VInt2 Pos in player.SkillFightRangeCordinates)
            {
                string key = UtilTools.combine(Pos.x, "|", Pos.y);
                if (this._SpotDic.TryGetValue(key, out spot))
                {
                    spot.ChangeColor(BattleSpotStatus.Normal);
                    spot.AddEvent(null);
                }
            }
        }

        if (player.SkillDemageCordinates != null)
        {
            foreach (VInt2 Pos in player.SkillDemageCordinates)
            {
                string key = UtilTools.combine(Pos.x, "|", Pos.y);
                if (this._SpotDic.TryGetValue(key, out spot))
                {
                    spot.ChangeColor(BattleSpotStatus.Normal);
                    spot.AddEvent(null);
                }
            }
        }

        //等待各种伤害数字以及被击中的人的被打击动画播放完毕
        //CoroutineUtil.GetInstance().WaitTime(_skillSecs, true, OnAttackEnd, callBack);
    }



    Queue<ReleaseSkillActionData> _releaseSkillPlayers = new Queue<ReleaseSkillActionData>();
    private Coroutine _corRelease;
    public void DoReleaseSkillCallAction(BattlePlayer pl, int skillID)
    {
        //先喊招
        _releaseSkillPlayers.Enqueue(new ReleaseSkillActionData(pl, skillID));
        if (_corRelease == null)
        {
            this._isDoingCallAction = false;
            _corRelease = StartCoroutine(ReleaseSkillCallAction());
        }
    }

    private bool _isDoingCallAction = false;
    private ReleaseSkillActionData _curRelaseData;
    IEnumerator ReleaseSkillCallAction()
    {
        while (_releaseSkillPlayers.Count > 0)
        {
            if (this._isDoingCallAction)
            {
                yield return 0;
            }
            else
            {
                _curRelaseData = this._releaseSkillPlayers.Dequeue();
                this.PlayCallAction();
                yield return 0;
            }
        }

        while (_isDoingCallAction)
        {
            yield return 0;
        }
        _corRelease = null;//全部释放完毕了，判断一下战斗是否结束

        BattleData bd = BattleProxy._instance.Data;
        if (bd.Status == BattleStatus.Action)
        {
            this.OnAttackEnd();
        }
        BattleProxy._instance.OnAllSkillReleased();
    }

    private void PlayCallAction()
    {
        this._isDoingCallAction = true;
        BattlePlayerUi plUi = this._PlayerDic[_curRelaseData.player.TeamID];
        plUi.SkillCall(_curRelaseData.skillID, OnAttackCallAnimationEnd); //通知喊招
    }


    private void OnAttackCallAnimationEnd()
    {
        BattlePlayer player = BattleProxy._instance.GetActionPlayer();
        if (player != null && player._AttackSkillID >= 0)
            this.SetSpotAttackEffects(player);

        //真正释放技能
        BattlePlayerUi plUi = this._PlayerDic[_curRelaseData.player.TeamID];
        plUi.PlayAnimation(SpineUiPlayer.STATE_ATTACK);//播放攻击动
        List<PlayerEffectChangeData> effectPlayers =  _curRelaseData.player.ReleaseSkill(_curRelaseData.skillID);
        if (effectPlayers.Count > 0)
            this.EffectPlayerResponseToSkill(effectPlayers, OnOneSkillReleaseEnd);
        else
            OnOneSkillReleaseEnd();
        //通知UI隐藏喊招
        MediatorUtil.SendNotification(NotiDefine.CallSkillUIHide);
    }

    private void OnOneSkillReleaseEnd()
    {
        this._isDoingCallAction = false;
        BattlePlayerUi plUi = this._PlayerDic[_curRelaseData.player.TeamID];
        plUi.PlayAnimation(SpineUiPlayer.STATE_IDLE);//播放攻击动
    }

    public void OrderlyReleaseBeforeStartSkill(List<BattlePlayer> upList)
    {
        int count = upList.Count;
        for (int i = 0; i < count; ++i)
        {
            foreach (BattleSkill skill in upList[i]._SkillDatas.Values)
            {
                SkillConfig config = SkillConfig.Instance.GetData(skill.ID);
                if (config.ReleaseTerm.Equals(SkillReleaseTerm.BeforeStart))
                {
                    this.DoReleaseSkillCallAction(upList[i], skill.ID);
                }
            }
        }//end for
    }


    private Queue<PlayerEffectChangeData> _PlayerEffectsResp = new Queue<PlayerEffectChangeData>();
    private Coroutine _corEffect;
    public void EffectPlayerResponseToSkill(List<PlayerEffectChangeData> effectPlayers,UnityAction callBack)
    {
        foreach (PlayerEffectChangeData effect in effectPlayers)
        {
            _PlayerEffectsResp.Enqueue(effect);
        }

        if(this._corEffect == null)
            this._corEffect = StartCoroutine(PlayEffects(callBack));
        MediatorUtil.SendNotification(NotiDefine.BattleEffectChange);
    }

    IEnumerator PlayEffects(UnityAction callBack)
    {
        yield return new WaitForSeconds(0.2f);
        WaitForSeconds waitYield = new WaitForSeconds(0.2f);
        while (this._PlayerEffectsResp.Count > 0)
        {
            PlayerEffectChangeData data = (this._PlayerEffectsResp.Dequeue()); ;
            BattlePlayerUi plUi = this._PlayerDic[data.TeamID];
            plUi.ReponseToEffect(data);
            yield return waitYield;
        }
        this._corEffect = null;
        if (callBack != null)
            callBack.Invoke();
    }

   

    public void BackToMoveState()
    {
        //返回到可移动状态的地块
        BattlePlayer player = BattleProxy._instance.GetActionPlayer();
        BattleSpot spot;
        if (player.SkillFightRangeCordinates != null)
        {
            foreach (VInt2 Pos in player.SkillFightRangeCordinates)
            {
                string key = UtilTools.combine(Pos.x, "|", Pos.y);
                if (this._SpotDic.TryGetValue(key, out spot))
                {
                    spot.ChangeColor(BattleSpotStatus.Normal);
                    spot.AddEvent(null);
                }
            }
        }

        if (player.SkillDemageCordinates != null)
        {
            foreach (VInt2 Pos in player.SkillDemageCordinates)
            {
                string key = UtilTools.combine(Pos.x, "|", Pos.y);
                if (this._SpotDic.TryGetValue(key, out spot))
                {
                    spot.ChangeColor(BattleSpotStatus.Normal);
                    spot.AddEvent(null);
                }
            }
        }

        foreach (VInt2 Pos in player.ActionMoveCordinates)
        {
            string key = UtilTools.combine(Pos.x, "|", Pos.y);
            if (this._SpotDic.ContainsKey(key) == false)
                continue;//不在范围内
            bool spotOccupy = BattleProxy._instance.IsSpotOccupy(Pos.x, Pos.y, player.TeamID);
            if (spotOccupy)
                this._SpotDic[key].ChangeColor(BattleSpotStatus.MoveDisable);
            else
                this._SpotDic[key].ChangeColor(BattleSpotStatus.MoveEnable);
            if (spotOccupy == false && player.TeamID > 0)
            {
                this._SpotDic[key].AddEvent(OnClickMoveSpot);
            }
        }

        player.SkillFightRangeCordinates = null;
        player.SkillFightRangeCordinatesStrList = null;
        player.SkillDemageCordinates = null;
        player._AttackSkillID = -1;
    }

  

    public void SetAttackRange(int skillid,VInt2 autoAttackPos=null)
    {
        //skillid 0-代表普通攻击
        BattlePlayer player = BattleProxy._instance.GetActionPlayer();
        player._AttackSkillID = skillid;
        //移除可移动状态，
        foreach (VInt2 movePos in player.ActionMoveCordinates)
        {
            string key = UtilTools.combine(movePos.x, "|", movePos.y);
            this._SpotDic[key].ChangeColor(BattleSpotStatus.Normal);
            this._SpotDic[key].AddEvent(null);
        }

        //设置攻击范围
        player.ComputeSkillFightRange(skillid,player.Postion);
        foreach (VInt2 attackPos in player.SkillFightRangeCordinates)
        {
            string key = UtilTools.combine(attackPos.x, "|", attackPos.y);
            if (this._SpotDic.ContainsKey(key) == false)
                continue;//不在范围内
           
            this._SpotDic[key].ChangeColor(BattleSpotStatus.CanAttack);
            if (autoAttackPos == null)
                this._SpotDic[key].AddEvent(OnClickAttackSpot);
        }
        //等待玩家选择要攻击的地块坐标
        if (autoAttackPos != null)
        {
            string key = UtilTools.combine(autoAttackPos.x, "|", autoAttackPos.y);
            BattleSpot spot;
            if (this._SpotDic.TryGetValue(key, out spot))
                this.AttackSpot(spot, true, this.OnAiCurrentStepEnd);
        }
    }

    private void AttackSpot(BattleSpot spot,bool isAuto,UnityAction callBack)
    {
        BattlePlayer player = BattleProxy._instance.GetActionPlayer();
        //清除旧的伤害范围
        if (player.SkillDemageCordinates != null && player.SkillDemageCordinates.Count > 0)
        {
            foreach (VInt2 oldPos in player.SkillDemageCordinates)
            {
                string key = UtilTools.combine(oldPos.x, "|", oldPos.y);
                if (this._SpotDic.ContainsKey(key) == false)
                    continue;//不在范围内
                if (player.SkillFightRangeCordinatesStrList.Contains(key))//说明是攻击范围
                    this._SpotDic[key].ChangeColor(BattleSpotStatus.CanAttack);
                else
                    this._SpotDic[key].ChangeColor(BattleSpotStatus.Normal);
            }
        }

        //设置显示新的伤害范围
        player.ComputeSkillDemageRange(spot.Pos, player._AttackSkillID, player.Postion);
        foreach (VInt2 attackPos in player.SkillDemageCordinates)
        {
            string key = UtilTools.combine(attackPos.x, "|", attackPos.y);
            if (this._SpotDic.ContainsKey(key) == false)
                continue;//不在范围内
            this._SpotDic[key].ChangeColor(BattleSpotStatus.AttackDemageRange);
        }


        if (isAuto == false)
        {
            MediatorUtil.SendNotification(NotiDefine.ShowSureFight);
        }
        else
        {
            //直接攻击
            this.DoManualReleaseAction(callBack);
        }
    }

    private void OnClickAttackSpot(BattleSpot spot)
    {
        this.AttackSpot(spot,false,null);
    }

    private void SetCurrentPlayerMoveRange()
    {
        BattlePlayer player = BattleProxy._instance.GetActionPlayer();
        if (player == null)
            return;
        player.ActionMoveCordinates = new List<VInt2>();
        int halfRange = (int)player.Attributes[AttributeDefine.MoveRange];
        VInt2 centerPos = new VInt2(player.Postion.x, player.Postion.y);
        int startX = centerPos.x - halfRange;
        int endX = centerPos.x + halfRange;
        int startZ = centerPos.y - halfRange;
        int endZ = centerPos.y + halfRange;

        for (int row = startX; row <= endX; ++row)
        {
            int corX = row;
            for (int col = startZ; col <= endZ; ++col)
            {
                int corZ = col;
                string key = UtilTools.combine(corX, "|", corZ);
                if (this._SpotDic.ContainsKey(key) == false)
                    continue;//不在范围内
                bool spotOccupy = BattleProxy._instance.IsSpotOccupy(corX, corZ, player.TeamID);
                if (spotOccupy)
                    this._SpotDic[key].ChangeColor(BattleSpotStatus.MoveDisable);
                else
                    this._SpotDic[key].ChangeColor(BattleSpotStatus.MoveEnable);
                player.ActionMoveCordinates.Add(new VInt2(corX, corZ));

                if (player.TeamID > 0 && spotOccupy == false)//添加地块点击事件
                {
                    this._SpotDic[key].AddEvent(OnClickMoveSpot);
                }

            }//end for col
        }//end for row

        //由近及远的排序
        player.SortMoveCordinate();
    }

  




    private void OnClickMoveSpot(BattleSpot spot)
    {
         this.PlayerDoMoveTo(spot.Pos);
    }

    private bool _isMoving = false;
    private float _moveDelta = 0.3f;
    private float PlayerDoMoveTo(VInt2 pos,UnityAction callBack = null)
    {
        float needSecs = 0f;
        BattlePlayer player = BattleProxy._instance.GetActionPlayer();
        if (player == null || _isMoving)
            return needSecs;
        if (pos.x == player.Postion.x && pos.y == player.Postion.y)
            return needSecs;

        VInt2 startPos = new VInt2(player.Postion.x, player.Postion.y);

        if (pos.x != startPos.x)
            needSecs += _moveDelta;
        if (pos.y != startPos.y)
            needSecs += _moveDelta;

        this._isMoving = true;
        BattleProxy._instance.SetPlayerPostion(player.TeamID, pos.x, pos.y);
        BattlePlayerUi pl = this._PlayerDic[player.TeamID];
        pl.PlayAnimation(SpineUiPlayer.STATE_WALK, needSecs);

        //Tween过去 先x后z
        if (pos.x != startPos.x)
        {
            pl.transform.DOMoveX(pos.x, _moveDelta).onComplete = () =>
            {
                if (pos.y != startPos.y)
                {
                    pl.transform.DOMoveZ(pos.y, _moveDelta).onComplete = () =>
                    {
                        this._isMoving = false;
                        if (callBack != null)
                            callBack.Invoke();
                    };
                }
                else
                {
                    this._isMoving = false;
                    if (callBack != null)
                        callBack.Invoke();
                }
            };
        }
        else
        {
            pl.transform.DOMoveZ(pos.y, _moveDelta).onComplete = () =>
            {
                this._isMoving = false;
                if (callBack != null)
                    callBack.Invoke();
            };
        }

        return needSecs;
    }

    private void Init()
    {
        this._prefabDic = new Dictionary<string, GameObject>();
        foreach (GameObject obj in this._prefabs)
        {
            this._prefabDic.Add(obj.name, obj);
        }

        this._EffectPrefabDic = new Dictionary<string, GameObject>();
        foreach (GameObject obj in this._EffectPrefabs)
        {
            this._EffectPrefabDic.Add(obj.name, obj);
        }

        this._BornPostions = new Dictionary<int, Vector3>();
        Transform tran = GameObject.FindGameObjectWithTag("Born").transform;
        int count = tran.childCount;
        for (int i = 0; i < count; ++i)
        {
            Transform bronTran = tran.GetChild(i);
            int index = UtilTools.ParseInt(bronTran.name);
            this._BornPostions.Add(index, bronTran.position);
        }
    }

    List<int> _MyBorns;
    public void InitPreBattle()
    {
        this.Init();
        this._battleSpots.SetActive(false);
        this._Base.SetActive(true);
        this._BornRoot.gameObject.SetActive(true);

        this._curSelectBornIndex = 0;
        BattleProxy._instance.SaveBornPostion(this._BornPostions);
        BattleData data = BattleProxy._instance.Data;
        this._config = BattleSceneConfig.Instance.GetData(data.Id);
        //spots
        int rowhalf = this._config.RowCol[0] / 2;
        int colhalf = this._config.RowCol[1] / 2;
        this._SpotDic = new Dictionary<string, BattleSpot>();
        for (int row = -rowhalf; row <= rowhalf; ++row)
        {
            int corX = row;
            for (int col =-colhalf; col <= colhalf; ++col)
            {
                int corZ = col;
                Vector3 pos = new Vector3(corX, 0, corZ);
                BattleSpot script = this.CreatePrefab<BattleSpot>("BattleSpot", pos.ToString(), pos, this._SpotRoot);
                script.transform.localEulerAngles = new Vector3(90, 0, 0);
                script.InitPostion(corX, corZ);
                string key = UtilTools.combine(corX, "|", corZ);
                this._SpotDic.Add(key, script);
            }
        }

        this._isMyAttack = data.MyPlace == BattlePlace.Attack;
        _MyBorns = new List<int>();

        if (this._isMyAttack)
            _MyBorns.AddRange(this._config.AttackBorn);
        else
            _MyBorns.AddRange(this._config.DefenseBorn);


        //设置出生点
        this._AttackBorns = new Dictionary<int, BornSpot>();
        int len = this._config.AttackBorn.Length;
        for (int i = 0; i < len; ++i)
        {
            int bornIndex = this._config.AttackBorn[i];
            Vector3 pos = this._BornPostions[bornIndex];
            BornSpot script = this.CreatePrefab<BornSpot>("BornSpot", bornIndex.ToString(),pos, this._BornRoot);
            script.Init(bornIndex, pos, BattlePlace.Attack, data.MyPlace);
            script.transform.localEulerAngles = new Vector3(90, 0, 0);
            if (data.MyPlace == BattlePlace.Attack)
                script.AddEvent(this.OnClickBorn);
            this._AttackBorns.Add(bornIndex, script);
        }

        this._DefenseBorns = new Dictionary<int, BornSpot>();
        len = this._config.DefenseBorn.Length;
        for (int i = 0; i < len; ++i)
        {
            int bornIndex = this._config.DefenseBorn[i];
            Vector3 pos = this._BornPostions[bornIndex];
            BornSpot script = this.CreatePrefab<BornSpot>("BornSpot", bornIndex.ToString(), pos, this._BornRoot);
            script.Init(bornIndex, pos, BattlePlace.Defense, data.MyPlace);
            script.transform.localEulerAngles = new Vector3(90, 0, 0);
            if (data.MyPlace == BattlePlace.Defense)
                script.AddEvent(this.OnClickBorn);
            this._DefenseBorns.Add(bornIndex, script);
        }
        //构造Npc
        this._PlayerDic = new Dictionary<int, BattlePlayerUi>();
        this.InitPlayers();
    }//end func



    private void InitPlayers()
    {
        BattleData data = BattleProxy._instance.Data;
        foreach(BattlePlayer pl in data.Players.Values)
        {
            int bornIndex = pl.BornIndex;
            Vector3 pos;
            if (pl.TeamID < 0)
                pos = this._BornPostions[bornIndex];
            else
                pos = Vector3.zero;

            BattlePlayerUi player = this.CreatePrefab<BattlePlayerUi>("BattlePlayer", pl.TeamID.ToString(), pos, this._PlayerRoot);
            player.SetData(pl, data.MyPlace);
            player.gameObject.SetActive(pl.TeamID < 0);
            this._PlayerDic.Add(pl.TeamID, player);
        }
    }


    private T CreatePrefab<T>(string name, string objname,Vector3 postion,Transform root)
    {
        GameObject prefab = this._prefabDic[name];
        GameObject obj = GameObject.Instantiate(prefab, postion, Quaternion.identity, root);
        obj.name = UtilTools.combine(name,":",objname);
        T script = obj.GetComponent<T>();
        return script;
    }

    private BattleEffect CreateBattleEffect(string nameStr, Vector3 postion)
    {
        GameObject prefab = this._EffectPrefabDic[nameStr];
        GameObject obj = GameObject.Instantiate(prefab, postion, Quaternion.identity, this._SpotRoot);
        obj.name = nameStr;
        BattleEffect script = obj.GetComponent<BattleEffect>();
        return script;
    }


}


