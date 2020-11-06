using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;

public class HeroScene : MonoBehaviour
{
    public Camera _camera;
    public Transform _evnTran;
    public List<Transform> _roots;
    private int _curIndex = 0;

    private Vector3 _orignal;
    private GameObject _curModel;

    private static HeroScene instance;

    public float _MaxX = 10f;
    public float _MinX = 0f;

    public static HeroScene GetInstance()
    {
        return instance;
    }

    float maxDesc;
    float maxxiebian;
    void Awake()
    {
        _orignal = new Vector3();
        this._orignal.x = this._camera.transform.position.x;
        this._orignal.y = this._camera.transform.position.y;
        this._orignal.z = this._camera.transform.position.z;

        instance = this;
    }

    private void LateUpdate()
    {
        if (this._curModel != null)
            this._curModel.transform.LookAt(this._camera.transform);
    }

    public void SetModel(string model,int direction)
    {
        if (direction == 0)
        {
            this._curIndex = 0;
        }
        else if (direction > 0)
        {
            this._curIndex += 1;
            if (this._curIndex >= this._roots.Count)
                this._curIndex = 0;
        }
        else
        {
            this._curIndex -= 1;
            if (this._curIndex < 0)
                this._curIndex = this._roots.Count-1;
        }

        Transform root = this._roots[this._curIndex];
        float targetY = this._curIndex * 360 / this._roots.Count;
        Vector3 target = new Vector3(0, targetY,0);
        this._evnTran.DORotate(target, 0.4f);
        Destroy(this._curModel);
        GameObject prefab = ResourcesManager.Instance.LoadHeroModel(model);
        this._curModel = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, root);
        this._curModel.transform.localPosition = new Vector3(0, 0, 0);
        this._curModel.transform.localScale = Vector3.one;
        this._curModel.transform.localRotation = Quaternion.Euler(Vector3.zero);
        UtilTools.ChangeLayer(this._curModel, Layer.HeroScene);
    }

    public void DoBackToOrignal()
    {
        this._camera.transform.DOMoveX(this._orignal.x, 0.3f);
    }

    public void DoDrag(float x)
    {
        this._camera.transform.Translate(new Vector3(x, 0, 0), Space.World);
        Vector3 pos = this._camera.transform.position;
        if (pos.x >= this._MaxX)
        {
            pos.x = this._MaxX;
            this._camera.transform.position = pos;

        }
        else if (pos.x <= this._MinX)
        {
            pos.x = this._MinX;
            this._camera.transform.position = pos;
        }
    }
    

}//end class
