using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

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

    public void SetSelctBornTeam(int teamid)
    {
        if (this._curSelectBornIndex == 0)
        {
            PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.SelectUpPostion));
            return;
        }
        BattleProxy._instance.SetMyPlayerBorn(teamid, this._curSelectBornIndex);
        this.UpdateMyPlayerBorn();
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
        else if (state == BattleStatus.Action)
        {
            this.PlayerDoAction();
        }
    }

    private void PlayerDoAction()
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
                this.DoMyAction();
        }
    }

    private void AttackAnimationEnd()
    {
        BattlePlayer player = BattleProxy._instance.GetActionPlayer();

        string effectRes = BattleEffect.Attack;
        SkillConfig configSkill = SkillConfig.Instance.GetData(player._AttackSkillID);
        if (configSkill != null)
            effectRes = configSkill.EffectRes;

        //再AttackRange显示特效
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
                _skillSecs = effect.Sces;
            }
        }
    }

    private float _skillSecs = 2f;
    public void DoAttack()
    {
        BattleProxy._instance.DoPlayerAttackAction();
        BattlePlayer player = BattleProxy._instance.GetActionPlayer();
        player.HasDoRoundActionFinish = true;

        BattlePlayerUi pl = this._PlayerDic[player.TeamID];
        pl.PlayAnimation(SpineUiPlayer.STATE_ATTACK, 0.3f,this.AttackAnimationEnd);


        BattleSpot spot;
        foreach (VInt2 attackPos in player.SkillFightRangeCordinates)
        {
            string key = UtilTools.combine(attackPos.x, "|", attackPos.y);
            if (this._SpotDic.TryGetValue(key, out spot))
            {
                spot.AddEvent(null);
            }
        }
    }

    public void ResponseToSkillBehaviour(List<PlayerEffectChangeData> effectPlayers)
    {
        foreach (PlayerEffectChangeData data in effectPlayers)
        {
            BattlePlayerUi pl = this._PlayerDic[data.TeamID];
            //播放被击动画
            pl.ReponseToEffect(data);
        }
        //等待各种伤害数字以及被击中的人的被打击动画播放完毕
        CoroutineUtil.GetInstance().WaitTime(_skillSecs, true, OnAttackEnd);
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

    private void OnAttackEnd(object[] param)
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
        //移除可移动状态
        BattleSpot spot;
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

        BattleProxy._instance.OnPlayerActionFinishded(player.TeamID);
        //通知UI更新
        MediatorUtil.SendNotification(NotiDefine.AttackPlayerEndJudge);
        if (player.TeamID < 0 && BattleProxy._instance.Data.IsGameOver == false)//玩家自己，等待点击结束回合
        {
            //自动进入下一轮
            BattleProxy._instance.DoNextRound();
        }
    }

    public void SetAttackRange(int skillid)
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
        player.ComputeSkillFightRange(skillid);
        foreach (VInt2 attackPos in player.SkillFightRangeCordinates)
        {
            string key = UtilTools.combine(attackPos.x, "|", attackPos.y);
            if (this._SpotDic.ContainsKey(key) == false)
                continue;//不在范围内
           
            this._SpotDic[key].ChangeColor(BattleSpotStatus.CanAttack);
            this._SpotDic[key].AddEvent(OnClickAttackSpot);
        }
        //等待玩家选择要攻击的地块坐标
    }

    private void OnClickAttackSpot(BattleSpot spot)
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
                if(player.SkillFightRangeCordinatesStrList.Contains(key))//说明是攻击范围
                    this._SpotDic[key].ChangeColor(BattleSpotStatus.CanAttack);
                else
                    this._SpotDic[key].ChangeColor(BattleSpotStatus.Normal);
            }
        }

        //设置显示新的伤害范围
        player.ComputeSkillDemageRange(spot.Pos);
        foreach (VInt2 attackPos in player.SkillDemageCordinates)
        {
            string key = UtilTools.combine(attackPos.x, "|", attackPos.y);
            if (this._SpotDic.ContainsKey(key) == false)
                continue;//不在范围内
            this._SpotDic[key].ChangeColor(BattleSpotStatus.AttackDemageRange);
        }


        MediatorUtil.SendNotification(NotiDefine.ShowSureFight);
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

        
    }

    private void DoMyAction()
    {
         
    }

    public void StopAi()
    {
        if (this._cor != null)
        {
            CoroutineUtil.GetInstance().Stop(this._cor);
            this._cor = null;
        }
    }

    Coroutine _cor;
    private void DoAiAction(BattlePlayer player)
    {
        //暂时随便移动一下
        int randomIndex = UtilTools.RangeInt(0, player.ActionMoveCordinates.Count-1);
        float moveSecs = this.PlayerDoMoveTo(player.ActionMoveCordinates[randomIndex]);
        _cor =   CoroutineUtil.GetInstance().WaitTime(moveSecs, true, OnAiEnd);
    }

    private void OnAiEnd(object[] param)
    {
        this._cor = null;
        //进入下一轮
        if (BattleProxy._instance.Data.Status == BattleStatus.Action)
            BattleProxy._instance.DoNextRound();
    }

    private void OnClickMoveSpot(BattleSpot spot)
    {
         this.PlayerDoMoveTo(spot.Pos);
    }

    private bool _isMoving = false;
    private float _moveDelta = 0.3f;
    private float PlayerDoMoveTo(VInt2 pos)
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
                    };
                }
                else
                {
                    this._isMoving = false;
                }
            };
        }
        else
        {
            pl.transform.DOMoveZ(pos.y, _moveDelta).onComplete = () =>
            {
                this._isMoving = false;
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


