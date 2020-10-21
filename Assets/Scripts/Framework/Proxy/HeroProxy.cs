using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;

public class Hero
{
    public string UID;//唯一标识
    public int Id;
    public int Level;
    public int Exp;
    
    public List<AttributeData> Attributes;
    public float MarchSpeed;
    public float ElementValue;
}

public class HeroProxy : BaseRemoteProxy
{
    public static HeroProxy _instance;
    public HeroProxy() : base(ProxyNameDefine.HERO)
    {
        _instance = this;
    }

   
}//end class
