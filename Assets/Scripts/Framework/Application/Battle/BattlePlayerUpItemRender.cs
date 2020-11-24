using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class BattlePlayerUpItemData : ScrollData
{
    public int _teamid;
    
    public BattlePlayerUpItemData(int id)
    {
        this._teamid = id;
        this._Key = "Team";
    }
}

public class BattlePlayerUpItemRender : ItemRender
{
    public BattleTeamRender _teamUi;
    public UIButton _btnUp;
    public UIButton _btnDown;
    public GameObject _MaskUp;
    private int _id;

    public int ID => this._id;

    private void Start()
    {
        this._btnUp.AddEvent(OnClickUp);
        this._btnDown.AddEvent(OnClickDown);
    }

    private void OnClickUp(UIButton btn)
    {
        BattleController.Instance.SetSelctBornTeam(this.ID);
    }

    private void OnClickDown(UIButton btn)
    {
        BattleController.Instance.UnSetSelctBornTeam(this.ID);
    }

    protected override void setDataInner(ScrollData data)
    {
        BattlePlayerUpItemData curData = (BattlePlayerUpItemData)data;
        BattlePlayer player = BattleProxy._instance.GetPlayer(curData._teamid);
        this._id = curData._teamid;
        this._teamUi.SetMyTeam(this.ID);
        this.UpdateState();
    }

    public void UpdateState()
    {
        BattlePlayer player = BattleProxy._instance.GetPlayer(this.ID);
        this._MaskUp.SetActive(player.BornIndex > 0);
        this._btnUp.gameObject.SetActive(player.BornIndex == 0);
        this._btnDown.gameObject.SetActive(player.BornIndex > 0);
    }
}


