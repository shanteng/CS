using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//保存每一波敌人生成所需属性
[Serializable]
public class Wave 
{
    public GameObject _enemyPrefab;
    public int _count;
    public float _rate;
}
