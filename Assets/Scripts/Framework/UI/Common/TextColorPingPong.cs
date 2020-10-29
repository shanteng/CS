using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




public class TextColorPingPong : MonoBehaviour
{
    public Text _txt;
    public Color _orignlColor = new Color(0.38f, 0.52f, 0.75f);
    public Color _EndColor = new Color(0.3f, 0.5f, 0.9f, 1);

    public float _Speed = 1f;
    private Color _curColor;
    private bool _isStart = false;
    private float _delatTime = 0;

    void Awake()
    {
        this.Init();
    }

    private void Init()
    {
        if (this._txt == null)
            this._txt = this.GetComponent<Text>();
    }

    private void SetColor()
    {
        this.Init();
        this._txt.color = this._curColor;
    }

    public void SetEnable(bool isEn)
    {
        _delatTime = 0;
        this._isStart = isEn;
        _curColor = this._orignlColor;
        this.SetColor();
    }

    void Update()
    {
        if (this._isStart)
        {
            this._delatTime += Time.deltaTime * this._Speed;
            _curColor = Color.Lerp(this._orignlColor, this._EndColor, Mathf.PingPong(this._delatTime, 1));
            this.SetColor();
        }
    }
}

