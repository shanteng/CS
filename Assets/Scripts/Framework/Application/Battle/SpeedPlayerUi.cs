using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SpeedPlayerUi : UIBase
{
    private RectTransform _rect;
    public Image _Head;
    private int _teamid;

    public int ID => this._teamid;
    private float _needSecs;
    private bool _DoMove = false;
    private Coroutine _cor;

    public void SetData(int id)
    {
        this._rect = this.GetComponent<RectTransform>();
        this._teamid = id;
        BattlePlayer player = BattleProxy._instance.GetPlayer(this._teamid);
        this._Head.sprite = ResourcesManager.Instance.GetHeroSmallSprite(player.HeroID);
        this._needSecs = player.ActionCountDown;
        this._rect.anchoredPosition = new Vector2(-750, 0);
        this.SetMove(false);
    }

    public void StartWaitMove()
    {
        this.SetMove(true);
        this._rect.anchoredPosition = new Vector2(-750, 0);
        if (this._cor != null)
        {
            StopCoroutine(this._cor);
            this._cor = null;
        }
        this._cor = StartCoroutine(MoveTo(new Vector2(750, 0)));
    }
    

    private IEnumerator MoveTo(Vector2 pos)
    {
        float t = 0;
        while (_DoMove)
        {
            t += Time.deltaTime;
            float a = t / this._needSecs;
            this._rect.anchoredPosition = Vector2.Lerp(new Vector2(-750, 0), pos, a);
            if (a >= 1.0f)
            {
                BattleProxy._instance.OnTeamBegin(this.ID);
                break;
            }
            yield return null;
        }
    }

    public void SetMove(bool isMove)
    {
        this._DoMove = isMove;
    }

   

   

}


