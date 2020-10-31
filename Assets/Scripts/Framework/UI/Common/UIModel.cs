using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

//所有滑动列表单项的基类
public class UIModel : UIBase
{
    public UIDragRotation _rotate;
    public UIPress _leftPress;
    public UIPress _rightPress;
    public float _speed = 10f;
    private GameObject _curModel;
    

    void Awake()
    {
        this._leftPress.AddEvent(OnLeftRotate);
        this._rightPress.AddEvent(OnRightRotate);
    }

    private void OnLeftRotate()
    {
        this._rotate.DoRotate(this._rotate._speed*Time.deltaTime *this._speed, 0);
    }

    private void OnRightRotate()
    {
        this._rotate.DoRotate(-this._rotate._speed * Time.deltaTime*this._speed, 0);
    }

    public void SetModel(string model)
    {
        Destroy(this._curModel);
        GameObject prefab = ResourcesManager.Instance.LoadArmyModel(model);
        this._curModel = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, this._rotate.transform);
        this._curModel.transform.localPosition = new Vector3(0, 0, 1);
        this._curModel.transform.localScale = Vector3.one;
        this._curModel.transform.localRotation = Quaternion.Euler(this._rotate._originalRotate);
        this._rotate.SetTarget(this._curModel.transform);
        UtilTools.ChangeLayer(this._curModel, Layer.UI);
    }
 
}


