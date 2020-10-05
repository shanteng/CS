using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewController : MonoBehaviour
{
    public float _WheelSpeed = 1000f;
    public float _speed = 10f;
    public float _speedMobile = 2f;
    public float _CameraY = 25;
    public float _CameraRotateX = 45;

    public Transform leftBorder;
    public Transform rightBorder;
    public Transform topBorder;
    public Transform bottomBorder;

   


    public float _maxSize = 21f;
    public float _minSize = 5f;

    private float _xLeftBorder = 0;
    private float _xRightBorder = 0;

    private float _zTopBorder = 0;
    private float _zBottomBorder = 0;

    private bool _isMouseDown = false;

    //记录两个手指的旧位置
    //记录上一次手机触摸位置判断用户是在左放大还是缩小手势
    private Vector2 oldPosition1;
    private Vector2 oldPosition2;

    private int isForward;

    Vector2 m_screenPos = new Vector2(); //记录手指触碰的位置

    private void Start()
    {

        this._xLeftBorder = leftBorder.position.x;
        this._xRightBorder = rightBorder.position.x;

        this._zTopBorder = topBorder.position.z;
        this._zBottomBorder = bottomBorder.position.z;


        Input.multiTouchEnabled = true;//开启多点触碰
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            this._isMouseDown = true;
            this.ComputePos();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            this._isMouseDown = false;
        }
#endif

        this.DragMap();
        this.ScaleMap();
    }

    

    void LateUpdate()
    {

    }

    void ScaleMap()
    {
#if UNITY_EDITOR
        // 缩放
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            //获取鼠标滚轮的滑动量
            float wheel = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * _WheelSpeed;
            this.DoScale(wheel);
        }
#endif
    }

    private void DoScale(float wheel)
    {
        //改变相机的位置
        float aftersize = Camera.main.orthographicSize + wheel;
        if (aftersize > this._maxSize)
            aftersize = this._maxSize;
        else if (aftersize < this._minSize)
            aftersize = this._minSize;
        Camera.main.orthographicSize = aftersize;
        this.ComputePos();
        this.JudgeBorder();
    }

    void JudgeBorder()
    {
        Vector3 pos = this.transform.position;
        if (pos.x > this._xMax)
            pos.x = this._xMax;
        else if (pos.x < this._xMin)
            pos.x = this._xMin;

        if (pos.z > this._zMax)
            pos.z = this._zMax;
        else if (pos.z < this._zMin)
            pos.z = this._zMin;

        transform.position = pos;
    }

    void DragMap()
    {
#if UNITY_EDITOR
        if (this._isMouseDown)
        {
            float xMove = -Input.GetAxisRaw("Mouse X") * Time.deltaTime * _speed;
            float zMove = -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * _speed;

            transform.Translate(new Vector3(xMove, 0, zMove),Space.World);

            this.JudgeBorder();
        }
        
#else
    
     if (Input.touchCount <= 0)
            return;
        if (Input.touchCount == 1)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
                m_screenPos = Input.touches[0].position;   //记录手指刚触碰的位置
            // 单点触碰移动摄像机
            if (Input.touches[0].phase == TouchPhase.Moved) //手指在屏幕上移动，移动摄像机
            {
                float xMove = -Input.touches[0].deltaPosition.x * Time.deltaTime * _speedMobile;
                float zMove = -Input.touches[0].deltaPosition.y * Time.deltaTime * _speedMobile;
                transform.Translate(new Vector3(xMove, 0, zMove), Space.World);
                this.JudgeBorder();
            }
        }
        else if (Input.touchCount > 1)//多点触碰
        {
            //前两只手指触摸类型都为移动触摸
            if (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
            {
                //计算出当前两点触摸点的位置
                var tempPosition1 = Input.GetTouch(0).position;
                var tempPosition2 = Input.GetTouch(1).position;
                //函数返回真为放大，返回假为缩小
                if (isEnlarge(oldPosition1, oldPosition2, tempPosition1, tempPosition2))
                {
                    this.isForward = -1;
                }
                else
                {
                    this.isForward = 1;
                }
                //备份上一次触摸点的位置，用于对比
                oldPosition1 = tempPosition1;
                oldPosition2 = tempPosition2;

                //获取鼠标滚轮的滑动量
                float wheel = isForward * Time.deltaTime * this._WheelSpeed;
                this.DoScale(wheel);
            }
        }
#endif
    }//end func



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
    }

    private float _xMin = 0;
    private float _xMax = 0;

    private float _zMin = 0;
    private float _zMax = 0;
    private void ComputePos()
    {
        //视口宽高
        float _height = Camera.main.orthographicSize;
        float _width = _height * 2 * Camera.main.aspect;

        //左右边界
        float halfWidth = _width * 0.5f;
        _xMin = halfWidth + this._xLeftBorder;
        _xMax = this._xRightBorder - halfWidth;

        //前后边界
        float CosDegreeValue = Mathf.Cos(_CameraRotateX * Mathf.Deg2Rad);
        float TanDegreeValue = Mathf.Tan(_CameraRotateX * Mathf.Deg2Rad);

        //上
        float zhijiaobian = _height / CosDegreeValue + _CameraY;
        float linbian = zhijiaobian / TanDegreeValue;
        this._zMax = this._zTopBorder - linbian;

        //下
        float xiaozhijiaobian = _CameraY / CosDegreeValue - _height;
        float xiaoxiebian = xiaozhijiaobian / Mathf.Sin(_CameraRotateX * Mathf.Deg2Rad);
        float dazhijiaobian = _CameraY * TanDegreeValue;
        float movedis = dazhijiaobian - xiaoxiebian;

        this._zMin = this._zBottomBorder + movedis;
    }

}
