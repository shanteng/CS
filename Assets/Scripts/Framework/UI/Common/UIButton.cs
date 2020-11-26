using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public delegate void UIButtonDelegate(UIButton btnSelf);

public class UIButton : UIBase
{
    private Button _btn;
    private Text _text;
    private Image _icon;
    private Image _btnImg;
    private UIButtonDelegate _listener;
    private Material _oriMaterial;

    void Awake()
    {
        this._btnImg = this.gameObject.GetComponent<Image>();
        this._btn = this.gameObject.GetComponent<Button>();
        this._btn.onClick.AddListener(this.DoClick);
        Text[] txts =  transform.GetComponentsInChildren<Text>();
        if (txts != null && txts.Length > 0)
            this._text = txts[0];

        if(this._btnImg != null)
            this._oriMaterial = _btnImg.material;

        if (this.transform.Find("Icon") != null)
            this._icon = this.transform.Find("Icon").GetComponent<Image>();
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

            if (this._btnImg != null)
            {
                if (value == false)
                    UIRoot.Intance.SetImageGray(this._btnImg, true);
                else
                    this._btnImg.material = this._oriMaterial;
            }
               

            if (this._text != null)
            {
                Color orcolor = this._text.color;
                orcolor.a = value ? 1 : 0.5f;
                this._text.color = orcolor;
            }
        }
    }

    public Text Label => this._text;
    public Image Icon => this._icon;
}
