using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;




public class HeroCareerRates : UIBase
    , IPointerClickHandler
{
    public List<CareerRate> _list;
    private int _id;
    public void SetData(int id)
    {
        this._id = id;
        HeroConfig config = HeroConfig.Instance.GetData(id);
        int count = this._list.Count;
        for (int i = 0; i < count; ++i)
        {
            int career = i + 1;
            this._list[i].SetData(career, config.CareerRates[i]);
        }

    }

    public void SetUnSet()
    {
        int count = this._list.Count;
        for (int i = 0; i < count; ++i)
        {
            int career = i + 1;
            this._list[i].SetData(career, 0);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PopupFactory.Instance.ShowCareerRate();
    }

}


