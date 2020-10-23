using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RecruitView : MonoBehaviour
{
    public DataGrid _hGrid;

    void Awake()
    {
      
    }

    public void SetList()
    {
        Dictionary<int, Hero> dic = HeroProxy._instance.GeAllHeros();
        _hGrid.Data.Clear();
        foreach (Hero hero in dic.Values)
        {
            RecruitItemData data = new RecruitItemData(hero);
            this._hGrid.Data.Add(data);
        }
        _hGrid.ShowGrid(null);
    }
}
