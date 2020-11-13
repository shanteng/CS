using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;



public class HeroTalents : UIBase
    , IPointerClickHandler
{
    public List<TextMeshProUGUI> _list;
    public TextMeshProUGUI _luckyTxt;
    private int _id;
    public void SetData(int id)
    {
        this._id = id;
        HeroConfig config = HeroConfig.Instance.GetData(id);
        string[] datas = config.Talents;
        int count = this._list.Count;
        for (int i = 0; i < count; ++i)
        {
            string[] kv = datas[i].Split(':');
            this._list[i].text = kv[1];
        }

        this._luckyTxt.text = config.Lucky.ToString();
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PopupFactory.Instance.ShowHeroTalent(this._id);
    }

}


