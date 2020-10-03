#if UNITY_EDITOR
using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ExcelToJsonTool))]
public class ExcelToJsonToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Excel转Json"))
        {
            this.DoAllXlsxToJson();
        }
        //扩展Inspector
    }

    void DoAllXlsxToJson()
    {
        ExcelToJsonTool excels = target as ExcelToJsonTool;
        string dataPath = UnityEngine.Application.dataPath;
        foreach (ExcelConfig excelName in excels._jsonConfigs)
        {
            this.DoXlsxToJson(excelName);
        }
    }

    public  void DoXlsxToJson(ExcelConfig config)
    {
        string dataPath = System.Environment.CurrentDirectory;
        // xlsx路径
        string xlsxPath = dataPath + "/Excel/" + config.name + ".xlsx";
        string savePath = Application.dataPath + "/Resources/Config/";


        FileStream stream = null;
        try
        {
            stream = File.Open(xlsxPath, FileMode.Open, FileAccess.Read);
        }
        catch (IOException e)
        {
            UnityEngine.Debug.LogFormat("关闭xlsx文件后重试！");
        }

        if (stream == null)
            return;
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        DataSet result = excelReader.AsDataSet();
        UnityEngine.Debug.Log(result.DataSetName);
        // 读取
        foreach (ExcelSheetClass exSheet in config.sheets)
        {
            ReadSingleSheet(exSheet.classname, result.Tables[exSheet.sheet], savePath+exSheet.sheet+".json");
        }
        
    }

    private static void ReadSingleSheet(string typeName, DataTable dataTable, string jsonPath)
    {
        Type type =  Activator.CreateInstance(Type.GetType(typeName, true)).GetType();// as LoaderBase;


        int rows = dataTable.Rows.Count;
        int Columns = dataTable.Columns.Count;
        // 工作表的行数据
        DataRowCollection collect = dataTable.Rows;
        // xlsx对应的数据字段，规定是第二行
        // xlsx对应的数据字段類型，规定是第3行
        string[] jsonFileds = new string[Columns];
        string[] jclassTypes = new string[Columns];
        for (int i = 0; i < Columns; i++)
        {
            jsonFileds[i] = collect[1][i].ToString();
            jclassTypes[i] = collect[2][i].ToString();
        }

        // 要保存成Json的obj
        List<object> objsToSave = new List<object>();
        // 从第三行开始
        for (int i = 3; i < rows; i++)
        {
            System.Object objIns = type.Assembly.CreateInstance(type.ToString());
            for (int j = 0; j < Columns; j++)
            {
                FieldInfo field = type.GetField(jsonFileds[j]);
                if (field == null)
                    continue;
                object value = null;

                string classType = jclassTypes[j];
                try // 赋值
                {
                    switch (classType)
                    {
                        case "String":
                            value = Convert.ChangeType(collect[i][j], typeof(string));
                            break;
                        case "Int":
                            value = Convert.ChangeType(collect[i][j], typeof(int));
                            break;
                        case "ArrayInt":
                            string str = collect[i][j].ToString();
                            string[] strs = str.Split(',');
                            int[] ints = new int[strs.Length];
                            for (int k = 0; k < strs.Length; k++)
                            {
                                ints[k] = int.Parse(strs[k]);
                            }
                            value = ints;
                            break;
                        case "ArrayString":
                            string strString = collect[i][j].ToString();
                            value = strString.Split(',');
                            break;
                    }
                }
                catch (InvalidCastException e) // 一般到这都是Int数组，当然还可以更细致的处理不同类型的数组
                {
                    Debug.LogError("未定义的ClassType：" + classType);
                }
                field.SetValue(objIns, value);
            }//end for j

            objsToSave.Add(objIns);
        }//end for
        // 保存为Json
        string content = Newtonsoft.Json.JsonConvert.SerializeObject(objsToSave);
        SaveFile(content, jsonPath);

    }//end func

    private static void SaveFile(string content, string jsonPath)
    {
        StreamWriter streamWriter;
        FileStream fileStream;
        if (File.Exists(jsonPath))
        {
            File.Delete(jsonPath);
        }
        fileStream = new FileStream(jsonPath, FileMode.Create);
        streamWriter = new StreamWriter(fileStream);
        streamWriter.Write(content);
        streamWriter.Close();
    }
}

#endif
