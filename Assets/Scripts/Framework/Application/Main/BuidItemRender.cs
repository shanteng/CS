using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuidItemData : ScrollData
{
    public BuildingConfig _config;
    public BuidItemData(BuildingConfig cg)
    {
        this._config = cg;
        this._Key = "BuidItemRender";
    }
}


public class BuidItemRender : ItemRender
{
    public Text _nameText;
    private void Start()
    {
        
    }

    protected override void setDataInner(ScrollData data)
    {
        BuidItemData curData = (BuidItemData)data;
        this._nameText.text = curData._config.Name;
    }
}


