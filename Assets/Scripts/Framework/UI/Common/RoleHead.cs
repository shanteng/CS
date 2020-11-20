using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class RoleHead : UIBase
{
    public Image _Frame;
    public Image _Icon;

    public void SetData(int head,int frame = 0)
    {
        this._Icon.sprite = ResourcesManager.Instance.GetHeadSprite(head);
    }//end 

    public void SetHero(int id)
    {
        this._Icon.sprite = ResourcesManager.Instance.GetHeroHeadSprite(id);
    }
}
