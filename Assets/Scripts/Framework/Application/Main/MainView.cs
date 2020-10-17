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

    private int _dianLiangCount = 60;
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

    public void UpdateIncome()
    {
        foreach (IncomeItem item in this._incomeList)
        {
            item.UpdateValue();
        }
        this._goldTxt.text = UtilTools.NumberFormat(RoleProxy._instance.GetNumberValue(ItemKey.gold));
    }

    public void SetName()
    {
        this._nameTxt.text = RoleProxy._instance.Role.Name;
    }
}
