﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SpeedPlayerUi : UIBase
{
    private RectTransform _rect;
    public Image _Head;
    public TextMeshProUGUI _SpeedTxt;
    private int _teamid;

    public int ID => this._teamid;
    private float _needSecs;
    private bool _DoMove = false;
    private Coroutine _cor;
    public static float MoveX = 500;

    public void SetData(int id)
    {
        this._rect = this.GetComponent<RectTransform>();
        this._teamid = id;
        BattlePlayer player = BattleProxy._instance.GetPlayer(this._teamid);
        this._Head.sprite = ResourcesManager.Instance.GetHeroSmallSprite(player.HeroID);
        this.UpdateSpeed();
        this._rect.anchoredPosition = new Vector2(-MoveX, 0);
        this.SetMove(false);
    }

    public void UpdateSpeed()
    {
        BattlePlayer player = BattleProxy._instance.GetPlayer(this._teamid);
        this._needSecs = player.GetActionCountDown();
        this._SpeedTxt.text = player.GetCurrentSpeed().ToString();
    }

    public void StartWaitMove()
    {
        this._rect.anchoredPosition = _fromPos;
        this.MoveSces = 0;
        this.SetMove(true);
    }

    float MoveSces = 0;
    Vector2 _targetPos = new Vector2(MoveX, 0);
    Vector2 _fromPos = new Vector2(-MoveX, 0);
    private void Update()
    {
        if (_DoMove == false)
            return;
        MoveSces += Time.deltaTime;
        float a = MoveSces / this._needSecs;
        this._rect.anchoredPosition = Vector2.Lerp(_fromPos, _targetPos, a);
        if (a >= 1.0f)
        {
            this._DoMove = false;
            this.MoveSces = 0;
            BattleProxy._instance.OnPlayerStartAction(this.ID);
        }
    }


    public void SetMove(bool isMove)
    {
        this._DoMove = isMove;
    }

   

   

}


