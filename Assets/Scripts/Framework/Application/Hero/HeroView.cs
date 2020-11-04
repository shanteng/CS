using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HeroView : MonoBehaviour
    ,IScollItemClickListener
{
    public DataGrid _hGrid;
    public List<UIToggle> _toggleList;
    public TextMeshProUGUI _countTxt;
    public HeroDetailsUI _detailsUi;
    private string _Element = "";
    private int _selectHeroId;
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

        string Element = (string)item._param._value;
        if (Element.Equals(this._Element))
            this._Element = "";
        else
            this._Element = Element;
        this.SetList();
    }

    public void InitData()
    {
        this._selectHeroId = 0;
        this._Element = "";
        this.SetList();
    }

    public void onClickScrollItem(ScrollData data)
    {
        if (data._Key.Equals("Hero"))
        {
            HeroItemRData curData = (HeroItemRData)data;
            this.SetDetails(curData._hero.Id);
        }
    }

    private void SetDetails(int id)
    {
        this._selectHeroId = id;
        foreach (ItemRender render in this._hGrid.ItemRenders)
        {
            HeroItemRender rd = (HeroItemRender)render;
            if (rd.gameObject.activeSelf == false)
                continue;
            rd._select.SetActive(id == rd.ID);
        }

        HeroConfig config = HeroConfig.Instance.GetData(id);
        HeroScene.GetInstance().SetModel(config.Model);
    }

    public void SetList()
    {
        foreach (UIToggle toggle in this._toggleList)
        {
            string curEle = (string)toggle._param._value;
            toggle.IsOn = curEle.Equals(this._Element);
        }

        Dictionary<int, Hero> dic = HeroProxy._instance.GeAllHeros();
        List<Hero> owns = new List<Hero>();
        List<Hero> others = new List<Hero>();

        _hGrid.Data.Clear();
        foreach (Hero hero in dic.Values)
        {
            HeroConfig config = HeroConfig.Instance.GetData(hero.Id);
            if (config.Element.Equals(this._Element) || this._Element.Equals(""))
            {
                if (hero.Belong == (int)HeroBelong.My)
                    owns.Add(hero);
                else
                    others.Add(hero);
            }
        }


        owns.Sort(this.Compare);
        others.Sort(this.Compare);

        this._countTxt.text = LanguageConfig.GetLanguage(LanMainDefine.Progress, owns.Count, dic.Count);

        foreach (Hero hero in owns)
        {
            HeroItemRData data = new HeroItemRData(hero);
            this._hGrid.Data.Add(data);
        }

        foreach (Hero hero in others)
        {
            HeroItemRData data = new HeroItemRData(hero);
            this._hGrid.Data.Add(data);
        }

        _hGrid.ShowGrid(this);
        this.SetDetails((this._hGrid.Data[0] as HeroItemRData)._hero.Id);
    }

    private int Compare(Hero a, Hero b)
    {
        HeroConfig aConfig = HeroConfig.Instance.GetData(a.Id);
        HeroConfig bConfig = HeroConfig.Instance.GetData(b.Id);

        int compare = UtilTools.compareInt(bConfig.Star, aConfig.Star);
        if (compare != 0)
            return compare;
        return UtilTools.compareInt(b.Level, a.Level);
    }
}
