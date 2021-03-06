﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;



public class CareerRate : UIBase
{
    public Image _bg;
    public Image _Icon;
    public TextMeshProUGUI _rateTxt;

    public  void SetData(int career,int rate)
    {
        bool isUnSet = rate == 0;
        this._Icon.sprite = ResourcesManager.Instance.GetCareerIcon(career);
        this._rateTxt.text = Hero.GetCareerEvaluateName(rate);
        this._rateTxt.gameObject.SetActive(isUnSet == false);
        UIRoot.Intance.SetImageGray(this._Icon, isUnSet);
        if(this._bg != null)
            UIRoot.Intance.SetImageGray(this._bg, isUnSet);
    }

    
}


