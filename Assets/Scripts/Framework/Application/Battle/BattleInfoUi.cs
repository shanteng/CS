using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class BattleInfoUi : MonoBehaviour
{
    public DataGrid _hGrid;
    public Slider _bloodSlider;
    public TextMeshProUGUI _bloodTxt;

    public CallSkillUi _callSkillUi;

    private List<int> _players;
    void Awake()
    {

    }

    public void HideCallSkill()
    {
        this._callSkillUi.HideAnimation();
    }

    public void SetList(List<int> players)
    {
        this.HideCallSkill();
        this._players = new List<int>();
        _hGrid.Data.Clear();
        foreach (int pl in players)
        {
            BattlePlayer player = BattleProxy._instance.GetPlayer(pl);
            if (player.BornIndex > 0)
            {
                _players.Add(pl);
                BattlePlayerInfoItemData data = new BattlePlayerInfoItemData(pl);
                this._hGrid.Data.Add(data);
            }
            
        }
        _hGrid.ShowGrid(null);
        this.UpdateBlood();
    }

    public void UpdateBlood()
    {
        float totle = 0;
        float cur = 0;
        foreach (int pl in this._players)
        {
            BattlePlayer player = BattleProxy._instance.GetPlayer(pl);
            totle += player.Attributes[AttributeDefine.OrignalBlood];
            cur += player.Attributes[AttributeDefine.Blood];
        }

        if (totle > 0)
            this._bloodSlider.value = cur / totle;
        else
            this._bloodSlider.value = 0f;
        this._bloodTxt.text = LanguageConfig.GetLanguage(LanMainDefine.Progress, cur, totle);
    }

    public void UpdateList()
    {
        foreach (ItemRender rd in this._hGrid.ItemRenders)
        {
            BattlePlayerInfoItemRender item = (BattlePlayerInfoItemRender)rd;
            if (item.gameObject.activeSelf == false)
                continue;
            item.UpdateBlood();
        }
    }

    public void UpdateBuffs()
    {
        foreach (ItemRender rd in this._hGrid.ItemRenders)
        {
            BattlePlayerInfoItemRender item = (BattlePlayerInfoItemRender)rd;
            if (item.gameObject.activeSelf == false)
                continue;
            item.UpdateBuffs();
        }
    }

}
