using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

//所有滑动列表单项的基类
public class UIDragRotation : UIBase
    , IDragHandler
{
    public Vector3 _originalRotate;
    public bool _isHorize = true;
    public bool _isVertical = true;
    public int _speed = 1;
    private Transform _target;

    public void SetTarget(Transform tran)
    {
        this._target = tran;
    }

    public void DoRotate(float xMove, float yMove)
    {
        if (this._target == null)
            return;
        if (this._isHorize && this._isVertical)
            this._target.Rotate(yMove, -xMove, 0);
        else if (this._isVertical)
            this._target.Rotate(yMove, 0, 0);
        else if (this._isHorize)
            this._target.Rotate(0, -xMove, 0);
    }
   
    public void OnDrag(PointerEventData eventData)
    {
        float xMove = eventData.delta.x * Time.deltaTime * _speed;
        float yMove = eventData.delta.y * Time.deltaTime * _speed;
        this.DoRotate(xMove, yMove);
    }

    public void ResetDefualt()
    {
        this._target.localRotation = Quaternion.Euler(this._originalRotate);
    }
}


