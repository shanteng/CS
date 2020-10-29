using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;


public class ArmyItem : UIToggle
{
    public Text _Star;
    public Image _Icon;
    private int _id;

    public int ID => this._id;
    public void SetData(int id)
    {
        this._id = id;
        ArmyConfig config = ArmyConfig.Instance.GetData(id);
        this._Star.text = config.Star.ToString();
        this._Icon.sprite = ResourcesManager.Instance.getAtlasSprite(AtlasDefine.Army, config.Model);
        int currentCount = ArmyProxy._instance.GetArmyCountBy(id);
        this.Label.text = currentCount.ToString();
    }
}


