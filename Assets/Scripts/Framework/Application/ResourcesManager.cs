
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine.U2D;
using System.Collections.Generic;
using Newtonsoft.Json.Utilities;

public class AtlasDefine
{
    public static string Common = "Common";//
    public static string Item = "Item";//
    public static string Army = "Army";//
    public static string Head = "Head";//
    public static string HeadFrame = "HeadFrame";//
    public static string Hero = "Hero";//
    public static string HeroCard = "HeroCard";//
    public static string HeroHead = "HeroHead";//
}
public class ResourcesManager : SingletonFactory<ResourcesManager>
{
    private string Atlas_Path = "Atlas/";
    private string UI_Path = "UI/";
    private string Popup_Path = "Popup/";
    private string Building = "Building/";
    private string Army = "Army/";
    private string Model = "Model/";
    private string Land = "Land/";

    private Dictionary<string, SpriteAtlas> _atlasDic = new Dictionary<string, SpriteAtlas>();
    public GameObject LoadUIRes(string resName)
    {
        string path = UtilTools.combine(UI_Path, resName);
        return Resources.Load<GameObject>(path);
    }

    public GameObject LoadLandRes(string resName)
    {
        string path = UtilTools.combine(Land, resName);
        return Resources.Load<GameObject>(path);
    }

    public GameObject LoadBuildingRes(string resName)
    {
        string path = UtilTools.combine(Building, resName);
        return Resources.Load<GameObject>(path);
    }

    public GameObject LoadArmyModel(string model)
    {
        string path = UtilTools.combine(Army, model);
        return Resources.Load<GameObject>(path);
    }

    public GameObject LoadModel(string model)
    {
        string path = UtilTools.combine(Model, model);
        return Resources.Load<GameObject>(path);
    }

    public GameObject LoadPopupRes(string resName)
    {
        string path = UtilTools.combine(Popup_Path, resName);
        return Resources.Load<GameObject>(path);
    }

    public Sprite getAtlasSprite(string sourceType, int id)
    {
        return getAtlasSprite(sourceType, id.ToString());
    }

    public Sprite getAtlasSprite(string atlasName, string spName)
    {
        var sAtlas = GetAtlas(atlasName);
        if (sAtlas == null)
            return null;
        var sprite = sAtlas.GetSprite(spName);
        return sprite;
    }

    public Sprite GetItemSprite(string itemKey)
    {
        ItemInfoConfig config = ItemInfoConfig.Instance.GetData(itemKey);
        if (config == null)
            return null;
        return this.getAtlasSprite(AtlasDefine.Item, config.Icon);
    }

    public Sprite GetHeroSprite(int id)
    {
        HeroConfig config = HeroConfig.Instance.GetData(id);
        if (config == null)
            return null;
        return this.getAtlasSprite(AtlasDefine.Hero, config.Model);
    }

    public Sprite GetHeroCardSprite(int id)
    {
        HeroConfig config = HeroConfig.Instance.GetData(id);
        if (config == null)
            return null;
        return this.getAtlasSprite(AtlasDefine.HeroCard, config.Model);
    }

    public Sprite GetHeroHeadSprite(int id)
    {
        HeroConfig config = HeroConfig.Instance.GetData(id);
        if (config == null)
            return null;
        return this.getAtlasSprite(AtlasDefine.HeroHead, config.Model);
    }


    public Sprite GetItemFrameSprite(string itemKey)
    {
        ItemInfoConfig config = ItemInfoConfig.Instance.GetData(itemKey);
        if (config == null)
            return null;
        return this.GetCommonFrame(config.Quality);
    }

    public Sprite GetHeadSprite(int head)
    {
        return this.getAtlasSprite(AtlasDefine.Head, head.ToString());
    }

    public Sprite GetCommonFrame(int Quality)
    {
        string icon = UtilTools.combine("ItemFrame", Quality);
        return this.getAtlasSprite(AtlasDefine.Common, icon);
    }

    public Sprite GetCommonSprite(string icon)
    {
        return this.getAtlasSprite(AtlasDefine.Common, icon);
    }

    public Sprite GetCareerIcon(int career)
    {
        string icon = UtilTools.combine("ca", career);
        return this.getAtlasSprite(AtlasDefine.Common, icon);
    }

    public SpriteAtlas GetAtlas(string atlasName)
    {
        SpriteAtlas atlas = null;
        if (!_atlasDic.TryGetValue(atlasName, out atlas))
        {
            string path = UtilTools.combine(Atlas_Path, atlasName);
            atlas =  Resources.Load<SpriteAtlas>(path);
            _atlasDic.Add(atlasName, atlas);
        }
        return atlas;
    }
}