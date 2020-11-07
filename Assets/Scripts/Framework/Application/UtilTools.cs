
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

public class UtilTools
{
    public const float FloatPrecision = 0.0001f;
    public const float NFloatPrecision = -1 * FloatPrecision;
    public const float V3Precision = 0.0001f;

    private static object m_builderLock = new object();
    private static StringBuilder m_builder = new StringBuilder();

    public static string combine(params object[] texts)
    {
        lock (m_builderLock)
        {
            m_builder.Remove(0, m_builder.Length);
            var len = texts.Length;
            for (int i = 0; i < len; i++)
            {
                m_builder.Append(texts[i]);
            }
            return m_builder.ToString(0, m_builder.Length);
        }
    }

    public static string combine(string main, params object[] texts)
    {
        lock (m_builderLock)
        {
            m_builder.Remove(0, m_builder.Length);
            m_builder.Append(main);
            var len = texts.Length;
            for (int i = 0; i < len; i++)
            {
                m_builder.Append(texts[i]);
            }
            return m_builder.ToString(0, m_builder.Length);
        }
    }

    public static string combine(string main, int intParam)
    {
        lock (m_builderLock)
        {
            m_builder.Remove(0, m_builder.Length);
            m_builder.Append(main);
            m_builder.Append(intParam.ToString());
            return m_builder.ToString(0, m_builder.Length);
        }
    }

    public static string combine(string main, float floatParam)
    {
        lock (m_builderLock)
        {
            m_builder.Remove(0, m_builder.Length);
            m_builder.Append(main);
            m_builder.Append(floatParam.ToString());
            return m_builder.ToString(0, m_builder.Length);
        }
    }

    public static int ParseInt(string data)
    {
        if (data.Length == 0)
            return 0;
        int val = 0;
        if (int.TryParse(data, out val) == false)
        {
            return 0;
        }
        return val;
    }

    public static float ParseFloat(string data)
    {
        if (data.Length == 0)
            return 0;

        float val = 0;
        if (float.TryParse(data, out val) == false)
        {
            return 0;
        }
        return val;
    }

    public static string format(string valuestr, params object[] paramStrs)
    {
        string afterStr = "";
        try
        {
            afterStr = string.Format(valuestr, paramStrs);
        }
        catch (Exception ex)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(string.Format("Language: {0} 参数数量不匹配", valuestr));
#endif
        }
        return afterStr;
    }

    public static string NumberFormat(long nNum)
    {
        var sText = "";
        if (nNum < 100000)
            sText = nNum.ToString();
        else if (nNum >= 100000 && nNum < 100000000)
        {
            var wanNum = nNum / 10000;
            var wanFloorStr = (float)Math.Round((double)wanNum, 1);
            sText = LanguageConfig.GetLanguage(LanMainDefine.Wan, wanFloorStr);
        }
        else if (nNum >= 100000000)
        {
            var yiNum = nNum / 100000000;
            var yiFloor = (float)Math.Round((double)yiNum, 1);
            sText = LanguageConfig.GetLanguage(LanMainDefine.Yi, yiFloor);
        }
        return sText;
    }


    public static bool IsFloatZero(float v)
    {
        if (v > 0)
        {
            return v < V3Precision;
        }
        return v > NFloatPrecision;
    }

    public static bool IsFloatSimilar(float a, float b)
    {
        return IsFloatZero(a - b);
    }

    public static string GenerateUId()
    {
        byte[] buffer = Guid.NewGuid().ToByteArray();
        return BitConverter.ToInt64(buffer, 0).ToString();
    }

    public static long GetExpireTime(int passSecsFromNow)
    {
        return GameIndex.ServerTime + passSecsFromNow;
    }

    public static string GetCdStringExpire(long expire)
    {
        long leftSecs = expire - GameIndex.ServerTime;
        return GetCdString(leftSecs);

    }
    public static string GetCdString(long leftsecs)
    {
        if (leftsecs < 0)
            leftsecs = 0;

        var hour = leftsecs / 3600;
        var min = leftsecs % 3600 / 60;
        var sec = leftsecs % 60;
        if (hour > 0)
            return $"{hour:D2}:{min:D2}:{sec:D2}";
        return $"{min:D2}:{sec:D2}";
    }



    public static bool isFingerOverUI()
    {
        if (GameIndex.InGame == false)
            return false;
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
#if UNITY_EDITOR
        pointerEventData.position = Input.mousePosition;
#else
        if (Input.touchCount > 0)
            pointerEventData.position = Input.GetTouch(0).position;
#endif
        List<RaycastResult> results = new List<RaycastResult>();
        for (WindowLayer layer = WindowLayer.Main; layer < WindowLayer.Count; ++layer)
        {
            GraphicRaycaster graphicRaycaster = UIRoot.Intance.GetLayer(layer).gameObject.GetComponent<GraphicRaycaster>();
            graphicRaycaster.Raycast(pointerEventData, results);
            if (results != null && results.Count > 0)
                return true;
        }
        return false;
    }

    public static bool IsFingerOverBuilding(string key = "")
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
#if UNITY_EDITOR
        pointerEventData.position = Input.mousePosition;
