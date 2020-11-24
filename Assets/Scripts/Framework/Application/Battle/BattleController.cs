using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class BattleController : MonoBehaviour
{
    public GameObject _battleSpots;
    public GameObject _Base;
    public List<GameObject> _prefabs;
    private Dictionary<string, GameObject> _prefabDic;

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

    private void Init()
    {
        this._prefabDic = new Dictionary<string, GameObject>();
        foreach (GameObject obj in this._prefabs)
        {
            this._prefabDic.Add(obj.name, obj);
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

    
}


