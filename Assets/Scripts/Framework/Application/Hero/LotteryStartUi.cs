using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;



public class LotteryStartUi : UIBase
{
    public UIButton _btnClose;
 
    public Text _countTxt;
    public Text _countPlayerTxt;

    public Text _roundTxt;
   
    public UIButton _btnCheck;

    public GameObject _canDoCon;
    public UIButton _btnStart;
    public UIButton _btnStop;
    public GameObject _PlayerCon;
    public List<UITexts> _PlayerUis;
    private List<UITexts> _ShowUis = new List<UITexts>();
    private int _roundId;
    [HideInInspector]
    public LotteryRoundConfig _config;
    private int _totleRound = 0;
    private int _curRound = 0;
    private int _startIndex = 0;
    private bool _isStart = false;
    private Coroutine _curCor;
    private List<int> _noneLotteryPlayerIds = new List<int>();
    private List<int> _randomPageIds = new List<int>();
    private List<LotteryResultPlayer> _awardPlayers = new List<LotteryResultPlayer>();
    private bool _isDoingLottery = false;
    private Dictionary<int, LotteryNameConfig> _NameConfigDic;

    [HideInInspector]
    public LotteryView _view;
    private void Awake()
    {
        this._btnClose.AddEvent(OnCloseClick);
        this._btnStart.AddEvent(OnClickStart);
        this._btnStop.AddEvent(OnClickStop);
        this._btnCheck.AddEvent(OnChevckResult);
    }

    private void OnChevckResult(UIButton btn)
    {
        this._view.ShowResult(this._awardPlayers,this._config.ID);   
    }

    private void OnCloseClick(UIButton btn)
    {
        this._view.OnSavePlayers(this._awardPlayers, _isDoingLottery);
    }

    private void OnClickStart(UIButton btn)
    {
        this._PlayerCon.SetActive(true);
     
        this._startIndex = 0;
        this._isStart = true;
        if (this._curCor != null)
            this.StopCoroutine(_curCor);
        _curCor = StartCoroutine(DoAnimation());
        this._btnStart.Hide();
        this._btnStop.Show();

    }

    IEnumerator DoAnimation()
    {
        WaitForSeconds waitYield = new WaitForSeconds(0.05f);
        while (this._isStart)
        {
            this.SetCurrentPlayers();
            yield return waitYield;
        }
    }

    private void SetCurrentPlayers()
    {
        int count = this._noneLotteryPlayerIds.Count;
        int needCount = this._config.PageCount;

        //每轮刷新随机获得一个起始下标
        this._startIndex = UnityEngine.Random.Range(0, count);

        _randomPageIds.Clear();
        if (count < needCount)
        {
            //玩家不足needCount时，全部添加
            _randomPageIds.AddRange(this._noneLotteryPlayerIds);
        }
        else
        {
            //循环needCount次取出当前随机的玩家
            while (_randomPageIds.Count < needCount)
            {
                int id = _noneLotteryPlayerIds[this._startIndex];
                this._randomPageIds.Add(id);
                this._startIndex++;
                if (this._startIndex >= this._noneLotteryPlayerIds.Count)
                    this._startIndex = 0;
            }
        }

        //显示
        int datacount = this._randomPageIds.Count;
        int uiCount = this._ShowUis.Count;
        for (int i = 0; i < uiCount; ++i)
        {
            this._ShowUis[i].gameObject.SetActive(i < datacount);
            if (i >= datacount)
                continue;
            int currentId = this._randomPageIds[i];
            LotteryNameConfig config = _NameConfigDic[currentId];
            int index = config.Name.IndexOf('(');
            string english = config.Name;//.Substring(0, index);
            this._ShowUis[i]._texts[0].gameObject.SetActive(true);
           
            this._ShowUis[i]._icon.gameObject.SetActive(true);
            this._ShowUis[i].FirstLabel.text = english;
            Sprite sp = ResourcesManager.GetPlayerPicture(english);
            this._ShowUis[i]._icon.sprite = sp;
            this._ShowUis[i]._texts[1].gameObject.SetActive(sp == null);
        }
        
    }

    private void OnClickStop(UIButton btn)
    {
        this._isStart = false;
        this.ShowResult();
    }

    private void ShowResult()
    {
        //删除中奖玩家
        while (this._randomPageIds.Count > 0)
        {
            LotteryResultPlayer pl = new LotteryResultPlayer();
            int plid = this._randomPageIds[0];
            pl.Id = plid;
            pl.AwardId = this._roundId;
            pl.Name = this._view._NameConfigDic[plid].Name;
            this._awardPlayers.Add(pl);

            this._noneLotteryPlayerIds.Remove(plid);
            this._randomPageIds.RemoveAt(0);
        }

        this._curRound++;
        this.SetRoundData();
    }

    private void SetRoundData()
    {
        bool isOver = this._curRound >= this._totleRound || this._isDoingLottery == false;
        this._canDoCon.SetActive(isOver == false);
        this._btnCheck.gameObject.SetActive(isOver);
        this._btnStart.Show();
        this._btnStop.Hide();
        int left = this._totleRound - this._curRound;
        this._countTxt.text = UtilTools.combine("剩余次数：",left, "/", this._totleRound,"(每轮",_config.PageCount,"人)");

        int totle = _NameConfigDic.Count;
        int leftCount = this._noneLotteryPlayerIds.Count;
        int winCount = totle - leftCount;
        this._countPlayerTxt.text = LanguageConfig.GetLanguage(LanMainDefine.LotteryNumber, leftCount, totle, winCount);
    }

 
    public void SetData(LotteryRoundConfig config, List<int> players, Dictionary<int, LotteryNameConfig> NameConfigDic)
    {
        _awardPlayers.Clear();
        List<LotteryResultPlayer> awardPlayers = UtilTools.LoadLotteryResultList();
        foreach (LotteryResultPlayer pl in awardPlayers)
        {
            if (pl.AwardId == config.ID)
            {
                pl.Name = this._view._NameConfigDic[pl.Id].Name;
                this._awardPlayers.Add(pl);
            }
        }

        this._isDoingLottery = this._awardPlayers.Count == 0;
       
        this._roundTxt.text = config.Title;

        this._PlayerCon.SetActive(false);
      
        this._NameConfigDic = NameConfigDic;
       
        //随机打乱所有玩家
        this._noneLotteryPlayerIds = UtilTools.SortRandom<int>(players);

        this._roundId = config.ID;
        _config = config;
        this._totleRound = this._config.Count / this._config.PageCount;
        this._curRound = 0;

        int count = this._PlayerUis.Count;
        int pageCount = this._config.PageCount;
        this._ShowUis.Clear();
        for (int i = 0; i < count; ++i)
        {
            this._PlayerUis[i].gameObject.SetActive(i < pageCount);
            this._PlayerUis[i]._texts[0].gameObject.SetActive(false);
            this._PlayerUis[i]._texts[1].gameObject.SetActive(true);
            this._PlayerUis[i]._icon.gameObject.SetActive(false); 

            if (i < pageCount)
                this._ShowUis.Add(this._PlayerUis[i]);
        }

        this.SetRoundData();

        if (this._isDoingLottery)
        {
            //自动开始
            this.OnClickStart(null);
        }
    }
}


