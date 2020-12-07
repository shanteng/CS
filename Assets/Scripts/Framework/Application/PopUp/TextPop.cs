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
    public float MaxWidth = 320;
    public override void setContent(object data)
    {
        string content = (string)data;
        this._descTxt.text = content;
        float textWidh = this._descTxt.preferredWidth+6;
        if (textWidh < MaxWidth)
        {
            Vector2 size = this._rect.sizeDelta;
            _rect.sizeDelta = new Vector2(textWidh, size.y);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(this._rect);
    }//end func
}//end class
