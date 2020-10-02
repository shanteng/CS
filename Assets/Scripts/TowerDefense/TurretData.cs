using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//保存每一波敌人生成所需属性
[Serializable]
public class TurretData
{
    public GameObject _prefab;
    public GameObject _prefabUpgrade;
    public int _cost;
    public int _costUpgrade;
    public TurretType _type;
}

public enum TurretType
{
    Tower1,
    Tower2,
    Tower3,
}
