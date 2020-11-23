using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuestCityPop : Popup
{
    public Text _titleTxt;
    public DataGrid _vGrid;
    int Target;
    public override void setContent(object data)
    {
        Target = (int)data;
        string cityname = WorldProxy._instance.GetCityName(Target);
        this._titleTxt.text = LanguageConfig.GetLanguage(LanMainDefine.QuestTarget, cityname);
        this.SetList();
    }

    private void SetList()
    {
        Dictionary<int, Hero> dic = HeroProxy._instance.GetAllHeros();
        _vGrid.Data.Clear();
        List<Hero> heros = new List<Hero>();
        foreach (Hero hero in dic.Values)
        {
            if (hero.DoingState != (int)HeroDoingState.Idle || hero.IsMy == false)
                continue;
            int inTeamID = TeamProxy._instance.GetHeroTeamID(hero.Id);
            if (inTeamID > 0)
                continue;
            heros.Add(hero);
        }//end for city
        heros.Sort(this.Compare);

        foreach (Hero hero in heros)
        {
            QuesttemData data = new QuesttemData(this.Target, hero.Id);
            this._vGrid.Data.Add(data);
        }
        _vGrid.ShowGrid();
    }

    private int Compare(Hero a, Hero b)
    {
        HeroConfig aConfig = HeroConfig.Instance.GetData(a.Id);
        HeroConfig bConfig = HeroConfig.Instance.GetData(b.Id);

        int compare = UtilTools.compareInt(a.City, b.City);
        if (compare != 0)
            return compare;
        compare = UtilTools.compareFloat(bConfig.Speed, aConfig.Speed);
        return compare;
    }

}
