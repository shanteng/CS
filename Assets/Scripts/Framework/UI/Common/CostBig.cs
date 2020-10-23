using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class CostBig : UIBase
{
    public Image _Frame;
    public Image _Icon;
    public Text _Text;
    public Text _MyTxt;
    public Animator _NotEnoughtAni;

    public void SetData(CostData data,bool needMy = false)
    {
        this._Icon.sprite = ResourcesManager.Instance.GetItemSprite(data.id);
        this._Frame.sprite = ResourcesManager.Instance.GetItemFrameSprite(data.id);
        this._Text.text = UtilTools.NumberFormat(data.count);
        this._MyTxt.gameObject.SetActive(needMy);
        if (needMy)
        {
            int myCount = RoleProxy._instance.GetNumberValue(data.id);
            string valueStr = UtilTools.NumberFormat(data.count);
            this._MyTxt.text = LanguageConfig.GetLanguage(LanMainDefine.OwnCount, valueStr);
            _NotEnoughtAni.enabled = myCount < data.count;
        }
    }//end 
}
