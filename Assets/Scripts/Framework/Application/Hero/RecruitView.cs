using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RecruitView : MonoBehaviour
{
    public DataGrid _hGrid;
    public List<UIToggle> _toggleList;
    public CountDownText _cdTxt;
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
        this._Element = (string)item._param._value;
        this.SetList();
    }

    public void SetCity(int city)
    {
        this._city = city;
        this._Element = "All";
        MediatorUtil.SendNotification(NotiDefine.GetHeroRefreshDo);
        //this.SetList(this._Element);
    }

    public void OnFavorUp(int id)
    {
        foreach (ItemRender render in this._hGrid.ItemRenders)
        {
            RecruitItemRender rd = (RecruitItemRender)render;
            if (rd.gameObject.activeSelf == false)
                continue;
            if (rd.ID == id)
            {
                rd.FavorLevelChange();
                break;
            }
        }
    }

    public void SetList()
    {
        foreach (UIToggle toggle in this._toggleList)
        {
            string curEle = (string)toggle._param._value;
            toggle.IsOn = curEle.Equals(this._Element);
        }

        long expire = HeroProxy._instance.GetTervenExpire();
        _cdTxt.DoCountDown(expire, LanMainDefine.TavenRefresh);

        Dictionary<int, Hero> dic = HeroProxy._instance.GeAllHeros();
        _hGrid.Data.Clear();
        foreach (Hero hero in dic.Values)
        {
            HeroConfig config = HeroConfig.Instance.GetData(hero.Id);

            if (this._city == 0)
            {
                //我的酒馆并且在野的
                bool isInMyTarven = HeroProxy._instance.IsInMyTarvenHero(hero.Id);
                if (isInMyTarven == false)
                    continue;
            }
            else if (this._city != hero.Belong)
            {
                //不属于当前城市
                continue;
            }
               
            if (config.Element.Equals(this._Element) || this._Element.Equals("All"))
            {
                RecruitItemData data = new RecruitItemData(hero);
                this._hGrid.Data.Add(data);
            }
        }
        _hGrid.ShowGrid(null);
    }
}
