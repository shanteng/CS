using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class TeamAttackView : MonoBehaviour
    ,IScollItemClickListener
{
    public DataGrid _CityGrid;
    public DataGrid _TeamGrid;
    public DataGrid _NpcGrid;
    public List<FightHeroUi> _HeroFightList;
    public UIButton _BtnFight;

    public Text _CityTxt;
    public TextMeshProUGUI _CordinateTxt;
    public UIButton _btnCheckCity;
   

    public Text _NpcCountTxt;
    public UITexts _ExpAddTxt;
    public List<CostBig> _awards;

    public UITexts _TimeTxt;
    public UITexts _MoraleTxt;
    public UITexts _EnegryTxt;
    public UITexts _TotleTxt;
    public RectTransform _Res;
    public List<CostItem> _ResList;


    private List<int> FightTeamList = new List<int>();
    private VInt2 Goto;
    private int _curCityId = 0;
    private int _targetCityID = 0;
    void Awake()
    {
       // this._CordinateTxt.GetComponent<UIClickHandler>().AddListener(OnEndClick);
        this._btnCheckCity.AddEvent(OnClickCheck);
        this._BtnFight.AddEvent(OnClickFight);
        foreach (FightHeroUi heroUi in this._HeroFightList)
        {
            heroUi._btnDel.AddEvent(OnRemoveFight);
        }
    }

    private void OnRemoveFight(UIButton btn)
    {
        int teamid = (int)btn._param._value;
        this.FightTeamList.Remove(teamid);
        this.UpdateFightList();
    }

    private void OnClickCheck(UIButton btn)
    {
        PopupFactory.Instance.ShowNpcCityInfo(this._targetCityID);
    }

    private void OnClickFight(UIButton btn)
    {
        Dictionary<string, object> vo = new Dictionary<string, object>();
        vo["cityid"] = this._targetCityID;
        vo["from"] = this._curCityId;
        vo["teams"] = this.FightTeamList;
        MediatorUtil.SendNotification(NotiDefine.MoveToAttackCityDo, vo);
    }

    private void OnEndClick(object param)
    {
        ViewControllerLocal.GetInstance().TryGoto(this.Goto);
    }

    public void onClickScrollItem(ScrollData data)
    {
        if (data._Key.Equals("TeamCityItemData"))
        {
            TeamCityItemData cityData = (TeamCityItemData)data;
            if (cityData._info.ID == this._curCityId)
                return;
            this.SetCity(cityData._info.ID);
        }
        else if (data._Key.Equals("Team"))
        {
            int teamid = (int)data._Param;
            if (this.FightTeamList.Contains(teamid))
                this.FightTeamList.Remove(teamid);
            else
                this.FightTeamList.Add(teamid);
            this.UpdateFightList();
        }
    }

   
    public void InitData(int npccity)
    {
        this._targetCityID = npccity;
        CityConfig config = CityConfig.Instance.GetData(npccity);
        this._CityTxt.text = config.Name;

        Goto = WorldProxy._instance.GetCityCordinate(this._targetCityID);
        VInt2 gamePos = UtilTools.WorldToGameCordinate(Goto.x, Goto.y);
        this._CordinateTxt.text = LanguageConfig.GetLanguage(LanMainDefine.SpotCordinate, gamePos.x, gamePos.y);
        //设置守军信息
        this._NpcGrid.Data.Clear();
        int[] npcs = config.NpcTeams;
        int count = 0;
        int addExp = 0;
        TeamProxy._instance.GetFightAwardTotleExp(npccity, out count, out addExp);
        for (int i = 0; i < npcs.Length; ++i)
        {
            ScrollData data = new ScrollData();
            data._Param = -npcs[i];
            this._NpcGrid.Data.Add(data);
        }
        this._NpcGrid.ShowGrid(null);

        this._NpcCountTxt.text = count.ToString();
        this._ExpAddTxt.FirstLabel.text = addExp.ToString();

        UtilTools.SetFullAwardtList(this._awards, config.AttackDrops);

        //我的城市
        Dictionary<int, CityData> dic = WorldProxy._instance.AllCitys;
        _CityGrid.Data.Clear();
  
        foreach (CityData cityInfo in dic.Values)
        {
            if (cityInfo.IsOwn)
            {
                TeamCityItemData data = new TeamCityItemData(cityInfo);
                this._CityGrid.Data.Add(data);
            }
        }
        this._CityGrid.ShowGrid(this);
        this.SetCity(0);
    }

    private void SetCity(int city)
    {
        foreach (ItemRender render in this._CityGrid.ItemRenders)
        {
            TeamCityItemRender rd = (TeamCityItemRender)render;
            if (rd.gameObject.activeSelf == false)
                continue;
            rd.m_renderData._IsSelect = rd.ID.Equals(city);
            rd.SetSelectState();
        }

        this._curCityId = city;
        List<Team> list = TeamProxy._instance.GetCityTeams(this._curCityId);
        _TeamGrid.Data.Clear();
        int upCount = 0;
        foreach (Team t in list)
        {
            if (t.HeroID == 0)
                continue;

            ScrollData data = new ScrollData();
            data._Param = t.Id;
            if (t.HeroID > 0)
                upCount++;
            this._TeamGrid.Data.Add(data);
        }
        this._TeamGrid.ShowGrid(this);

        //时间
        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.AttackDeltaSces);
        int SecsDelta = cfgconst.IntValues[0];
        BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects(city);
        float deltaFinal = (1f - effect.TeamMoveReduceRate) * (float)SecsDelta;
        VInt2 StartPos = WorldProxy._instance.GetCityCordinate(city);
        long ExpireTime = WorldProxy._instance.GetMoveExpireTime(StartPos.x, StartPos.y, Goto.x, Goto.y, deltaFinal);//栈道来提升速度百分比
        this._TimeTxt.FirstLabel.text = UtilTools.GetCdStringExpire(ExpireTime);
        long NeedSecs = ExpireTime - GameIndex.ServerTime;


        //气势
        cfgconst = ConstConfig.Instance.GetData(ConstDefine.MoraleReduceDelta);
        int onePercentReduceSecs = cfgconst.IntValues[0];
        int leftPercent = 100 -  (int)(NeedSecs / onePercentReduceSecs);
        if (leftPercent < 0)
            leftPercent = 0;
        this._MoraleTxt.FirstLabel.text = LanguageConfig.GetLanguage(LanMainDefine.Percent, leftPercent);

        cfgconst = ConstConfig.Instance.GetData(ConstDefine.HeroCostEnegry);
        int needEnegry = cfgconst.IntValues[0];
        this._EnegryTxt.FirstLabel.text = needEnegry.ToString();

        this.FightTeamList.Clear();
        this.UpdateFightList();
    }//end func

    private void UpdateFightList()
    {
        foreach (ItemRender rd in this._TeamGrid.ItemRenders)
        {
            if (rd.gameObject.activeSelf == false)
                continue;
            int curid = (int)rd.m_renderData._Param;
            rd.m_renderData._IsSelect = this.FightTeamList.Contains(curid);
            rd.SetSelectState();
        }

        int count = this._HeroFightList.Count;
        int len = this.FightTeamList.Count;
        int totleCount = 0;
        for (int i = 0; i < count; ++i)
        {
            if (i >= len)
            {
                this._HeroFightList[i].gameObject.SetActive(false);
                continue;
            }
            this._HeroFightList[i].gameObject.SetActive(true);
            int troopCount = this._HeroFightList[i].SetData(FightTeamList[i]);
            totleCount += troopCount;
        }//end for

        this._TotleTxt.FirstLabel.text = totleCount.ToString();
        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.FightOneArmyCost);
        bool isStisfy = UtilTools.SetCostList(this._ResList, cfgconst.StringValues, true, totleCount);
        LayoutRebuilder.ForceRebuildLayoutImmediate(this._Res);
        this._BtnFight.gameObject.SetActive(len > 0);
        this._BtnFight.IsEnable = isStisfy;
    }//end f


}
