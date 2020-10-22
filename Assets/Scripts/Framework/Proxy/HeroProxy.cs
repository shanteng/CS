using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;


public class HeroProxy : BaseRemoteProxy
{
    private Dictionary<int, Hero> _datas = new Dictionary<int, Hero>();
    public static HeroProxy _instance;
    public HeroProxy() : base(ProxyNameDefine.HERO)
    {
        _instance = this;
    }

    public void DoSaveHeros()
    {
        List<Hero> list = new List<Hero>();
        foreach (Hero data in this._datas.Values)
        {
            list.Add(data);
        }
        CloudDataTool.SaveFile(SaveFileDefine.HeroDatas, this._datas);
    }

    public Hero GetHero(int id)
    {
        Hero hero = null;
        if (this._datas.TryGetValue(id, out hero))
            return hero;
        return null;
    }

    public Dictionary<int, Hero> GeAllHeros()
    {
        return this._datas;
    }

    public void ComputeBuildingEffect()
    {
        bool isChange = false;
        var it = this._datas.Values.GetEnumerator();
        while (it.MoveNext())
        {
            Hero hero = it.Current;
            if (hero.Belong == (int)HeroBelong.My)
            {
                isChange = true;
                hero.ComputeAttributes();
            }
              
        }
        it.Dispose();
        if (isChange)
            this.SendNotification(NotiDefine.HerosUpdated);
    }

    public void LoadAllHeros()
    {
        this._datas.Clear();
        string fileName = UtilTools.combine(SaveFileDefine.HeroDatas);
        string json = CloudDataTool.LoadFile(fileName);
        if (json.Equals(string.Empty) == false)
        {
           this._datas = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, Hero>>(json);
        }

        bool hasNewHero = false;
        Dictionary<int, HeroConfig> heroDic = HeroConfig.Instance.getDataArray();
        var it = heroDic.Values.GetEnumerator();
        while (it.MoveNext())
        {
            HeroConfig config = it.Current;
            if (this._datas.ContainsKey(config.ID))
                continue;
            hasNewHero = true;
            Hero newHero = new Hero();
            newHero.Create(config);
            this._datas[newHero.Id] = newHero;
        }
        it.Dispose();

        if (hasNewHero)
            this.DoSaveHeros();

        this.SendNotification(NotiDefine.LoadAllHeroResp);
    }

    public void ChangeHeroBelong(Dictionary<string, object> vo)
    {
        int id = (int)vo["id"];
        int belong = (int)vo["belong"];

        Hero hero = this.GetHero(id);
        if (hero == null) 
            return;
        int oldBelong = hero.Belong;
        if (oldBelong == belong)
            return;
        hero.Belong = belong;
        hero.Blood = 0;//变更所有权后兵力归零
        if (belong == (int)HeroBelong.My)
            this.SendNotification(NotiDefine.GetHeroNoti, id);
        else 
            this.SendNotification(NotiDefine.LosetHeroNoti, id);

        this.DoSaveHeros();
    }


}//end class