#else
        if (Input.touchCount > 0)
            pointerEventData.position = Input.GetTouch(0).position;
#endif

        List<RaycastResult> results = new List<RaycastResult>();

        PhysicsRaycaster graphicRaycaster = Camera.main.gameObject.GetComponent<PhysicsRaycaster>();
        graphicRaycaster.Raycast(pointerEventData, results);
        if (results != null && results.Count > 0)
        {
            Vector3 rayPos =  results[0].worldPosition;
            int x = Mathf.CeilToInt(rayPos.x);
            int z = Mathf.CeilToInt(rayPos.z);
            BuildingData bd = WorldProxy._instance.GetBuildingInRange(x, z);
            return bd != null && bd._key.Equals(key);
        }

        return false;
    }


    public static bool SetCostList(List<CostItem> items, string[] Costs,bool NeedMy = false,float mutil = 1f)
    {
        bool isEnough = true;
        int len = Costs.Length;
        int count = items.Count;
        for (int i = 0; i < count; ++i)
        {
            if (i >= len)
            {
                items[i].Hide();
                continue;
            }
            CostData data = new CostData();
            data.Init(Costs[i],mutil);
            bool isStisfy = items[i].SetData(data, NeedMy);
            if (isStisfy == false)
                isEnough = false;
            items[i].Show();
        }//end for
        return isEnough;
    }//end func

    public static void SetCostList(List<CostBig> items, string[] Costs,bool needMy = false)
    {
        int len = Costs.Length;
        int count = items.Count;
        for (int i = 0; i < count; ++i)
        {
            if (i >= len)
            {
                items[i].Hide();
                continue;
            }
            CostData data = new CostData();
            data.Init(Costs[i]);
            items[i].SetData(data, needMy);
            items[i].Show();
        }//end for
    }//end func


    public static VInt2 WorldToGameCordinate(int x, int z)
    {
        VInt2 kv = new VInt2();
        kv.x = x + GameIndex.ROW / 2;
        kv.y = z + GameIndex.COL / 2;
        return kv;
    }

    public static VInt2 GameToWorldCordinate(int x, int z)
    {
        VInt2 kv = new VInt2();
        kv.x = x - GameIndex.ROW / 2;
        kv.y = z - GameIndex.COL / 2;
        return kv;
    }

    public static void ChangeLayer(GameObject obj, Layer layerValue)
    {
        Transform[] transArray = obj.GetComponentsInChildren<Transform>();
        foreach (Transform trans in transArray)
        {
            trans.gameObject.layer = (int)layerValue;
        }
    }

    public static List<T> GetRandomChilds<T>(List<T> list, int count)
    {
        List<T> tempList = new List<T>();
        tempList.AddRange(list);
        SortRandom<T>(tempList);
        return tempList.GetRange(0, count);
    }

    public static List<T> SortRandom<T>(List<T> list)
    {
        int randomIndex;
        for (int i = list.Count - 1; i > 0; i--)
        {
            randomIndex = UnityEngine.Random.Range(0, i);
            Swap(list,randomIndex, i);
        }
        return list;
    }

    public static void Swap<T>(List<T> list, int index1, int index2)
    {
        T temp = list[index2];
        list[index2] = list[index1];
        list[index1] = temp;
    }

    public static string GetPercentAddOn(int addon)
    {
        if (addon > 0)
            return LanguageConfig.GetLanguage(LanMainDefine.PercentAdd, addon);

        if (addon < 0)
            return LanguageConfig.GetLanguage(LanMainDefine.PercentDes, addon);

        return LanguageConfig.GetLanguage(LanMainDefine.PercentZeroAdd, addon);
    }

    public static int compareInt(int intA, int intB)
    {
        return intA - intB;
    }

    public static long compareLong(long intA, long intB)
    {
        return intA - intB;
    }

    public static int compareBool(bool intA, bool intB)
    {
        if (intA && !intB)
            return 1;
        else if (!intA && intB)
            return -1;
        return 0;
    }

    public static int compareString(string x, string y)
    {
        return x.CompareTo(y);
    }


    public static int compareFloat(float intA, float intB)
    {
        return Mathf.CeilToInt(intA - intB);
    }
}