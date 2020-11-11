
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;



public class SaveFileDefine
{
    public const string Role = "Role";
    public const string WorldBuiding = "WorldBuiding";
    public const string WorldSpot = "WorldSpot";
    public const string HeroDatas = "HeroDatas";
    public const string Army = "Army";
    public const string HeroRecruitRefresh = "HeroRecruitRefresh";
    public const string Team = "Team";
    public const string VisibleSpot = "VisibleSpot";
    public const string Patrol = "Patrol";
    public const string Citys = "Citys";
    public const string QuestCity = "QuestCity";
}

public class CloudDataTool
{
    public static void SaveFile(string filename,object obj)
    {
        string content = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        SdkView.Intance.SaveToCloud(filename, content);
        /*
        string jsonPath = Application.persistentDataPath + "/" + filename + ".json";   
        StreamWriter streamWriter;
           FileStream fileStream;
           if (File.Exists(jsonPath))
               File.Delete(jsonPath);
           fileStream = new FileStream(jsonPath, FileMode.Create);
           streamWriter = new StreamWriter(fileStream);
           streamWriter.Write(content);
           streamWriter.Close();
        */
        //同步云端

    }

    public static string LoadFile(string filename)
    {
        //直接从云端读取

        string str = SdkView.Intance.LoadCloudData(filename);
        if (str == null)
            return string.Empty;
        return str;
        /*string path = Application.persistentDataPath + "/" + filename + ".json";
        if (!File.Exists(path))
            return string.Empty;

        StreamReader reader = new StreamReader(path);
        string jsonData = reader.ReadToEnd();
        reader.Close();
        reader.Dispose();
        return jsonData;
        */
    }
}


public class WorldBuildings
{
    public int World;
    public string MainCityKey;
    public VInt2 StartCordinates;//出生地中心点坐标
    public Dictionary<int, List<BuildingData>> Datas;//key 城市ID 0-主城
}

public class WorldCanOprateSpot
{
    public int World;
    public List<string> Dpots;
}