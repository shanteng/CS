﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public delegate void UIToggleDelegate(UIToggle btnSelf);

public class UIToggle : UIBase
{
    public Toggle _toggle;
    public Text _text;
    public Text _text2;
    private UIToggleDelegate _listener;

    void Awake()
    {
        this._toggle.onValueChanged.AddListener(this.DoClick);
    }

    public void DoClick(bool isOn)
    {
        if (this._listener != null)
            this._listener.Invoke(this);
    }

    public void AddEvent(UIToggleDelegate listener)
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
            return this._toggle.interactable;
        }
        set
        {
            this._toggle.interactable = value;
        }
    }

    public bool IsOn
    {
        get
        {
            return this._toggle.isOn;
        }
        set
        {
            this._toggle.SetIsOnWithoutNotify(value);
        }
    }

    public Text Label => this._text;
    public Text Label2 => this._text2;
}
