using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;

public class BattleViewController : MonoBehaviour
{
    public Camera _camera;
    public Camera _computeCamera;
    public Transform _left;
    public Transform _right;
    public Transform _top;
    public Transform _bottom;


    public float _WheelSpeed = 1000f;
    public float _WheelMobile = 100f;

    [Range(100, 500)]
    public float _speed = 100f;
    [Range(1, 5)]
    public float _speedMobile = 2f;
    private float _realDragSpeed = 100;

    public float _minSize = 12;
    public float _maxSize = 24;
    public float _constY = 28;

    private Vector3 _StartPos;
    private float CosDegreeValue;
    private bool _DoUpdateDrag = false;

    //记录两个手指的旧位置
    //记录上一次手机触摸位置判断用户是在左放大还是缩小手势
    private Vector2 oldPosition1;
    private Vector2 oldPosition2;
    private int isForward;

    private static BattleViewController ins;
    public static BattleViewController Instance => ins;


    void Awake()
    {
        ins = this;
        float degree = this._camera.transform.rotation.eulerAngles.x;
        CosDegreeValue = Mathf.Cos(degree * Mathf.Deg2Rad);
        this.InitBorder();
    }

    void Start()
    {
        Input.multiTouchEnabled = true;//开启多点触碰

#if UNITY_EDITOR
        //
#else
        this._WheelSpeed = this._WheelMobile;
#endif
    }


    //左右最大位移量
    private float _leftX = 0;
    private float _rightX = 0;
    private float _constLeftZ = 0;
    private float _constRightZ = 0;

    private float _TopY = 0;
    private float _BottomY = 0;
    private float _constTopZ = 0;
    private float _constBottomZ = 0;

    private float _xMin = 0;
    private float _xMax = 0;
    //前后最大位移量
    private float _yMin = 0;
    private float _yMax = 0;
    private void InitBorder()
    {
        this._StartPos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);

        Vector3 leftViewPostion = this._computeCamera.WorldToViewportPoint(this._left.transform.position);
        Vector3 viewPosLeft = this._computeCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, leftViewPostion.z));
        this._leftX = viewPosLeft.x;
        this._constLeftZ = leftViewPostion.z;

        Vector3 rightViewPostion = this._computeCamera.WorldToViewportPoint(this._right.transform.position);
        Vector3 viewPosRight = this._computeCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, rightViewPostion.z));
        this._rightX = viewPosRight.x;
        this._constRightZ = rightViewPostion.z;

        Vector3 topViewPostion = this._computeCamera.WorldToViewportPoint(this._top.transform.position);
        Vector3 viewPosTop = this._computeCamera.ViewportToWorldPoint(new Vector3(0.5f,1f, topViewPostion.z));
        this._TopY = viewPosTop.z;
        this._constTopZ = topViewPostion.z;

        Vector3 bottomViewPostion = this._computeCamera.WorldToViewportPoint(this._bottom.transform.position);
        Vector3 viewPosBottom = this._computeCamera.ViewportToWorldPoint(new Vector3(0.5f, 0f, bottomViewPostion.z));
        this._BottomY = viewPosBottom.z;
        this._constBottomZ = bottomViewPostion.z;

        this.ComputeDragSpeed();
    }

    private void ComputeDragSpeed()
    {
        Vector3 viewPosLeft = this._computeCamera.ViewportToWorldPoint(new Vector3(0,0.5f, this._constLeftZ));
        float CurLeftX = viewPosLeft.x;
     
        Vector3 viewPosRight = this._computeCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, this._constRightZ));
        float CurRightX = viewPosRight.x;

        //视口宽高
        this._xMax = this._StartPos.x + (this._rightX - CurRightX);//
        this._xMin = this._StartPos.x + (this._leftX- CurLeftX);//


        float TanDesc = Mathf.Tan((this._maxSize - _computeCamera.fieldOfView) * Mathf.Deg2Rad);
        Vector3 viewPoTop = this._computeCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, this._constTopZ));
        float CurTopY = viewPoTop.z * (1-TanDesc);

        Vector3 viewPosBottom = this._computeCamera.ViewportToWorldPoint(new Vector3(0.5f, 0f, this._constBottomZ));
        float CurBottomY = viewPosBottom.z * (1-TanDesc);
        this._yMax = this._StartPos.z + this._TopY - CurTopY;// 
        this._yMin = this._StartPos.z + this._BottomY - CurBottomY;

#if UNITY_EDITOR
        this._realDragSpeed = this._speed * (this._camera.fieldOfView / this._maxSize);
