using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class TeamView : MonoBehaviour
    ,IScollItemClickListener
{
    public DataGrid _CityGrid;
    public DataGrid _TeamGrid;

    public TextMeshProUGUI _CordinateTxt;
    private VInt2 Goto;
    private int _curCityId = 0;
    void Awake()
    {
        this._CordinateTxt.GetComponent<UIClickHandler>().AddListener(OnEndClick);
    }

    private void OnEndClick(object param)
    {
        ViewControllerLocal.GetInstance().TryGoto(this.Goto);
    }

    public void SetList(int city = 0)
    {
        Dictionary<int, CityData> dic = WorldProxy._instance.AllCitys;
        _CityGrid.Data.Clear();
        int index = 0;
        int i = 0;
        foreach (CityData cityInfo in dic.Values)
        {
            if (cityInfo.IsOwn)
            {
                TeamCityItemData data = new TeamCityItemData(cityInfo);
                this._CityGrid.Data.Add(data);
                if (cityInfo.ID == city)
                    index = i;
                i++;
            }
        }
        this._CityGrid.ShowGrid(this,index);
        this.SetCity(city);
    }

    public void UpdateTeamList()
    {
        foreach (ItemRender render in this._TeamGrid.ItemRenders)
        {
            render.updateData();
        }
    }

    public void onClickScrollItem(ScrollData data)
    {
        if (data._Key.Equals("TeamCityItemData"))
        {
            TeamCityItemData cityData = (TeamCityItemData)data;
            if (cityData._info.ID == this._curCityId)
                return;
            this.SetCity(cityData._info.ID);
        }
    }

    private void SetCity(int city)
    {
        foreach (ItemRender render in this._CityGrid.ItemRenders)
        {
            TeamCityItemRender rd = (TeamCityItemRender)render;
            if (rd.gameObject.activeSelf == false)
                continue;
            rd.m_renderData._IsSelect = rd.ID.Equals(city);
            rd.SetSelectState();
        }

        this._curCityId = city;
        List<Team> list = TeamProxy._instance.GetCityTeams(this._curCityId);
        _TeamGrid.Data.Clear();
        int upCount = 0;
        foreach (Team t in list)
        {
            ScrollData data = new ScrollData();
            data._Param = t.Id;
            if (t.HeroID > 0)
                upCount++;
            this._TeamGrid.Data.Add(data);
        }
        this._TeamGrid.ShowGrid(null);

        Goto = WorldProxy._instance.GetCityCordinate(this._curCityId);
        VInt2 gamePos = UtilTools.WorldToGameCordinate(Goto.x, Goto.y);
        this._CordinateTxt.text =  LanguageConfig.GetLanguage(LanMainDefine.SpotCordinate, gamePos.x, gamePos.y);
    }
}
