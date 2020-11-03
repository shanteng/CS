using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;


public class CreateRoleItem : UIToggle
{
    public RoleHead _Head;
    private int _id;
    public int ID => this._id;
    public void SetData()
    {
        this._id = UtilTools.ParseInt(this.gameObject.name);
        _Head.SetData(this._id);
    }
}


