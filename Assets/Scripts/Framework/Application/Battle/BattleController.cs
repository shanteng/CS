using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class BattleController : MonoBehaviour
{
    private static BattleController ins;
    public static BattleController Instance => ins;

    void Awake()
    {
        ins = this;
    }

    public void InitScene()
    {
        BattleData data = BattleProxy._instance.Data;
        
    }
}


