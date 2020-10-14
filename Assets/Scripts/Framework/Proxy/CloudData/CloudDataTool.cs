﻿
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
    public const string WorldBuiding = "WorldBuiding";
    public const string WorldSpot = "WorldSpot";
}

public class CloudDataTool
{
    public static void SaveFile(string filename,object obj)
    {
        string jsonPath = Application.persistentDataPath + "/" + filename + ".json";
        string content = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

     /*   StreamWriter streamWriter;
        FileStream fileStream;
        if (File.Exists(jsonPath))
            File.Delete(jsonPath);
        fileStream = new FileStream(jsonPath, FileMode.Create);
        streamWriter = new StreamWriter(fileStream);
        streamWriter.Write(content);
        streamWriter.Close();
     */
        //同步云端
        UIRoot.Intance.SaveToCloud(filename, content);
    }

    public static string LoadFile(string filename)
    {
        //直接从云端读取

        string str = UIRoot.Intance.LoadCloudData(filename);
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
    public List<BuildingData> Datas;
}

public class WorldCanOprateSpot
{
    public int World;
    public List<string> Dpots;
}