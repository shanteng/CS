using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Newtonsoft.Json.Utilities;

public enum OpType
{
    Enter=1,
    Info,
    Upgrade,
    Cancel,
    Patrol,
    QuestCity,
    NpcCityInfo,
    Attack,
}

public class IntStrPair
{
    public OpType OpType;
    public string BtnName;
    public string Icon;
    public bool Enable;
    public object Param;
    public IntStrPair(OpType key, string value="",string icon = "",bool en = true,object param = null)
    {
        this.OpType = key;
        this.BtnName = value;
        this.Icon = icon;
        this.Enable = en;
        this.Param = param;
    }
}


public class InfoCanvas : UIBase, IConfirmListener
{
    public RectTransform _NameRect;
    public Text _spotNameTxt;
    public TextMeshProUGUI _cordinateTxt;

    public GameObject _btnCon;
    public List<UIButton> _btnFunList;


    public GameObject _cdCon;
    public CountDownCanvas _countDown;
    public UIButton _btnSpeedUp;
    public Animator _animator;
    private BuildingData _data;
   
    private Coroutine _cor;
    private string _addType;
    private int _cityId;
    private VInt2 _curPos = new VInt2();

    public int City => this._cityId;
    private void Start()
    {
        foreach (UIButton btn in this._btnFunList)
        {
            btn.AddEvent(this.OnClickFun);
        }
        this._btnSpeedUp.AddEvent(this.OnClickSpeedUp);
    }

    public new void Show()
    {
        if (this._cor != null)
        {
            CoroutineUtil.GetInstance().Stop(this._cor);
            this._cor = null;
        }
        this.gameObject.SetActive(true);
        this._animator.enabled = true;
        //this.transform
    }

    public new void Hide()
    {
        this._animator.SetTrigger("MoveOut");
        _cor =  CoroutineUtil.GetInstance().WaitTime(0.4f, true, WaitInitEnd);
        this._cityId = -1;
    }

    private void WaitInitEnd(object[] param)
    {
        this.gameObject.SetActive(false);
    }

    private void GetBtnList( out List<IntStrPair> btnTypeList)
    {
        BuildingConfig config = BuildingConfig.Instance.GetData(this._data._id);
        BuildingUpgradeConfig configLevel = BuildingUpgradeConfig.GetConfig(this._data._id, this._data._level);

        this._addType = config.AddType;

        btnTypeList = new List<IntStrPair>();

        IntStrPair data = new IntStrPair(OpType.Info,LanguageConfig.GetLanguage(LanMainDefine.OpInfo), "OpDetails");
        btnTypeList.Add(data);

        if (this._data._status == BuildingData.BuildingStatus.NORMAL && this._data._level < config.MaxLevel)
        {
            bool canUpgrade = true;
            if (config.AddType.Equals(ValueAddType.RecruitVolume))
            {
                int career = UtilTools.ParseInt(configLevel.AddValues[0]);
                int doingCount = ArmyProxy._instance.GetCareerRecruitCount(career,this._cityId);
                canUpgrade = doingCount == 0;
            }

            if (canUpgrade)
            {
                data = new IntStrPair(OpType.Upgrade, LanguageConfig.GetLanguage(LanMainDefine.OpUpgrade), "OP_UPGRADE");
                btnTypeList.Add(data);
            }
        }
        else if (this._data._status == BuildingData.BuildingStatus.UPGRADE)
        {
            data = new IntStrPair(OpType.Cancel, LanguageConfig.GetLanguage(LanMainDefine.OpCancel), "Op_cancel");
            btnTypeList.Add(data);
        }
      

        if (this._data._status == BuildingData.BuildingStatus.BUILD)
            return;//建造中的没有功能入口
       
        //功能入口
        switch (config.AddType)
        {
            case ValueAddType.HourTax:
                {
                  /*  CostData add = new CostData();
                    add.Init(configLevel.AddValues[0]);
                    int value = RoleProxy._instance.GetCanAcceptIncomeValue(add.id);
                    bool isEnable = value >= this._needValueShow;
                    data = new IntStrPair(OpType.Enter, "", add.id, isEnable, add.id);
                    btnTypeList.Add(data);
                  */
                    break;
                }
            case ValueAddType.RecruitVolume:
                {
                    if (this._data._status == BuildingData.BuildingStatus.NORMAL)
                    {
                        string icon = UtilTools.combine("ca", configLevel.AddValues[0]);
                        int career = UtilTools.ParseInt(configLevel.AddValues[0]);
                        data = new IntStrPair(OpType.Enter, LanguageConfig.GetLanguage(LanMainDefine.OpEnter), icon, true, career);
                        btnTypeList.Add(data);
                    }
                    break;
                }
            case ValueAddType.HeroRecruit:
                {
                    data = new IntStrPair(OpType.Enter, LanguageConfig.GetLanguage(LanMainDefine.OpEnter), "OpClientCityTroop");
                    btnTypeList.Add(data);
                    break;
                }
        }

    }

