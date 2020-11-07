using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;


public class IncomeItem : MonoBehaviour
{
    public Text CurMax;
    public Image Icon;
    public Text _AddValue;
    public UIButton _btnAccept;

    public string _key;
    private int _oldValue = -1;
    private int _needValueShow = 0;
    void Awake()
    {
        this._btnAccept.AddEvent(OnClickAccept);
        this._AddValue.gameObject.SetActive(false);
        this._key = this.gameObject.name;
        this.Icon.sprite = ResourcesManager.Instance.getAtlasSprite(AtlasDefine.Common, this.gameObject.name);
    }

    private void OnClickAccept(UIButton btn)
    {
        MediatorUtil.SendNotification(NotiDefine.AcceptHourAwardDo, this._key);
    }

    public void JudgeIncome()
    {
        int value = RoleProxy._instance.GetCanAcceptIncomeValue(this._key);
        bool isShow = value >= this._needValueShow;
        this._btnAccept.gameObject.SetActive(isShow);
    }

    public void UpdateValue()
    {
        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.IncomeShowValue);
        _needValueShow = cfgconst.IntValues[0];
       

        int curValue = RoleProxy._instance.GetNumberValue(this._key);
        string curStr = UtilTools.NumberFormat(curValue);

        if (_oldValue > 0 && curValue > _oldValue)
        {
            int add = curValue - this._oldValue;
            this._AddValue.text = UtilTools.combine("+", add);
            this._AddValue.gameObject.SetActive(true);
        }

        this.CurMax.DOKill();
        this.CurMax.DOText(curStr, 1f).onComplete = () =>
        {
            this.CurMax.rectTransform.localScale = Vector3.one;
            this._AddValue.gameObject.SetActive(false);
        };

    
        _oldValue = curValue;
        this.JudgeIncome();
    }

    
}


