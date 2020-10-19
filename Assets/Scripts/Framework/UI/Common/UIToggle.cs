using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public delegate void UIToggleDelegate(UIToggle btnSelf);

public class UIToggle : UIBase
{
    private Toggle _toggle;
    private Text _text;
    private UIToggleDelegate _listener;

    void Awake()
    {
        this._toggle = this.gameObject.GetComponent<Toggle>();
        this._toggle.onValueChanged.AddListener(this.DoClick);

        Text[] txts = transform.GetComponentsInChildren<Text>();
        if (txts != null && txts.Length > 0)
            this._text = txts[0];
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
            this._toggle.isOn = value;
        }
    }

    public Text Label => this._text;

}
