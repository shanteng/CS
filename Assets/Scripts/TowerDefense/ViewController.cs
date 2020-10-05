using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewController : MonoBehaviour
{
    public float _WheelSpeed = 1000f;
    public float _speed = 10f;
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
        if (Input.GetMouseButtonDown(0))
        {
            this._isMouseDown = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            this._isMouseDown = false;
        }

        if (this._isMouseDown)
        {
            this.ComputePos();
        }
    }

    

    void LateUpdate()
    {
        this.DragMap();
        this.ScaleMap();
    }

    void ScaleMap()
    {
        // 缩放
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            //获取鼠标滚轮的滑动量
            float wheel = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * _WheelSpeed;
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
#endif
    }//end func

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
