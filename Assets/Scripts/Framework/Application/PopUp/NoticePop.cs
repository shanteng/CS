using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class NoticePop : Popup
{
    public Text _contentTxt;
    public GameObject _iconCon;
    public Image _icon;
 
    public override void setContent(object data)
    {
        StringKeyValue kv = (StringKeyValue)data;
        this._contentTxt.text = kv.key;
        bool hasIcon = kv.value.Equals("") == false;
        this._iconCon.SetActive(hasIcon);
        if (hasIcon)
        {
            this._icon.sprite = ResourcesManager.Instance.GetCommonSprite(kv.value);
            this._icon.SetNativeSize();
        }
          
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
    }//end func
}//end class
