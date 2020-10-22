using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class NoticePop : Popup
{
    public Text _contentTxt;
 
    public override void setContent(object data)
    {
        this._contentTxt.text = (string)data;
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
    }//end func
}//end class
