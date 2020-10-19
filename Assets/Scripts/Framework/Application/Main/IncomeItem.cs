﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;


public class IncomeItem : MonoBehaviour
{
    public Text HourAdd;
    public Text CurMax;
    public Image Icon;

    private string _key;
    private int _oldValue = -1;

    void Awake()
    {
        this._key = this.gameObject.name;
        this.Icon.sprite = ResourcesManager.Instance.getAtlasSprite(AtlasDefine.Common, this.gameObject.name);
    }

    public void UpdateValue()
    {
        int hourAdd = RoleProxy._instance.GetHourInCome(this._key);
        this.HourAdd.text = LanguageConfig.GetLanguage(LanMainDefine.HourAdd, hourAdd);
        int maxLimit = RoleProxy._instance.Role.ValueLimit;
        int curValue = RoleProxy._instance.GetNumberValue(this._key);
        if(curValue < maxLimit)
            this.CurMax.text = LanguageConfig.GetLanguage(LanMainDefine.Progress, curValue,maxLimit);
        else
            this.CurMax.text = LanguageConfig.GetLanguage(LanMainDefine.ProgressFull, curValue,maxLimit);

        if (_oldValue > 0 && _oldValue != curValue)
        {
            this.CurMax.rectTransform.DOPunchScale(Vector3.one * 1.1f, 2f, 4, 0);
        }
        _oldValue = curValue;

    }
}


