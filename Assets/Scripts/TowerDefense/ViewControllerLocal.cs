using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewControllerLocal : MonoBehaviour
{
    
    public float _WheelSpeed = 1000f;

    [Range(100, 500)]
    public float _speed = 100f;
    [Range(1, 5)]
    public float _speedMobile = 2f;
    private float _realDragSpeed = 100;

    public Transform rightBorder;
    public Transform topBorder;
    public Transform leftBorder;
    public Transform bottomBorder;


    public float _maxSize = 21f;
    public float _minSize = 5f;
    private float _curSize = 21f;

    //相机局部坐标系下Translate的范围
    private float _xRightBorder = 0;
    private float _yTopBorder = 0;

    private float _xLeftBorder = 0;
    private float _yBottomBorder = 0;


    private bool _DoUpdateDrag = false;

    //记录两个手指的旧位置
    //记录上一次手机触摸位置判断用户是在左放大还是缩小手势
    private Vector2 oldPosition1;
    private Vector2 oldPosition2;

    private int isForward;

   

    void Start()
    {
        this._xRightBorder = Camera.main.transform.InverseTransformPoint(rightBorder.position).x;
        this._yTopBorder = Camera.main.transform.InverseTransformPoint(topBorder.position).y;

        this._xLeftBorder = Camera.main.transform.InverseTransformPoint(leftBorder.position).x;
        this._yBottomBorder = Camera.main.transform.InverseTransformPoint(bottomBorder.position).y;
        this._curSize = this._maxSize;
        this.ComputeBorder();

        Input.multiTouchEnabled = true;//开启多点触碰
    }

    //左右最大位移量
    private float _xMin = 0;
    private float _xMax = 0;

    //前后最大位移量
    private float _yMin = 0;
    private float _yMax = 0;
    private void ComputeBorder()
    {
        //视口宽高
        float halfHeight = Camera.main.orthographicSize;//正交相机高度的一半
        float _width = halfHeight * 2 * Camera.main.aspect;//正交相机的宽度
        float halfWidth = _width * 0.5f;

        //左右边界相机空间局部坐标
        float localMoveOffset = this._xRightBorder - halfWidth;
        this._xMax = localMoveOffset > 0 ? localMoveOffset : 0;
        this._xMin = this._xLeftBorder + halfWidth < 0 ? this._xLeftBorder + halfWidth : 0;

        localMoveOffset = this._yTopBorder - halfHeight;
        this._yMax = localMoveOffset > 0 ? localMoveOffset : 0;
        this._yMin = this._yBottomBorder + halfHeight < 0 ? this._yBottomBorder + halfHeight : 0;

#if UNITY_EDITOR
        this._realDragSpeed = this._speed * (this._curSize / this._maxSize);
#else
        this._realDragSpeed = this._speedMobile * (this._curSize / this._maxSize);
#endif
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
        }
#endif
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
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            //获取鼠标滚轮的滑动量
            float wheel = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * _WheelSpeed;
            this.DoScale(wheel);
        }
#else
        if (Input.touchCount > 1 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)//多点触碰
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
        bool needChange = (wheel > 0 && Camera.main.orthographicSize < this._maxSize) || (wheel < 0 && Camera.main.orthographicSize > this._minSize);
        if (needChange)
        {
            float aftersize = Camera.main.orthographicSize + wheel;
            if (aftersize > this._maxSize)
                aftersize = this._maxSize;
            else if (aftersize < this._minSize)
                aftersize = this._minSize;
            Camera.main.orthographicSize = aftersize;
            this._curSize = aftersize;
            this.ComputeBorder();
            this.DoDrag(0, 0);
        }
    }


    private void DoDrag(float xMove, float yMove)
    {
        float endX = this._translateX + xMove;
        if (endX > this._xMax)
        {
            xMove = this._xMax - this._translateX;
        }
        else if (endX  < this._xMin)
        {
            xMove = this._xMin - this._translateX;
        }

        float endY = this._translateY + yMove;
        if (endY > this._yMax)
        {
            yMove = this._yMax - this._translateY;
        }
        else if (endY < this._yMin)
        {
            yMove = this._yMin - this._translateY;
        }

        this._translateX += xMove;
        this._translateY += yMove;
        transform.Translate(new Vector3(xMove, yMove, 0));
    }
    private float _translateX = 0;
    private float _translateY = 0;
    void JudgeDragMap()
    {
#if UNITY_EDITOR
        if (this._DoUpdateDrag)
        {
            float xMove = -Input.GetAxisRaw("Mouse X") * Time.deltaTime * _realDragSpeed;
            float yMove = -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * _realDragSpeed;
            this.DoDrag(xMove, yMove);
        }
#else
        if (Input.touchCount == 1)
        {
            // 单点触碰移动摄像机
            if (Input.touches[0].phase == TouchPhase.Moved) //手指在屏幕上移动，移动摄像机
            {
                float xMove = -Input.touches[0].deltaPosition.x * Time.deltaTime * _realDragSpeed;
                float yMove = -Input.touches[0].deltaPosition.y * Time.deltaTime * _realDragSpeed;
                this.DoDrag(xMove, yMove);
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
    }//enf func

}//end class
