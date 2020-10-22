using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;

public class Hero
{
    public string UID;//唯一标识
    public int Id;
    public int Level;
    public int Exp;
    public List<AttributeData> Attributes;
    public float MarchSpeed;
    public float ElementValue;
    public int Blood;
    public int MaxBlood;

    public void ComputeAttributes()
    {
        HeroConfig config = HeroConfig.Instance.GetData(this.Id);
        HeroLevelConfig configLv = HeroLevelConfig.Instance.GetData(this.Level); 
        if (this.Attributes == null)
            this.Attributes = new List<AttributeData>();
        else
            this.Attributes.Clear();
        Dictionary<string, float> initValueDic = AttributeData.InitAttributes(config.InitAttribute);
        Dictionary<string, float> levelAddValueDic = AttributeData.InitAttributes(config.AttributeGrow);
        BuildingEffectsData bdAddData = WorldProxy._instance.GetBuildingEffects();
   
        foreach (string key in initValueDic.Keys)
        {
            float curAdd = 0;
            if (levelAddValueDic.TryGetValue(key, out curAdd))
            {
                float levelAdd = curAdd * (this.Level - 1);
                //等级增加的属性
                initValueDic[key] += levelAdd;
            }

            if (bdAddData.CareerAttrAdds[config.Career].TryGetValue(key, out curAdd))
            {
                //建筑属性增加
                initValueDic[key] += curAdd;
            }

            AttributeData data = new AttributeData();
            data.Id = key;
            data.Value = initValueDic[key];
            this.Attributes.Add(data);
        }//end for


        this.MarchSpeed = config.MarchSpeed + bdAddData.MarchSpeedAdd;
        this.ElementValue = config.ElementValue + bdAddData.ElementAdds[config.Element];
        this.MaxBlood = configLv.BloodMax + bdAddData.MaxBloodAdd;
    }//end function
}


public class HeroProxy : BaseRemoteProxy
{
    private Dictionary<string, Hero> _datas = new Dictionary<string, Hero>();
    public static HeroProxy _instance;
    public HeroProxy() : base(ProxyNameDefine.HERO)
    {
        _instance = this;
    }

    public Hero GetHero(string uid)
    {
        Hero hero = null;
        if (this._datas.TryGetValue(uid, out hero))
            return hero;
        return null;
    }

    public Dictionary<string, Hero> GeAllHeros()
    {
        return this._datas;
    }

    public void ComputeBuildingEffect()
    {
        var it = this._datas.Values.GetEnumerator();
        while (it.MoveNext())
        {
            Hero hero = it.Current;
            hero.ComputeAttributes();
        }
        it.Dispose();
        if (this._datas.Count > 0)
            this.SendNotification(NotiDefine.AllHeroUpdated);
    }

    public void LoadAllHeros()
    {
        this._datas.Clear();
        string fileName = UtilTools.combine(SaveFileDefine.Heros);
        string json = CloudDataTool.LoadFile(fileName);
        if (json.Equals(string.Empty) == false)
        {
            List<Hero> heros = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Hero>>(json);
            foreach (Hero hero in heros)
            {
                this._datas[hero.UID] = hero;
            }
        }

        this.SendNotification(NotiDefine.LoadAllHeroResp);
    }

    public void CreateOneHero(int id)
    {
        HeroConfig config = HeroConfig.Instance.GetData(id);
        Hero newHero = new Hero();
        newHero.UID = UtilTools.GenerateUId();
        newHero.Id = id;
        newHero.Level = 1;
        newHero.Exp = 0;
        newHero.ComputeAttributes();
        this._datas[newHero.UID] = newHero;
        this.SendNotification(NotiDefine.CreateHeroResp, newHero.UID);
        this.DoSaveHeros();
    }

    
    public void DoSaveHeros()
    {
        List<Hero> list = new List<Hero>();
        foreach (Hero data in this._datas.Values)
        {
            list.Add(data);
        }
        string fileName = UtilTools.combine(SaveFileDefine.Heros);
        CloudDataTool.SaveFile(fileName, list);
    }
}//end class
