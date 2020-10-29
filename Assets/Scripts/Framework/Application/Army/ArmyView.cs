﻿using System.Collections;
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

    private int _id;
    private int _oneSecs = 0;
    void Awake()
    {
        foreach (ArmyItem item in this._toggleList)
        {
            item.AddEvent(OnSelectToggle);
        }
        this._btnStart.AddEvent(this.OnClickStart);
        this._btnGoto.AddEvent(this.OnClickGoTo);
        this._btnSpeed.AddEvent(this.OnClickStart);
        this._btnCancel.AddEvent(this.OnClickGoTo);
        this._recruitSlider.onValueChanged.AddListener(UpdateCost);
    }

    private void OnClickStart(UIButton btn)
    {
        
    }

    private void OnClickGoTo(UIButton btn)
    {

    }

    private void OnSelectToggle(UIToggle btnSelf)
    {
        ArmyItem item = (ArmyItem)btnSelf;
        this.SetData((int)item.ID);
    }

    private void SetData(int id)
    {
        foreach (ArmyItem toggle in this._toggleList)
        {
            toggle.IsOn = id == toggle.ID;
        }

        this._id = id;
        ArmyConfig config = ArmyConfig.Instance.GetData(id);
        this._nameTxt.text = config.Name;
        this._careerIcon.sprite = ResourcesManager.Instance.GetCareerIcon(config.Career);

        Army armyDoing = ArmyProxy._instance.GetDoingArmy(id);
        bool isDoing = armyDoing != null;
        bool isOpen = ArmyProxy._instance.isArmyOpen(id);

        this._DoingCon.SetActive(isDoing);
        this._SelectCon.SetActive(!isDoing && isOpen);
        this._openTxt.gameObject.SetActive(!isDoing && isOpen == false);
     

        this._btnStart.gameObject.SetActive(isOpen && isDoing== false);
        this._btnGoto.gameObject.SetActive(isOpen == false && isDoing == false);
        this._btnSpeed.gameObject.SetActive(isDoing);
        this._btnCancel.gameObject.SetActive(isDoing);


        if (isDoing)
        {
            long totleSecs = armyDoing.RecruitExpireTime - armyDoing.RecruitStartTime;
            this._CdUi.DoCountDown(armyDoing.RecruitExpireTime, totleSecs);
            this._doingCountTxt.text = ArmyProxy._instance.ComputeRecruitCount(armyDoing).ToString();
            ArmyConfig configDo = ArmyConfig.Instance.GetData(armyDoing.Id);
            this._iconDoing.sprite = ResourcesManager.Instance.getAtlasSprite(AtlasDefine.Army, configDo.Model);
        }
        else if (isOpen == false)
        {
            //显示所需科技
        }
        else
        {
            BuildingEffectsData effect = WorldProxy._instance.GetBuildingEffects();
            ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.IncomeShowValue);
            int nSecs = cfgconst.IntValues[0];
            this._oneSecs = Mathf.FloorToInt(nSecs * (1f - effect.RecruitReduceRate));

            VInt2 canDoKv = ArmyProxy._instance.GetArmyCanRecruitCountBy(id);
            this._recruitSlider.maxValue = canDoKv.y;
            this._recruitSlider.value = canDoKv.x;
            //显示消耗
            this.UpdateCost(this._recruitSlider.value);
        }
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
  
}
