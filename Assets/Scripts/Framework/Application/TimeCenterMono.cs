
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
/// <summary>
/// 单例工厂,使用单例的可以直接继承他即可
/// </summary>
/// 


public class TimeCenterMono : MonoBehaviour
{
    private static TimeCenterMono instance;
    private void Awake()
    {
        instance = this;
    }

    public static TimeCenterMono GetInstance()
    {
        return instance;
    }
    
}
