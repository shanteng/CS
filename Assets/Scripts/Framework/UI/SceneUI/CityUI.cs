using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CityUI : UIBase
{
    public float MutilValue = 10;
    public Text _NameTxt;
    private int _id;


    public void SetUI(CityConfig config)
    {
        this._id = config.ID;
        this.UpdateOwn();


        //调整内部坐标
        int range = config.Range[0];
        Vector2 pos = this._NameTxt.rectTransform.anchoredPosition;
        pos.y += MutilValue * range;
        this._NameTxt.rectTransform.anchoredPosition = pos;
    }

    public void UpdateOwn()
    {
        CityConfig config = CityConfig.Instance.GetData(this._id);
        bool isOwn = WorldProxy._instance.IsOwnCity(this._id);

        if (!isOwn)
            this._NameTxt.text = config.Name;
        else
            this._NameTxt.text = LanguageConfig.GetLanguage(LanMainDefine.OwnCityName, config.Name);

    }

}
