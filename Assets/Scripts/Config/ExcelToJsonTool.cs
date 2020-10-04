
using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ExcelConfig
{
    public string name;
    public List<string> sheets;
}

[Serializable]
public class ExcelToJsonTool : MonoBehaviour
{
    public List<ExcelConfig> _Excels;
}



