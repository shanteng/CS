using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RecruitView : MonoBehaviour
{
    public DataGrid _hGrid;
    public List<UIToggle> _toggleList;
    private int _city = 0;
    private string _Element = "All";
    void Awake()
    {
        foreach (UIToggle item in this._toggleList)
        {
            item._param._value = item.gameObject.name;
            item.AddEvent(OnSelectToggle);
        }
    }

    private void OnSelectToggle(UIToggle btnSelf)
    {
        UIToggle item = (UIToggle)btnSelf;
        this.SetList((string)item._param._value);
    }

    public void SetCity(int city)
    {
        this._city = city;
        this._Element = "All";
        this.SetList(this._Element);
    }

    private void SetList(string element)
    {
        this._Element = element;
        foreach (UIToggle toggle in this._toggleList)
        {
            string curEle = (string)toggle._param._value;
            toggle.IsOn = curEle.Equals(element);
        }

        Dictionary<int, Hero> dic = HeroProxy._instance.GeAllHeros();
        _hGrid.Data.Clear();
        foreach (Hero hero in dic.Values)
        {
            HeroConfig config = HeroConfig.Instance.GetData(hero.Id);
            if (config.Element.Equals(this._Element) || this._Element.Equals("All"))
            {
                RecruitItemData data = new RecruitItemData(hero);
                this._hGrid.Data.Add(data);
            }
        }
        _hGrid.ShowGrid(null);
    }
}
