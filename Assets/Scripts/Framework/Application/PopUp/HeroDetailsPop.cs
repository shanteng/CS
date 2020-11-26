using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HeroDetailsPop : Popup
{
    public HeroDetailsUI _HeroUi;

    public override void setContent(object data)
    {
        int heroid = (int)data;
        this._HeroUi.SetData(heroid, true);
    }
}//end class
