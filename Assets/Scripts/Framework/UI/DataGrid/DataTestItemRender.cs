using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DataTestItemData : ScrollData
{
    public string _text;
}


public class DataTestItemRender : ItemRender
{
    public Text _testTxt;
    private void Start()
    {
        
    }

    protected override void setDataInner(ScrollData data)
    {
        DataTestItemData curData = (DataTestItemData)data;
        this._testTxt.text = curData._text;
    }
}


