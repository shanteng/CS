using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class SetTeamHeroView : MonoBehaviour
    ,IScollItemClickListener
{

    public DataGrid _hGrid;
    public Text _CityTxt;
    public HeroDetailsUI _detailsUi;

    public GameObject _armyCon;
    public DataGrid _armyGrid;
    public GameObject _attrCon;
    public ArmyAttributeUi _attrUi;
    public GameObject _NoHero;

    public GameObject _btnLayout;
    public UIButton _btnDown;
    public UIButton _btnSave;

    public Slider _recruitSlider;
    public Slider _canDoSlider;
    public UIMeshTexts _curBloodTxt;

    public Transform _SpineRoot;
    private SpineUiPlayer _curModel;
    private int _selectHeroId;
    private int _selectArmyId;
    private int _usedCount = 0;
    private int _teamOriginalBlood = 0;
    private int _heroMaxBlood = 0;
    private int _cityID;
    private int _teamId;
    private int _teamHeroID;
    private int _teamArmyID;
    private int _teamArmyCount;
    void Awake()
    {
        this._recruitSlider.onValueChanged.AddListener(OnValueChange);
        this._btnDown.AddEvent(OnClickDown);
        this._btnSave.AddEvent(OnClickSave);
    }

    private void OnClickDown(UIButton btn)
    {
        Dictionary<string, object> vo = new Dictionary<string, object>();
        vo["teamid"] = this._teamId;
        vo["heroid"] = 0;
        vo["army"] = 0;
        vo["count"] = 0;
        MediatorUtil.SendNotification(NotiDefine.SetTeamHeroDo,vo);
    }

    private void OnClickSave(UIButton btn)
    {
        Dictionary<string, object> vo = new Dictionary<string, object>();
        vo["teamid"] = this._teamId;
        vo["heroid"] = this._selectHeroId;
        vo["army"] = this._selectArmyId;
        int count = (int)this._recruitSlider.value;
        vo["count"] = count;
        if (count == 0)
        {
            PopupFactory.Instance.ShowErrorNotice(ErrorCode.TeamNotIdle);
            return;
        }
        MediatorUtil.SendNotification(NotiDefine.SetTeamHeroDo, vo);
    }

    public void onClickScrollItem(ScrollData data)
    {
        if (data._Key.Equals("Hero"))
        {
            SetTeamHeroItemData curData = (SetTeamHeroItemData)data;
            if (curData._hero.Id == this._selectHeroId)
                return;
            this.SetDetails(curData._hero.Id);
        }
        else if (data._Key.Equals("Army"))
        {
            ArmySetItemData curData = (ArmySetItemData)data;
            if (curData._armyId == this._selectArmyId)
                return;
            this.SetArmy(curData._armyId);
        }
    }

    public void InitData(int teamid)
    {
        Team team = TeamProxy._instance.GetTeam(teamid);
        this._cityID = team.CityID;
        this._teamId = teamid;
        this._teamHeroID = team.HeroID;
        this._teamArmyID = 0;
        this._teamArmyCount = 0;

        this._teamArmyID = team.ArmyTypeID;
        this._teamArmyCount = team.ArmyCount;

        Hero teamHero = HeroProxy._instance.GetHero(this._teamHeroID);
        int selectIndex = -1;
        _hGrid.Data.Clear();
        if (teamHero != null)
        {
            //放在第一个
            SetTeamHeroItemData data = new SetTeamHeroItemData(teamHero, this._teamHeroID);
            this._hGrid.Data.Add(data);
            selectIndex = 0;
        }

        Dictionary<int, Hero> heros = HeroProxy._instance.GetAllHeros();
        foreach (Hero hero in heros.Values)
        {
            HeroConfig config = HeroConfig.Instance.GetData(hero.Id);
            if (hero.IsMy == false || hero.DoingState != (int)HeroDoingState.Idle || hero.City != this._cityID)
                continue;
            int inTeamID = TeamProxy._instance.GetHeroTeamID(hero.Id);
            if (inTeamID > 0)
                continue;
            SetTeamHeroItemData data = new SetTeamHeroItemData(hero, this._teamHeroID);
            this._hGrid.Data.Add(data);
        }
        _hGrid.ShowGrid(this, selectIndex);



        //army
        this._armyGrid.Data.Clear();
        Dictionary<int, Army> dic = ArmyProxy._instance.GetCityArmys(_cityID);
        if (dic != null)
        {
            foreach (Army army in dic.Values)
            {
                ArmySetItemData data = new ArmySetItemData(army.Id, army.Count);
                this._armyGrid.Data.Add(data);
            }
        }
        _armyGrid.ShowGrid(this);

        int count = this._hGrid.Data.Count;
        this._NoHero.SetActive(count == 0);
        this._btnLayout.SetActive(count > 0);
        this._detailsUi.gameObject.SetActive(count > 0);
        this._armyCon.SetActive(count > 0);
        this._attrCon.SetActive(this._armyGrid.Data.Count > 0);
        if (selectIndex >= 0)
        {
            int id = (this._hGrid.Data[selectIndex] as SetTeamHeroItemData)._hero.Id;
            this.SetDetails(id);
        }
        else if(count > 0)
        {
            int id = (this._hGrid.Data[0] as SetTeamHeroItemData)._hero.Id;
            this.SetDetails(id);
        }
    }

    private void SetDetails(int id)
    {
        if (this._curModel != null)
            Destroy(this._curModel.gameObject);
        this._selectHeroId = id;
        Team team = TeamProxy._instance.GetTeam(this._teamId);
        foreach (ItemRender render in this._hGrid.ItemRenders)
        {
            SetTeamHeroItemRender rd = (SetTeamHeroItemRender)render;
            if (rd.gameObject.activeSelf == false)
                continue;
            rd.m_renderData._IsSelect = _selectHeroId == rd.ID;
            rd.SetSelectState();
        }

     
        this._btnDown.gameObject.SetActive(this._selectHeroId == this._teamHeroID);
        this._btnSave.gameObject.SetActive(false);
        if (this._selectHeroId == 0)
            return;

        Hero hero = HeroProxy._instance.GetHero(this._selectHeroId);
        this._detailsUi.SetData(id,true);
        this.SetArmy(team.ArmyTypeID);

        HeroConfig configHero = HeroConfig.Instance.GetData(hero.Id);
        GameObject prefab = ResourcesManager.Instance.LoadSpine(configHero.Model);
        GameObject obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, this._SpineRoot);

        this._curModel = obj.GetComponent<SpineUiPlayer>();
        this._curModel.transform.localPosition = new Vector3(0, 0, 0);
        this._curModel.transform.localScale = Vector3.one;
        this._curModel.transform.localRotation = Quaternion.Euler(Vector3.zero);
        this._curModel.AddEvent(this.OnAnimationEnd, null);
        this._curModel.Play(SpineUiPlayer.STATE_ATTACK, false);
        UtilTools.ChangeLayer(this._curModel.gameObject, Layer.UI);
    }//end func

    private void OnAnimationEnd(string state)
    {
        this._curModel.Play(SpineUiPlayer.STATE_IDLE, true);
    }

    private void SetArmy(int selectid)
    {
        Team team = TeamProxy._instance.GetTeam(this._teamId);
        Hero hero = HeroProxy._instance.GetHero(this._selectHeroId);
        this._selectArmyId = selectid;
        this._usedCount = 0;
        foreach (ScrollData data in this._armyGrid.Data)
        {
            ArmySetItemData curData = (ArmySetItemData)data;
            curData._heroId = this._selectHeroId;
            if (hero != null)
                curData._heroArmy = team.ArmyTypeID;
            curData._useCount = this._usedCount;
            curData._IsSelect = curData._armyId == this._selectArmyId;
        }

        foreach (ItemRender render in this._armyGrid.ItemRenders)
        {
            ArmySetItemRender curRender = (ArmySetItemRender)render;
            curRender.UpdateRate();
            curRender.SetSelectState();
            curRender.UpdateCount();
        }

        if (selectid > 0)
            this._attrUi.SetData(selectid);

        //血量
        this._teamOriginalBlood = team.ArmyCount;
        if (this._selectArmyId != team.ArmyTypeID || this._selectHeroId != team.HeroID)
            this._teamOriginalBlood = 0;

        this._heroMaxBlood = hero.MaxBlood;

        this._recruitSlider.minValue = 0;
        this._recruitSlider.maxValue = hero.MaxBlood;
        this._recruitSlider.SetValueWithoutNotify(_teamOriginalBlood);
        this._recruitSlider.interactable = this._selectArmyId > 0;

        this._curBloodTxt.FirstLabel.text = LanguageConfig.GetLanguage(LanMainDefine.Progress, _teamOriginalBlood, hero.MaxBlood);
        this.UpdateCanRecruit();
    }

    private void UpdateCanRecruit()
    {
        Team team = TeamProxy._instance.GetTeam(this._teamId);
        Hero hero = HeroProxy._instance.GetHero(this._selectHeroId);
        int max = hero.MaxBlood;
        int value = 0;
        if (this._selectArmyId > 0)
        {
            Army army = ArmyProxy._instance.GetArmy(this._selectArmyId, this._cityID);
            int armyCount = army.Count;
            if (army.Id == team.ArmyTypeID)
                armyCount += this._teamOriginalBlood;
            value = armyCount > max ? max : armyCount;
        }

        this._canDoSlider.minValue = 0;
        this._canDoSlider.maxValue = max;
        this._canDoSlider.value = value;

        int count = (int)this._recruitSlider.value;
        this._btnSave.gameObject.SetActive(this._selectHeroId != this._teamHeroID || this._selectArmyId != this._teamArmyID || count != this._teamArmyCount);
    }


    private void OnValueChange(float value)
    {
        if (this._selectHeroId == 0)
            return;
        if (value > this._canDoSlider.value)
        {
            value = this._canDoSlider.value;
            this._recruitSlider.SetValueWithoutNotify(value);
        }

        int count = (int)value;
        this._btnSave.gameObject.SetActive(this._selectHeroId != this._teamHeroID || this._selectArmyId != this._teamArmyID || count != this._teamArmyCount);

        this._usedCount = (int)value - this._teamOriginalBlood;
        this._curBloodTxt.FirstLabel.text = LanguageConfig.GetLanguage(LanMainDefine.Progress, value, this._heroMaxBlood);
        foreach (ScrollData data in this._armyGrid.Data)
        {
            ArmySetItemData curData = (ArmySetItemData)data;
            if (curData._armyId == this._selectArmyId)
            {
                curData._useCount = this._usedCount;
                break;
            }
        }

        foreach (ItemRender render in this._armyGrid.ItemRenders)
        {
            ArmySetItemRender curRender = (ArmySetItemRender)render;
            if (curRender.gameObject.activeSelf && curRender.ID == this._selectArmyId)
            {
                curRender.UpdateCount();
                break;
            }
            
        }
    }//end func

}
