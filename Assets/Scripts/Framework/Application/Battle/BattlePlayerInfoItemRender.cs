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
    public List<BuffItemUi> _buffs;

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
        this._heroUi.SetData(player.HeroID, player.ArmyID);
        this._heroUi._levelTxt.text = LanguageConfig.GetLanguage(LanMainDefine.RoleLevel, player.Level);
        this.UpdateBlood();
        this.UpdateBuffs();
    }

    public void UpdateBlood()
    {
        BattlePlayer player = BattleProxy._instance.GetPlayer(this.ID);
        float value = player.Attributes[AttributeDefine.Blood] / player.Attributes[AttributeDefine.OrignalBlood];
        this._blood.value = value;
        UIRoot.Intance.SetImageGray(this._heroUi._Icon, value <= 0);
    }

    public void UpdateBuffs()
    {
        BattlePlayer player = BattleProxy._instance.GetPlayer(this.ID);
        foreach (BuffItemUi ui in this._buffs)
        {
            ui.Hide();
        }

        int index = 0;
        foreach (BattleEffectBuff buff in player._Buffs.Values)
        {
            //先全部显示，后面改为0round不显示
            if (buff.Duration < 0)
                continue;
            this._buffs[index].Show();
            this._buffs[index].SetData(buff);
            index++;
        }

    }
}


