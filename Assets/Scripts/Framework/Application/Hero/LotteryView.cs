using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LotteryView : MonoBehaviour, IConfirmListener
{
    public GameObject _mainUi;
    public Text _NameText;
    public UIButton _BtnStart;
    public UIButton _BtnResult;
    public UIButton _BtnClear;
    public UIButton _BtnClearRound;
    public InputField _RoundInput;
    public Text _countPlayerTxt;
    public UIButton _btnCode;
    public GameObject _codeWin;
     public UIButton _btnCloseCode;

    public LotteryStartUi _startUi;

    public UIButton _btnGM;
    public GameObject _HideCon;

    public GameObject _logUi;
    public DataGrid _hGrid;
    public UIButton _BtnCloseLog;

    public UIButton _btnPre;
    public UIButton _btnNext;

    private List<int> _noneLotteryPlayerIds = new List<int>();
    private Dictionary<int, int> _AwardPlayersDic;
    private int _id;
    void Start()
    {
        _BtnStart.AddEvent(this.OnClickStart);
        _BtnResult.AddEvent(this.OnResult);
        _BtnClear.AddEvent(this.OnClear);
        _BtnClearRound.AddEvent(this.OnClearRound);
        _BtnCloseLog.AddEvent(this.OnCloseLog);
        _btnGM.AddEvent(this.OnGm);

        _btnCloseCode.AddEvent(this.OnCloseCode);
        _btnCode.AddEvent(this.OnCode);

        _btnPre.AddEvent(this.OnPre);
        _btnNext.AddEvent(this.OnNext);

        this._RoundInput.onValueChanged.AddListener(OnChangeEvent);
        this._startUi._view = this;
    }

    private void OnChangeEvent(string value)
    {
        int id = UtilTools.ParseInt(value);
        LotteryRoundConfig config;
        if (this._RoundConfigDic.TryGetValue(id, out config) == false)
        {
            this._NameText.text = "";
            return;
        }
        this._id = config.ID;
        this._NameText.text = config.Title;
        this._RoundInput.SetTextWithoutNotify(value);
    }

    private void OnPre(UIButton btn)
    {
        int preId = this._id - 1;
        this.OnChangeEvent(preId.ToString());
    }

    private void OnNext(UIButton btn)
    {
        int nextid = this._id + 1;
        this.OnChangeEvent(nextid.ToString());
    }


    private void OnCode(UIButton btn)
    {
        this._codeWin.SetActive(true);
    }

    private void OnCloseCode(UIButton btn)
    {
        this._codeWin.SetActive(false);
    }

    private void OnGm(UIButton btn)
    {
        this._HideCon.SetActive(!this._HideCon.activeSelf);
    }

    private void OnCloseLog(UIButton btn)
    {
        this._logUi.SetActive(false);
    }

    private void OnClickStart(UIButton btn)
    {
        int id = UtilTools.ParseInt(this._RoundInput.text);
        LotteryRoundConfig config;
        if (this._RoundConfigDic.TryGetValue(id, out config) == false)
        {
            PopupFactory.Instance.ShowNotice("Wrong Id!");
            return;
        }

        this._mainUi.gameObject.SetActive(false);
        this._startUi.gameObject.SetActive(true);
        this._startUi.SetData(config, this._noneLotteryPlayerIds, this._NameConfigDic);
    }

    public void OnConfirm(ConfirmData data)
    {
        if (data.userKey.Equals("clear"))
        {
            int clearid = (int)data.param;
            List<LotteryResultPlayer> players = UtilTools.LoadLotteryResultList();
            List<LotteryResultPlayer> leftplayers = new List<LotteryResultPlayer>();

            this._AwardPlayersDic.Clear();
            foreach (LotteryResultPlayer pl in players)
            {
                if (pl.AwardId != clearid && clearid > 0)
                {
                    leftplayers.Add(pl);
                    this._AwardPlayersDic[pl.Id] = pl.AwardId;
                }
            }

            _noneLotteryPlayerIds.Clear();
            foreach (LotteryNameConfig config in _NameConfigDic.Values)
            {
                if (this._AwardPlayersDic.ContainsKey(config.ID) == false)
                    this._noneLotteryPlayerIds.Add(config.ID);
            }

            string jsonstr = JsonConvert.SerializeObject(leftplayers, Formatting.Indented);
            UtilTools.SaveLotterResult(jsonstr);

            this.SetPlayerCount();

            if (this._startUi.gameObject.activeSelf)
            {
                this._startUi.SetData(_startUi._config, this._noneLotteryPlayerIds, this._NameConfigDic);
            }

            if (this._logUi.activeSelf)
            {
                _hGrid.Data.Clear();
                _hGrid.ShowGrid(null);
            }
        }
    }

    private void OnClearRound(UIButton btn)
    {
        int id = UtilTools.ParseInt(this._RoundInput.text);
        LotteryRoundConfig config;
        if (this._RoundConfigDic.TryGetValue(id, out config) == false)
        {
            PopupFactory.Instance.ShowNotice("Wrong Id!");
            return;
        }

        string notice = UtilTools.combine("是否清理(", config.Title, ")的抽奖结果？");
        PopupFactory.Instance.ShowConfirm(notice, this, "clear", id);
    }

    private void OnClear(UIButton btn)
    {
        if (this._startUi.gameObject.activeSelf)
            this.OnClearRound(null);
        else
            PopupFactory.Instance.ShowConfirm("确认要清除所有中奖玩家记录?", this, "clear", 0);
    }

    public void ShowResult(List<LotteryResultPlayer> players,int roundId = 0)
    {
        this._logUi.SetActive(true);
        _hGrid.Data.Clear();

        Dictionary<int, List<LotteryResultPlayer>> roundPlayersDic = new Dictionary<int, List<LotteryResultPlayer>>();
        foreach (LotteryResultPlayer player in players)
        {
            if (roundId > 0 && player.AwardId != roundId)
                continue;//
            List<LotteryResultPlayer> list;
            if (roundPlayersDic.TryGetValue(player.AwardId, out list) == false)
            {
                list = new List<LotteryResultPlayer>();
                roundPlayersDic.Add(player.AwardId, list);
            }
            list.Add(player);
        }

        foreach (int id in roundPlayersDic.Keys)
        {
            List<LotteryResultPlayer> curList = roundPlayersDic[id];
            LotteryRoundConfig configRound = this._RoundConfigDic[id];
            LotteryResultItemData data = new LotteryResultItemData(configRound, curList);
            data._Param = configRound.ID;
            this._hGrid.Data.Add(data);
        }
        this._hGrid.Data.Sort(Compare);
        _hGrid.ShowGrid(null);
    }

    private int Compare(ScrollData x, ScrollData y)
    {
        int idx = (int)x._Param;
        int idy = (int)y._Param;
        return UtilTools.compareInt(idx, idy);
    }

    private void OnResult(UIButton btn)
    {
        List<LotteryResultPlayer> players = UtilTools.LoadLotteryResultList();
        this.ShowResult(players);
    }

    public void OnSavePlayers(List<LotteryResultPlayer> awardPlayers, bool isDoing)
    {
        //保存数据到本地
        if (isDoing)
        {
            List<LotteryResultPlayer> players = new List<LotteryResultPlayer>();
            foreach (LotteryResultPlayer pl in awardPlayers)
            {
                _AwardPlayersDic[pl.Id] = pl.AwardId;
            }

            foreach (int plid in this._AwardPlayersDic.Keys)
            {
                int awardid = this._AwardPlayersDic[plid];
                LotteryNameConfig config = this._NameConfigDic[plid];
                LotteryRoundConfig configRd = this._RoundConfigDic[awardid];


                LotteryResultPlayer pl = new LotteryResultPlayer();
                pl.Id = plid;
                pl.AwardId = awardid;
                pl.Name = config.Name;
                players.Add(pl);
            }

            //to json
            string jsonstr = JsonConvert.SerializeObject(players, Formatting.Indented);
            UtilTools.SaveLotterResult(jsonstr);
        }
      
        this._startUi.gameObject.SetActive(false);
        this._mainUi.gameObject.SetActive(true);
        this.SetPlayerCount();
    }

     
    public Dictionary<int, LotteryNameConfig> _NameConfigDic = new Dictionary<int, LotteryNameConfig>();
    public Dictionary<int, LotteryRoundConfig> _RoundConfigDic = new Dictionary<int, LotteryRoundConfig>();
    public void InitData()
    {
        this._codeWin.SetActive(false);
        this._mainUi.SetActive(true);
        this._startUi.gameObject.SetActive(false);
        this._logUi.SetActive(false);
        this._HideCon.SetActive(false);
        _noneLotteryPlayerIds.Clear();

        var filePath = Application.streamingAssetsPath + "/Name.json";
        var strContent = File.ReadAllText(filePath);
       // Debug.LogError(strContent);

        this._AwardPlayersDic = UtilTools.LoadLotteryResult();
        List<LotteryNameConfig> configList = JsonConvert.DeserializeObject<List<LotteryNameConfig>>(strContent);
        foreach (LotteryNameConfig config in configList)
        {
            _NameConfigDic[config.ID] = config;
            if (this._AwardPlayersDic.ContainsKey(config.ID))
                continue;
            this._noneLotteryPlayerIds.Add(config.ID);
        }

        filePath = Application.streamingAssetsPath + "/Round.json";
        strContent = File.ReadAllText(filePath);
        List<LotteryRoundConfig> configRoundList = JsonConvert.DeserializeObject<List<LotteryRoundConfig>>(strContent);
        foreach (LotteryRoundConfig configRound in configRoundList)
        {
            _RoundConfigDic[configRound.ID] = configRound;
        }
        this.SetPlayerCount();
        this.OnChangeEvent(this._RoundInput.text);
    }

    private void SetPlayerCount()
    {
        int totle = _NameConfigDic.Count;
        int leftCount = this._noneLotteryPlayerIds.Count;
        int winCount = totle - leftCount;
        this._countPlayerTxt.text = LanguageConfig.GetLanguage(LanMainDefine.LotteryNumber, leftCount, totle, winCount);
    }
}
