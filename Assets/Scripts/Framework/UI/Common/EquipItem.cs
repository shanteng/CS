using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class EquipItem : UIBase
{
    public EquipType _Type;
    public Image _Frame;
    public Image _Icon;
    public Text _Text;

    private string _id;
    public void SetData(string id)
    {
        this._id = id;
        bool isShow = id.Equals("") == false;
        this._Icon.gameObject.SetActive(isShow);
        if (isShow)
        {
            ItemInfoConfig config = ItemInfoConfig.Instance.GetData(id);
            this._Icon.sprite = ResourcesManager.Instance.GetItemSprite(this._id);
            this._Frame.sprite = ResourcesManager.Instance.GetCommonFrame(config.Quality);
            this._Text.text = config.Name;
        }
        else
        {
            this._Text.text = ItemInfoConfig.GetEquipTypeName(this._Type);
        }
    }//end 
}
