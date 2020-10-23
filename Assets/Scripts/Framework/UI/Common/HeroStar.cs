using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HeroStar : UIBase
{
    public List<Image> _Stars;
   
    public void SetData(Hero hero)
    {
        HeroConfig config = HeroConfig.Instance.GetData(hero.Id);
        int star = config.Star;
        int count = this._Stars.Count;
        for (int i = 0; i < count; ++i)
        {
            this._Stars[i].gameObject.SetActive(i < star);
        }
    }
}
