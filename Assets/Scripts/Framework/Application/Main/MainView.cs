using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.PlayerIdentity;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainView : MonoBehaviour
{
    public Text _nameTxt;
    public UIButton _BtnBuild;
    public UIButton _BtnSdk;
    public InfoCanvas _InfoUI;
    public Image _dianLiang;
    public Text _timeTxt;
    public List<IncomeItem> _incomeList;
    public Text _goldTxt;
    public Text _goldHourAddTxt;
    public Text _goldAddTxt;

    private int _dianLiangCount = 60;
    private void Awake()
    {
        _goldAddTxt.gameObject.SetActive(false);
    }
    void Start()
    {
        _dianLiangCount = 60;
        _BtnBuild.AddEvent(this.OnClickBuild);
        _BtnSdk.AddEvent(this.OnSdk);
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

    private void OnSdk(UIButton btn)
    {
        UIRoot.Intance._SdkView.SetActive(true);
    }

    private void OnClickBuild(UIButton btn)
    {
        HomeLandManager.GetInstance().ConfirmBuild(false);
        MediatorUtil.ShowMediator(MediatorDefine.BUILD_CENTER);
    }

    public void setDianLiang()
    {
        var leftDianValue = SystemInfo.batteryLevel;
        if (leftDianValue < 0)
            leftDianValue = 0;
        this._dianLiang.fillAmount = leftDianValue;
    }

    private int _oldValue = -1;
    public void UpdateIncome()
    {
        foreach (IncomeItem item in this._incomeList)
        {
            item.UpdateValue();
        }

        int curValue = RoleProxy._instance.GetNumberValue(ItemKey.gold);
        this._goldTxt.text = UtilTools.NumberFormat(curValue);
        int hourAdd = RoleProxy._instance.GetHourInCome(ItemKey.gold);
        this._goldHourAddTxt.text = LanguageConfig.GetLanguage(LanMainDefine.HourAdd, hourAdd);
        if (_oldValue > 0 && _oldValue != curValue)
        {
            int add = curValue - this._oldValue;
            if (add > 0)
                this._goldAddTxt.text = UtilTools.combine("+", add);
            else
                this._goldAddTxt.text = add.ToString();
            this._goldAddTxt.gameObject.SetActive(true);

            this._goldTxt.rectTransform.DOPunchScale(Vector3.one * 1.1f, 2f, 2, 0).onComplete = () =>
            {
                this._goldAddTxt.gameObject.SetActive(false);
            };


        }
        _oldValue = curValue;
    }

    public void SetName()
    {
        this._nameTxt.text = RoleProxy._instance.Role.Name;
    }
}
