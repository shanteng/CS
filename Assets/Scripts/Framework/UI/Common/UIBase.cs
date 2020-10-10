using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class CustomizedData
{
    public string _key;
    public object _value;
}

public class UIBase : MonoBehaviour
{
    public CustomizedData _param;
    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}

