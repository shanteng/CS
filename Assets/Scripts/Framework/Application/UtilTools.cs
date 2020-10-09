
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System;

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

    public static string format(string template, params object[] paramStrs)
    {
        string value = template;
        int count = paramStrs.Length;
        for (int i = 0; i < count; i++)
        {
            value = value.Replace(combine("{", i, "}"), paramStrs[i].ToString());
        }
        return value;
    }

    public static string GetLanguage(string key, params object[] paramName)
    {
        LanguageConfig config = LanguageConfig.Instance.GetData(key);
        if (config == null)
            return "";
        string valuestr = config.Value;
        try
        {
            valuestr = string.Format(config.Value, paramName);
        }
        catch (Exception ex)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(string.Format("LanguageConfig Key: {0} 参数数量不匹配", key));
#endif
        }

        return valuestr;
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

    public static long GetExpireTime(int neddSecs)
    {
        TimeSpan nowStep = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
         return Convert.ToInt64(nowStep.TotalSeconds + neddSecs);
    }
}