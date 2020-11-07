using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.PlayerIdentity;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class MainView : MonoBehaviour
{
    public RoleHead _head;
    public Text _nameTxt;
    public TextMeshProUGUI _levelTxt;
    public Text _expTxt;
    public Image _lvSlider;
    public UIButton _BtnBuild;
    public UIButton _BtnSdk;
    public InfoCanvas _InfoUI;
    public Image _dianLiang;
    public Text _timeTxt;
    public List<IncomeItem> _incomeList;

    public TextMeshProUGUI _powerTxt;
    //mainbtn
    public UIButton _btnBag;
    public UIButton _btnHero;

    public MapUI _mapUI;

    private int _dianLiangCount = 60;
    private static MainView instance;
    public static MainView GetInstance()
    {
        return instance;
    }
    private void Awake()
    {
        instance = this;
    }

    public Vector3 GetIncomePostion(string key)
    {
        foreach (IncomeItem item in this._incomeList)
        {
            if (item._key.Equals(key))
            {
                Vector3 pos =  UIRoot.Intance.GetLayer(WindowLayer.Popup).worldToLocalMatrix.MultiplyPoint3x4(item.Icon.GetComponent<RectTransform>().position);
                return pos;
            }
        }
        return Vector3.zero;
    }

    void Start()
    {
        _dianLiangCount = 60;
        _BtnBuild.AddEvent(this.OnClickBuild);
        _BtnSdk.AddEvent(this.OnSdk);
    
        _btnBag.AddEvent(this.OnClickBag);
        _btnHero.AddEvent(this.OnClickHero);

        this._InfoUI.gameObject.SetActive(false);
        StartCoroutine(CountDown()); 
    }

    IEnumerator CountDown()
    {
        WaitForSeconds waitYield = new WaitForSeconds(1f);
        while (this.gameObject != null)
        {
            _timeTxt.text = DateTime.Now.ToString("T");
            _dianLiangCount++;
            if (_dianLiangCount >= 60)
            {
                this.setDianLiang();
                this._dianLiangCount = 0;
            }
            yield return waitYield;
        }
    }


    public void SetPower()
    {
        RoleInfo role = RoleProxy._instance.Role;
        this._powerTxt.text = role.Power.ToString();
    }

    public void SetLevelExp()
    {
        RoleInfo role = RoleProxy._instance.Role;
        RoleLevelConfig configNext = RoleLevelConfig.Instance.GetData(role.Level + 1);
        int max = configNext != null ? configNext.Exp : role.Exp;
        if (max == 0)
            max = 1;
        float value = role.Exp / max;
        this._lvSlider.fillAmount = value;

        this._levelTxt.text = LanguageConfig.GetLanguage(LanMainDefine.RoleLevel, role.Level);
        this._expTxt.text = LanguageConfig.GetLanguage(LanMainDefine.RoleLevelExp, role.Exp, max);
    }


    private void OnSdk(UIButton btn)
    {
        
        //PopupFactory.Instance.ShowErrorNotice(ErrorCode.ValueOutOfRange, "", 1);
        SdkView.Intance.ShowSdk();
    }

    private void OnClickBuild(UIButton btn)
    {
        HomeLandManager.GetInstance().ConfirmBuild(false);
        MediatorUtil.ShowMediator(MediatorDefine.BUILD_CENTER);
    }

    private void OnClickHero(UIButton btn)
    {
        MediatorUtil.ShowMediator(MediatorDefine.HERO);
        UIRoot.Intance.SetHomeSceneEnable(false);
    }

    private void OnClickBag(UIButton btn)
    {
        MediatorUtil.ShowMediator(MediatorDefine.BAG);
    }

    public void setDianLiang()
    {
        var leftDianValue = SystemInfo.batteryLevel;
        if (leftDianValue < 0)
            leftDianValue = 0;
        this._dianLiang.fillAmount = leftDianValue;
    }


    public void UpdateIncome()
    {
        foreach (IncomeItem item in this._incomeList)
        {
            item.UpdateValue();
        }
    }

    public void JudgeIncome()
    {
        foreach (IncomeItem item in this._incomeList)
        {
            item.JudgeIncome();
        }
    }

    public void SetName()
    {
        this._head.SetData(RoleProxy._instance.Role.Head);
        this._nameTxt.text = RoleProxy._instance.Role.Name;
    }
}
