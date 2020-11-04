using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;




public class HeroDetailsUI : UIBase
{
    private int _id;
    public  void SetData(int id)
    {
        this._id = id;
        HeroConfig config = HeroConfig.Instance.GetData(id);
       
    }

    

}


