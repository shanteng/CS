
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine.U2D;
using System.Collections.Generic;

public class AtlasDefine
{
    public static string Common = "Common";//

}
public class ResourcesManager : SingletonFactory<ResourcesManager>
{
    private string Atlas_Path = "Atlas/";
    private string UI_Path = "UI/";
    private string Popup_Path = "Popup/";

    private Dictionary<string, SpriteAtlas> _atlasDic = new Dictionary<string, SpriteAtlas>();
    public  GameObject LoadUIRes(string resName)
    {
        string path = UtilTools.combine(UI_Path, resName);
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