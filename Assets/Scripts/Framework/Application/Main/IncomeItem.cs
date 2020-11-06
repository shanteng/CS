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

    private string _key;
    private int _oldValue = -1;

    void Awake()
    {
        this._AddValue.gameObject.SetActive(false);
        this._key = this.gameObject.name;
        this.Icon.sprite = ResourcesManager.Instance.getAtlasSprite(AtlasDefine.Common, this.gameObject.name);
    }

    public void UpdateValue()
    {
       // int hourAdd = RoleProxy._instance.GetHourInCome(this._key);
    //    this.HourAdd.text = LanguageConfig.GetLanguage(LanMainDefine.HourAdd, hourAdd);
      
        int curValue = RoleProxy._instance.GetNumberValue(this._key);
        string curStr = UtilTools.NumberFormat(curValue);
        this.CurMax.text = curStr;

        if (_oldValue > 0 && curValue > _oldValue)
        {
            int add = curValue - this._oldValue;
            this._AddValue.text = UtilTools.combine("+", add);
            this._AddValue.gameObject.SetActive(true);
            this.CurMax.rectTransform.DOPunchScale(Vector3.one * 1.1f, 2f, 2, 0).onComplete = () =>
            {
                this._AddValue.gameObject.SetActive(false);
            };
        }
        _oldValue = curValue;
    }

    
}


