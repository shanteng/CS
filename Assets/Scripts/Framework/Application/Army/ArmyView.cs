using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ArmyView : MonoBehaviour
{
    public List<ArmyItem> _toggleList;
    public Image _careerIcon;
    public Text _nameTxt;

    public GameObject _SelectCon;
    public Slider _recruitSlider;
    public Text _recuritCountTxt;
    public RectTransform _ResCon;
    public List<CostItem> _costs;

    public GameObject _DoingCon;
    public CountDownCanvas _CdUi;
    public Text _doingCountTxt;
    public Image _iconDoing;
    public Text _openTxt;

    public Text _cdTxt;
    public UIButton _btnStart;
    public UIButton _btnGoto;
    public UIButton _btnSpeed;
    public UIButton _btnCancel;
    public UIButton _btnHarvest;

    public UIModel _curModel;

    private int _id;
    private int _career;
    private int _oneSecs = 0;
    void Awake()
    {
        foreach (ArmyItem item in this._toggleList)
        {
            item.AddEvent(OnSelectToggle);
        }
        this._btnStart.AddEvent(this.OnClickStart);
        this._btnGoto.AddEvent(this.OnClickGoTo);
        this._btnSpeed.AddEvent(this.OnClickSpeed);
        this._btnCancel.AddEvent(this.OnClickCancel);
        this._btnHarvest.AddEvent(this.OnClickHarvest);

        this._recruitSlider.onValueChanged.AddListener(UpdateCost);
    }

    private void OnClickStart(UIButton btn)
    {
        Dictionary<string, object> vo = new Dictionary<string, object>();
        vo["id"] = this._id;
        vo["count"] = (int)this._recruitSlider.value;
        MediatorUtil.SendNotification(NotiDefine.RecruitArmyDo, vo);
    }

    private void OnClickGoTo(UIButton btn)
    {

    }

    private void OnClickSpeed(UIButton btn)
    {
        int id = (int)btn._param._value;
        MediatorUtil.SendNotification(NotiDefine.SpeedUpArmyDo, id);
    }

    private void OnClickCancel(UIButton btn)
    {
        int id = (int)btn._param._value;
        MediatorUtil.SendNotification(NotiDefine.CancelArmyDo, id);
    }

    private void OnClickHarvest(UIButton btn)
    {
        int id = (int)btn._param._value;
        MediatorUtil.SendNotification(NotiDefine.HarvestArmyDo, id);
    }

    private void OnSelectToggle(UIToggle btnSelf)
    {
        ArmyItem item = (ArmyItem)btnSelf;
        this.SetData((int)item.ID);
    }

    public void UpdateState(int career)
    {
        if (career != this._career)
            return;
        ArmyConfig config = ArmyConfig.Instance.GetData(this._id);
        Army armyDoing = ArmyProxy._instance.GetCareerDoingArmy(config.Career);
        bool isDoing = armyDoing != null;
        bool isOpen = ArmyProxy._instance.isArmyOpen(_id);

        this._DoingCon.SetActive(isDoing);
        this._SelectCon.SetActive(!isDoing && isOpen);
        this._openTxt.gameObject.SetActive(!isDoing && isOpen == false);

        this._btnStart.gameObject.SetActive(isOpen && isDoing == false);
        this._btnGoto.gameObject.SetActive(isOpen == false && isDoing == false);

        bool canHarvest = armyDoing != null && armyDoing.CanAccept;
        this._btnSpeed.gameObject.SetActive(isDoing && !canHarvest);
        this._btnCancel.gameObject.SetActive(isDoing && !canHarvest);
        this._btnHarvest.gameObject.SetActive(isDoing && canHarvest);


        if (isDoing)
        {
            this._btnHarvest._param._value = armyDoing.Id;
            this._btnSpeed._param._value = armyDoing.Id;
            this._btnCancel._param._value = armyDoing.Id;

            long totleSecs = armyDoing.RecruitExpireTime - armyDoing.RecruitStartTime;
            if (totleSecs == 0)
            {
                this._CdUi.Stop();
                this._CdUi._progress.maxValue = 1f;
                this._CdUi._progress.value = 1f;
                this._CdUi._CDTxt.text = LanguageConfig.GetLanguage(LanMainDefine.Finished);
            }
            else
            {
                this._CdUi.DoCountDown(armyDoing.RecruitExpireTime, totleSecs);
            }

            this._doingCountTxt.text = armyDoing.ReserveCount.ToString();
            ArmyConfig configDo = ArmyConfig.Instance.GetData(armyDoing.Id);
            this._iconDoing.sprite = ResourcesManager.Instance.getAtlasSprite(AtlasDefine.Army, configDo.Model);
        }
        else if (isOpen == false)
        {
            //显示所需科技
        }
        else
        {
            this._oneSecs = ArmyProxy._instance.GetOneRecruitSecs();
            VInt2 canDoKv = ArmyProxy._instance.GetArmyCanRecruitCountBy(this._id);
            this._recruitSlider.minValue = 1;
            this._recruitSlider.maxValue = canDoKv.y;
            this._recruitSlider.value = canDoKv.x;
            //显示消耗
            this.UpdateCost(this._recruitSlider.value);
        }
    }

    private void SetData(int id)
    {
        foreach (ArmyItem toggle in this._toggleList)
        {
            toggle.IsOn = id == toggle.ID;
        }

        this._id = id;
        ArmyConfig config = ArmyConfig.Instance.GetData(id);
        this._curModel.SetModel(config.Model);

        this._nameTxt.text = config.Name;
        this._careerIcon.sprite = ResourcesManager.Instance.GetCareerIcon(config.Career);
        this.UpdateState(this._career);
        
    }

    private void UpdateCost(float value)
    {
        ArmyConfig config = ArmyConfig.Instance.GetData(this._id);
        if (config == null)
            return;
        int count = (int)value;
        this._recuritCountTxt.text = count.ToString();
        bool isStisfy = UtilTools.SetCostList(this._costs, config.Cost, true, count);
        this._btnStart.IsEnable = isStisfy;
        int totleSecs = this._oneSecs * count;
        this._cdTxt.text = UtilTools.GetCdString(totleSecs);
    }

    public void SetList(int career)
    {
        this._career = career;
        Dictionary<int, ArmyConfig> dic = ArmyConfig.Instance.getDataArray();
        List<ArmyConfig> list = new List<ArmyConfig>();
        foreach (ArmyConfig config in dic.Values)
        {
            if (config.Career == career)
                list.Add(config);
        }

        int count = this._toggleList.Count;
        int len = list.Count;
        for (int i = 0; i < count; ++i)
        {
            if (i >= len)
            {
                this._toggleList[i].Hide();
                continue;
            }
            this._toggleList[i].Show();
            this._toggleList[i].SetData(list[i].ID);
        }

        this.SetData(list[0].ID);
    }

    public void UpdateToggle()
    {
        foreach (ArmyItem item in this._toggleList)
        {
            item.UpdateCount();
        }
    }
  
}
