using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;

public class HeroScene : MonoBehaviour
{
    public Camera _camera;
    public Transform _root;

    public Transform _lookAtTrans;
    private Vector3 _orignal;
    private GameObject _curModel;
    private bool _DoUpdateDrag = false;
    private bool _enableDrag = true;
    private static HeroScene instance;

    public float _speed = 100f;
    public float _speedMobile = 2f;

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
        if(GameIndex.InGame)
         MediatorUtil.ShowMediator(MediatorDefine.HERO);
    }


    public void SetModel(string model)
    {
        Destroy(this._curModel);
        GameObject prefab = ResourcesManager.Instance.LoadHeroModel(model);
        this._curModel = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, this._root);
        this._curModel.transform.localPosition = new Vector3(0, 0, 0);
        this._curModel.transform.localScale = Vector3.one;
        this._curModel.transform.localRotation = Quaternion.Euler(Vector3.zero);
        UtilTools.ChangeLayer(this._curModel, Layer.HeroScene);
    }


    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            this._DoUpdateDrag = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            this._DoUpdateDrag = false;
            _enableDrag = false;
            this._camera.transform.DOMoveX(this._orignal.x, 0.2f).onComplete = () =>
            {
                _enableDrag = true;
            };
        }
#else
        if (Input.touchCount > 1)
        {
            this._DoUpdateDrag = true;
        }
        else if (Input.touchCount > 0)
        {
            if (this._DoUpdateDrag)
            {
                this._camera.transform.DOMoveX(this._orignal.x, 0.2f).onComplete = () =>
                {
                    _enableDrag = true;
                };
            }
            this._DoUpdateDrag = false;
            _enableDrag = false;
        }
#endif
    }


    void LateUpdate()
    {
        this.JudgeDragMap();
        this._camera.transform.LookAt(this._lookAtTrans);
    }

 
    void JudgeDragMap()
    {
        bool doDrag = this._DoUpdateDrag && UtilTools.isFingerOverUI() == false && _enableDrag;
#if UNITY_EDITOR
        if (doDrag)
        {
            float xMove = -Input.GetAxisRaw("Mouse X") * Time.deltaTime * this._speed;
            if (xMove != 0)
                this.DoDrag(xMove);
        }
#else
        if (doDrag)
        {
            // 单点触碰移动摄像机
            if (Input.touches[0].phase == TouchPhase.Moved) //手指在屏幕上移动，移动摄像机
            {
                float xMove = -Input.touches[0].deltaPosition.x * Time.deltaTime * this._speedMobile;
                if (xMove != 0 )
                    this.DoDrag(xMove);
            }
        }
#endif
    }//end func

    private void DoDrag(float x)
    {
        this._camera.transform.Translate(new Vector3(x, 0, 0), Space.World);
    }
    

}//end class
