using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TextPop : Popup
{
    public Text _descTxt;
    public RectTransform _rect;

    public override void setContent(object data)
    {
        string content = (string)data;
        this._descTxt.text = content;
        LayoutRebuilder.ForceRebuildLayoutImmediate(this._rect);
    }//end func
}//end class
