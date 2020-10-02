
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.Diagnostics;
using System.IO;

public class ResourcesManager : SingletonFactory<ResourcesManager>
{
    private string UI_Path = "UI/";
    public  GameObject LoadUIRes(string resName)
    {
        string path = UtilTools.combine(UI_Path, resName);
        return Resources.Load<GameObject>(path);
    }
}