#else
        this._realDragSpeed = this._speedMobile * (this._camera.fieldOfView / this._maxSize);
#endif
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            this._DoUpdateDrag = true;
            
        }
        else if (Input.GetMouseButtonUp(0))
        {
            this._DoUpdateDrag = false;
        }

        this.JudgeScaleMap();
    }



    void LateUpdate()
    {
        this.JudgeDragMap();
    }

    void JudgeScaleMap()
    {
#if UNITY_EDITOR
        // 缩放
        if (Input.GetAxis("Mouse ScrollWheel") != 0 && !UtilTools.isFingerOverUI())
        {
            //获取鼠标滚轮的滑动量
            float wheel = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * _WheelSpeed;
            this.DoScale(wheel);
        }
#else
        if (Input.touchCount > 1 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved &&  !UtilTools.isFingerOverUI())
        {
            //前两只手指触摸类型都为移动触摸
            //计算出当前两点触摸点的位置
            var tempPosition1 = Input.GetTouch(0).position;
            var tempPosition2 = Input.GetTouch(1).position;
            //函数返回真为放大，返回假为缩小
            this.isForward = isEnlarge(oldPosition1, oldPosition2, tempPosition1, tempPosition2) ? -1 : 1;
            //备份上一次触摸点的位置，用于对比
            oldPosition1 = tempPosition1;
            oldPosition2 = tempPosition2;
            //获取滑动量
            float wheel = isForward * Time.deltaTime * this._WheelSpeed;
            this.DoScale(wheel);
        }
#endif
    }


    private void DoScale(float wheel)
    {
        //改变相机的位置
        bool needChange = (wheel > 0 && this._camera.fieldOfView < this._maxSize) || (wheel < 0 && this._camera.fieldOfView > this._minSize);
        if (needChange)
        {
            float aftersize = this._camera.fieldOfView + wheel;
            if (aftersize > this._maxSize)
                aftersize = this._maxSize;
            else if (aftersize < this._minSize)
                aftersize = this._minSize;
            this._camera.fieldOfView = aftersize;
            this._computeCamera.fieldOfView = aftersize;
            this.ComputeDragSpeed();
            this.DoDrag(0, 0);
        }
    }


    private float _translateX = 0;
    private float _translateY = 0;
    void JudgeDragMap()
    {
#if UNITY_EDITOR
        if (this._DoUpdateDrag && !UtilTools.isFingerOverUI())
        {
            float xMove = -Input.GetAxisRaw("Mouse X") * Time.deltaTime * _realDragSpeed;
            float yMove = -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * _realDragSpeed;
            if (xMove != 0 || yMove != 0)
                this.DoDrag(xMove, yMove);
        }
#else
        if (Input.touchCount == 1 && !UtilTools.isFingerOverUI() == false)
        {
            // 单点触碰移动摄像机
            if (Input.touches[0].phase == TouchPhase.Moved) //手指在屏幕上移动，移动摄像机
            {
                float xMove = -Input.touches[0].deltaPosition.x * Time.deltaTime * _realDragSpeed;
                float yMove = -Input.touches[0].deltaPosition.y * Time.deltaTime * _realDragSpeed;
                if (xMove != 0 || yMove != 0)
                    this.DoDrag(xMove, yMove);
            }
        }
#endif
    }//end func

    private void DoDrag(float x, float y)
    {
        Vector3 pos = this.transform.position;
        pos.x += x * this.CosDegreeValue;
        pos.z += y * this.CosDegreeValue;
       
        if (pos.x > this._xMax)
            pos.x = this._xMax;
        else if (pos.x < this._xMin)
            pos.x = this._xMin;

        if (pos.z > this._yMax)
            pos.z = this._yMax;
        else if (pos.z < this._yMin)
            pos.z = this._yMin;

        pos.y = this._constY;
        this.transform.position = pos;

    }

    //用于判断是否放大
    bool isEnlarge(Vector2 oP1, Vector2 oP2, Vector2 nP1, Vector2 nP2)
    {
        //函数传入上一次触摸两点的位置与本次触摸两点的位置计算出用户的手势
        var leng1 = Mathf.Sqrt((oP1.x - oP2.x) * (oP1.x - oP2.x) + (oP1.y - oP2.y) * (oP1.y - oP2.y));
        var leng2 = Mathf.Sqrt((nP1.x - nP2.x) * (nP1.x - nP2.x) + (nP1.y - nP2.y) * (nP1.y - nP2.y));
        if (leng1 < leng2)
        {
            //放大手势
            return true;
        }
        else
        {
            //缩小手势
            return false;
        }
    }//enf func

}//end class
