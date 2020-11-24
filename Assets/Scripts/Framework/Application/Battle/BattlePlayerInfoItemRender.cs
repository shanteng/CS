using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class BattlePlayerInfoItemData : ScrollData
{
    public int _teamid;
    
    public BattlePlayerInfoItemData(int id)
    {
        this._teamid = id;
        this._Key = "Team";
    }
}

public class BattlePlayerInfoItemRender : ItemRender
{
    public HeroHead _heroUi;
    public Slider _blood;

    private int _id;
    public int ID => this._id;

    private void Start()
    {
        
    }

   
    protected override void setDataInner(ScrollData data)
    {
        BattlePlayerInfoItemData curData = (BattlePlayerInfoItemData)data;
        BattlePlayer player = BattleProxy._instance.GetPlayer(curData._teamid);
        this._id = curData._teamid;
        this._heroUi.SetData(player.HeroID);
        this._heroUi._levelTxt.text = LanguageConfig.GetLanguage(LanMainDefine.RoleLevel, player.Level);
        this.UpdateBlood();
    }

    public void UpdateBlood()
    {
        BattlePlayer player = BattleProxy._instance.GetPlayer(this.ID);
        float value = player.Attributes[AttributeDefine.Blood] / player.Attributes[AttributeDefine.OrignalBlood];
        this._blood.value = value;
        UIRoot.Intance.SetImageGray(this._heroUi._Icon, value <= 0);
    }
}


