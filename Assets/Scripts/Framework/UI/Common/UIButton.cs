﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public delegate void UIButtonDelegate(UIButton btnSelf);

public class UIButton : UIBase
{
    private Button _btn;
    private Text _text;
    private UIButtonDelegate _listener;

    void Awake()
    {
        this._btn = this.gameObject.GetComponent<Button>();
        this._btn.onClick.AddListener(this.DoClick);
        Text[] txts =  transform.GetComponentsInChildren<Text>();
        if (txts != null && txts.Length > 0)
            this._text = txts[0];
    }

   
    public void DoClick()
    {
        if (this._listener != null)
            this._listener.Invoke(this);
    }

    public void AddEvent(UIButtonDelegate listener)
    {
        this._listener = listener;
    }

    public void RemoveEvent()
    {
        this._listener = null;
    }

    public bool IsEnable
    {
        get
        {
            return this._btn.interactable;
        }
        set
        {
            this._btn.interactable = value;
        }
    }

    public Text Label => this._text;

}
