using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class BattleSkillItem : UIBase
{
    public SkillItemUi _Ui;
    public UIButton _btnCheck;
    private int _id;
    private int _level;
    public int ID => this._id;
    private void Awake()
    {
        this._btnCheck.AddEvent(OnCheck);
    }

    private void OnCheck(UIButton btn)
    {
        BattleSkill data = new BattleSkill();
        data.ID = this._id;
        data.Level = this._level;
        PopupFactory.Instance.ShowSkill(data);
    }

    public void SetData(int id,int level)
    {
        this._id = id;
        this._level = level;
        this._Ui.SetData(id,level);
    }
}


