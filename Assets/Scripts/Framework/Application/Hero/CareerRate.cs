using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;




public class CareerRate : UIBase
{
    public Image _Icon;
    public Text _rateTxt;

    public  void SetData(int career,int rate)
    {
        this._Icon.sprite = ResourcesManager.Instance.GetCareerIcon(career);
        this._rateTxt.text = Hero.GetCareerRateName(rate);
    }

    
}