    public void SetNpcCity(int city)
    {
        this._cityId = city;
        this._curPos = WorldProxy._instance.GetCityCordinate(city);
        bool isOwn = WorldProxy._instance.IsOwnCity(this._cityId);
        CityConfig config = CityConfig.Instance.GetData(city);
        if (isOwn)
            this._spotNameTxt.text = LanguageConfig.GetLanguage(LanMainDefine.OwnOccupyCityName, config.Name);
        else
            this._spotNameTxt.text = config.Name;

        VInt2 kv = UtilTools.WorldToGameCordinate(this._curPos.x, this._curPos.y);
        this._cordinateTxt.text = LanguageConfig.GetLanguage(LanMainDefine.SpotCordinate, kv.x, kv.y);
        this._cordinateTxt.gameObject.SetActive(true);
        List<IntStrPair> btnTypeList;
        this.GetNpcCityBtnList(out btnTypeList);
        this.SetBtnState(btnTypeList);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_NameRect);
        this._cdCon.gameObject.SetActive(false);
    }

    public void SetEmptySpot(int x, int z)
    {
        this._cityId = -1;
        this.SetCurrentPos(x, z);
        bool isVisible = WorldProxy._instance.IsSpotVisible(this._curPos.x, this._curPos.y);
        if (isVisible)
            this._spotNameTxt.text = LanguageConfig.GetLanguage(LanMainDefine.EmptyVisibleSpot);
        else
            this._spotNameTxt.text = LanguageConfig.GetLanguage(LanMainDefine.EmptyUnVisibleSpot);

        VInt2 kv = UtilTools.WorldToGameCordinate(this._curPos.x, this._curPos.y);
        this._cordinateTxt.text = LanguageConfig.GetLanguage(LanMainDefine.SpotCordinate, kv.x, kv.y);
        this._cordinateTxt.gameObject.SetActive(true);

        List<IntStrPair> btnTypeList;
        this.GetSpotBtnList(out btnTypeList);
        this.SetBtnState(btnTypeList);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_NameRect);
        this._cdCon.gameObject.SetActive(false);
    }

    private void SetCurrentPos(int x, int z)
    {
        this._curPos.x = x;
        this._curPos.y = z;
    }

    private void GetNpcCityBtnList(out List<IntStrPair> btnTypeList)
    {
        bool isOwn = WorldProxy._instance.IsOwnCity(this._cityId);
        btnTypeList = new List<IntStrPair>();

        IntStrPair data = new IntStrPair(OpType.NpcCityInfo, LanguageConfig.GetLanguage(LanMainDefine.OpCityInfo), "OpCheckRole");
        btnTypeList.Add(data);

        if (isOwn)
        {
            data = new IntStrPair(OpType.QuestCity, LanguageConfig.GetLanguage(LanMainDefine.OpQuestCity), "OP_DIG");
            btnTypeList.Add(data);
        }
        else
        {
            data = new IntStrPair(OpType.Attack, LanguageConfig.GetLanguage(LanMainDefine.OpAttack), "OP_ATK");
            btnTypeList.Add(data);
        }
    }

    private void GetSpotBtnList(out List<IntStrPair> btnTypeList)
    {
        bool isVisible = WorldProxy._instance.IsSpotVisible(this._curPos.x, this._curPos.y);
        btnTypeList = new List<IntStrPair>();

        if (isVisible == false)
        {
            IntStrPair data = new IntStrPair(OpType.Patrol, LanguageConfig.GetLanguage(LanMainDefine.OpPatrol), "Op_Patrol");
            btnTypeList.Add(data);
        }
    }

    private int _needValueShow = 0;
    public void SetBuildState(BuildingData data)
    {
        this.SetCurrentPos(data._cordinate.x, data._cordinate.y);
        this._cityId = data._city;
        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.IncomeShowValue);
        _needValueShow = cfgconst.IntValues[0];

        this._data = data;
        BuildingConfig config = BuildingConfig.Instance.GetData(this._data._id);
        string showName = "";
        if (this._data._status == BuildingData.BuildingStatus.BUILD)
        {
            showName = config.Name;
        }
        else
        {
            showName = LanguageConfig.GetLanguage(LanMainDefine.NameLv, config.Name, this._data._level);
        }

        this._spotNameTxt.text = showName;

        VInt2 kv = UtilTools.WorldToGameCordinate(this._data._cordinate.x, this._data._cordinate.y);
        this._cordinateTxt.text = LanguageConfig.GetLanguage(LanMainDefine.SpotCordinate,kv.x,kv.y);
        this._cordinateTxt.gameObject.SetActive(false);
        List<IntStrPair> btnTypeList;
        this.GetBtnList(out btnTypeList);
        this.SetBtnState(btnTypeList);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_NameRect);

        bool isBuildUp = this._data._status == BuildingData.BuildingStatus.BUILD || this._data._status == BuildingData.BuildingStatus.UPGRADE;
        this._cdCon.gameObject.SetActive(isBuildUp);
        if (isBuildUp)
        {
            this.SetBuildUpgrade();
        }
        else
        {
            this._countDown.Stop();
        }
    }

    private void SetBtnState(List<IntStrPair> btnTypeList)
    {
        var len = btnTypeList.Count;
        this._btnCon.SetActive(len > 0);
        int count = this._btnFunList.Count;
        for (int i = 0; i < count; ++i)
        {
            if (i >= len)
            {
                this._btnFunList[i].Hide();
                continue;
            }
            this._btnFunList[i].Show();
            this._btnFunList[i]._param._key = ((int)btnTypeList[i].OpType).ToString();
            this._btnFunList[i]._param._value = btnTypeList[i].Param;

            bool hasIcon = btnTypeList[i].Icon.Equals("") == false;
            this._btnFunList[i].Icon.gameObject.SetActive(hasIcon);
            this._btnFunList[i].Label.text = btnTypeList[i].BtnName;
            //this._btnFunList[i].Label.gameObject.SetActive(hasIcon == false);
            if (hasIcon)
            {
                this._btnFunList[i].Icon.sprite = ResourcesManager.Instance.getAtlasSprite(AtlasDefine.Common, btnTypeList[i].Icon);
                this._btnFunList[i].Icon.SetNativeSize();
            }
            this._btnFunList[i].IsEnable = btnTypeList[i].Enable;
            UIRoot.Intance.SetImageGray(this._btnFunList[i].Icon, !btnTypeList[i].Enable);
        }
    }

    private void SetBuildUpgrade()
    {
        this._btnSpeedUp.gameObject.SetActive(this._data._status == BuildingData.BuildingStatus.UPGRADE);
        this._countDown.DoCountDown(this._data._expireTime,this._data._UpgradeSecs);
    }

    private void OnEnterClick(UIButton btn)
    {
        if (this._cityId == 0)
        {
            if (this._addType.Equals(ValueAddType.HourTax))
            {
                string attrKey = (string)btn._param._value;
                MediatorUtil.SendNotification(NotiDefine.AcceptHourAwardDo, attrKey);
            }
            else if (this._addType.Equals(ValueAddType.RecruitVolume))
            {
                int career = (int)btn._param._value;
                VInt2 kv = new VInt2();
                kv.x = this._cityId;
                kv.y = career;

                MediatorUtil.ShowMediator(MediatorDefine.ARMY, kv);
            }
            else if (this._addType.Equals(ValueAddType.HeroRecruit))
            {
                MediatorUtil.ShowMediator(MediatorDefine.RECRUIT, 0);
            }
        }
    }

    private void OnClickFun(UIButton btn)
    {
        int op = UtilTools.ParseInt(btn._param._key);
        switch ((OpType)op)
        {
            case OpType.Enter:
                {
                    this.OnEnterClick(btn);
                    break;
                }
            case OpType.Info:
                {
                    PopupFactory.Instance.ShowBuildingInfo(this._data._key);
                    break;
                }
            case OpType.Upgrade:
                {
                    PopupFactory.Instance.ShowBuildingUpgrade(this._data._key);
                    break;
                }
            case OpType.Cancel:
                {
                    if(this._data._status == BuildingData.BuildingStatus.UPGRADE)
                        PopupFactory.Instance.ShowConfirm(LanguageConfig.GetLanguage(LanMainDefine.CancelUpgradeNotice), this, "Cancel");
                    else if (this._data._status == BuildingData.BuildingStatus.BUILD)
                        PopupFactory.Instance.ShowConfirm(LanguageConfig.GetLanguage(LanMainDefine.CancelBuildNotice), this, "Cancel");
                    break;
                }
            case OpType.Patrol:
                {
                    VInt2 kv = WorldProxy._instance.GetCityPatrolInfo(0);
                    if (kv.y == 0)
                    {
                        PopupFactory.Instance.ShowErrorNotice(ErrorCode.NoPatroller);
                        return;
                    }
                    PopupFactory.Instance.ShowPatrol(this._curPos);
                    break;
                }
            case OpType.QuestCity:
                {
                    bool hasFree = HeroProxy._instance.HasFreeHero();
                    if (hasFree == false)
                    {
                        PopupFactory.Instance.ShowErrorNotice(ErrorCode.NoHeroFreeToQuest);
                        return;
                    }

                    PopupFactory.Instance.ShowQuestNpcCity(this._cityId);
                    break;
                }
            case OpType.NpcCityInfo:
                {
                    PopupFactory.Instance.ShowNpcCityInfo(this._cityId);
                    break;
                }
            case OpType.Attack:
                {
                    WorldProxy._instance.DoOwnCity(this._cityId);
                    break;
                }
        }
        HomeLandManager.GetInstance().HideInfoCanvas();
    }

    public void OnConfirm(ConfirmData data)
    {
        if (data.userKey.Equals("Cancel"))
        {
            MediatorUtil.SendNotification(NotiDefine.BuildingCancelDo, this._data._key);
        }
    }

    private void OnClickSpeedUp(UIButton btn)
    {
        MediatorUtil.SendNotification(NotiDefine.BuildingSpeedUpDo, this._data._key);
    }

   

}
