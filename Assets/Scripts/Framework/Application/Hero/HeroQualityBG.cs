using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;




public class HeroQualityBG : UIBase
{
    public Image _quality;
    public List<Color> _qualityColors;

    public  void SetData(int quality)
    {
        this._quality.color = this._qualityColors[quality - 1];
    }

    
}


