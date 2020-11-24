using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PreBattleUi : MonoBehaviour
{
    public DataGrid _hGrid;
    public UIButton _btnFight;
    void Awake()
    {
        this._btnFight.AddEvent(OnFight);
    }

    private void OnFight(UIButton btn)
    {
        BattleProxy._instance.StartFight();
    }

    public void SetList()
    {
        Dictionary<int, BattlePlayer> dic = BattleProxy._instance.Data.Players;
        _hGrid.Data.Clear();

        foreach (BattlePlayer pl in dic.Values)
        {
            if (pl.TeamID < 0)
                continue;
            BattlePlayerUpItemData data = new BattlePlayerUpItemData(pl.TeamID);
            this._hGrid.Data.Add(data);
        }
        _hGrid.ShowGrid(null);
    }

    public void UpdateList()
    {
        foreach (ItemRender rd in this._hGrid.ItemRenders)
        {
            BattlePlayerUpItemRender item = (BattlePlayerUpItemRender)rd;
            if (item.gameObject.activeSelf == false)
                continue;
            item.UpdateState();
        }
    }

}
