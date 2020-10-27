
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
            if (results.Count > 0)
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
        if (results.Count > 0 && key.Equals("") == false)
        {
            return results[0].gameObject.name.Equals(key);
        }
        return results.Count > 0;
    }


    public static bool SetCostList(List<CostItem> items, string[] Costs,bool NeedMy = false)
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
            data.Init(Costs[i]);
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
}