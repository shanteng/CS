using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;

public class ViewControllerLocal : MonoBehaviour
{

    Vector3 _StartPos;
    public float _WheelSpeed = 1000f;
    public float _WheelMobile = 100f;

    [Range(100, 500)]
    public float _speed = 100f;
    [Range(1, 5)]
    public float _speedMobile = 2f;
    private float _realDragSpeed = 100;

    public Transform _minFollowTran;
  

    public float _maxSize = 21f;
    public float _minSize = 5f;
    
    public int _overShowCount = 50;
    public float _constY = 36;

    private bool _DoUpdateDrag = false;
    private float CosDegreeValue;
    private float SinDegreeValue;
    //记录两个手指的旧位置
    //记录上一次手机触摸位置判断用户是在左放大还是缩小手势
    private Vector2 oldPosition1;
    private Vector2 oldPosition2;

    private int isForward;
    private bool _isTryGoTo = false;
    
    private static ViewControllerLocal instance;

    public static ViewControllerLocal GetInstance()
    {
        return instance;
    }

    float maxDesc;
    float maxxiebian;
    void Awake()
    {
        instance = this;
        CosDegreeValue =Mathf.Cos(HomeLandManager.Degree * Mathf.Deg2Rad);
        SinDegreeValue = Mathf.Sin(HomeLandManager.Degree * Mathf.Deg2Rad);

        maxDesc = this._maxSize - this._minSize;
        maxxiebian = maxDesc / SinDegreeValue;
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

   
    public void InitBorder(int showRow,int showCol)
    {
        Camera.main.orthographicSize = this._maxSize / 2f;
        this._StartPos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);

        this.ComputeBorder();
    }

    //左右最大位移量
    private float _xMin = 0;
    private float _xMax = 0;
    //前后最大位移量
    private float _yMin = 0;
    private float _yMax = 0;
    public void ComputeBorder()
    {
        //视口宽高
 
        this._xMax = GameIndex.ROW / 2 + this._StartPos.x;// localMoveOffset > 0 ? localMoveOffset : 0;
        this._xMin = this._StartPos.x - GameIndex.ROW / 2;// this._xLeftBorder + halfWidth < 0 ? this._xLeftBorder + halfWidth : 0;

        this._yMax = GameIndex.COL / 2 + this._StartPos.z;// localMoveOffset > 0 ? localMoveOffset : 0;
        this._yMin = this._StartPos.z - GameIndex.COL / 2;

#if UNITY_EDITOR
        this._realDragSpeed = this._speed * (Camera.main.orthographicSize / this._maxSize);
#else
        this._realDragSpeed = this._speedMobile * (Camera.main.orthographicSize / this._maxSize);
#endif
    }

    public void SetDoUpdateDrag(bool isdo)
    {
        this._DoUpdateDrag = isdo;
    }

    private bool _isOverBuilding = false;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            this._DoUpdateDrag = true;
            _isOverBuilding = UtilTools.IsFingerOverBuilding(HomeLandManager.GetInstance().CurrentKey) && HomeLandManager.GetInstance().CurrentKey.Length > 0;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            this._DoUpdateDrag = false;
            /*  _isOverBuilding = UtilTools.IsFingerOverBuilding(HomeLandManager.GetInstance().CurrentKey) && HomeLandManager.GetInstance().CurrentKey.Length > 0;
            if (!HomeLandManager.GetInstance().IsDraging && !_isOverBuilding &&  !UtilTools.isFingerOverUI() && !HomeLandManager.GetInstance().isTryBuild)
             {
                 HomeLandManager.GetInstance().SetCurrentSelectBuilding("");
             }
         */
        }

        if (HomeLandManager.GetInstance().IsDraging == false)
            this.JudgeScaleMap();
       
    }

    private VInt2 _curPos = new VInt2();
    public VInt2 GetCurrentCordinate()
    {
        this._curPos.x = Mathf.RoundToInt( this.transform.position.x - this._StartPos.x);
        this._curPos.y = Mathf.RoundToInt(this.transform.position.z - this._StartPos.z);
        return this._curPos;
    }


    void LateUpdate()
    {
        if (HomeLandManager.GetInstance().IsDraging == false)
            this.JudgeDragMap();
    }

    void JudgeScaleMap()
    {
#if UNITY_EDITOR
        // 缩放
        if (Input.GetAxis("Mouse ScrollWheel") != 0 && UtilTools.isFingerOverUI() == false && !this._isTryGoTo)
        {
            //获取鼠标滚轮的滑动量
            float wheel = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * _WheelSpeed;
            this.DoScale(wheel);
        }
#else
        if (Input.touchCount > 1 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved 
        &&  UtilTools.isFingerOverUI() == false &&  !this._isTryGoTo)//多点触碰
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
            this.ComputeBorder();
            this.DoDrag(0, 0);
        }
    }

    public bool TryGotoWorldPostion(Vector3 wolrdPos, UnityAction<object> callBack = null, object param = null)
    {
        Vector3 cameraPos = new Vector3(this._StartPos.x + wolrdPos.x, this._StartPos.y, this._StartPos.z + wolrdPos.z);
        this._isTryGoTo = true;
        MediatorUtil.SendNotification(NotiDefine.WorldGoToStart);
        this.transform.DOMove(cameraPos, 1f).onComplete = () =>
        {
            this.SetMinMapPostion();
            this._isTryGoTo = false;
            MediatorUtil.SendNotification(NotiDefine.CordinateChange);
            if (callBack != null)
                callBack.Invoke(param);
        };

        PopupFactory.Instance.Hide();
        this._translateX = wolrdPos.x;
        this._translateY = wolrdPos.y;
        return true;
    }

    public bool TryGoto(VInt2 wolrdPos,UnityAction<object> callBack = null,object param = null)
    {
        VInt2 gamePos = UtilTools.WorldToGameCordinate(wolrdPos.x, wolrdPos.y);
        if (gamePos.x < 0 || gamePos.x > GameIndex.ROW || gamePos.y < 0 || gamePos.y > GameIndex.COL)
        {
            return false;
        }
         
        Vector3 cameraPos = new Vector3(this._StartPos.x + wolrdPos.x, this._StartPos.y, this._StartPos.z + wolrdPos.y);
        //this.transform.position = cameraPos;
        this._isTryGoTo = true;
        MediatorUtil.SendNotification(NotiDefine.WorldGoToStart);
        this.transform.DOMove(cameraPos, 1f).onComplete = () =>
        {
            this.SetMinMapPostion();
            this._isTryGoTo = false;
            MediatorUtil.SendNotification(NotiDefine.CordinateChange);
            
            BuildingData bd = WorldProxy._instance.GetBuildingInRange(wolrdPos.x, wolrdPos.y);
            if (bd != null && bd._city == 0)
            {
                HomeLandManager.GetInstance().SetCurrentSelectBuilding(bd._key);
            }
            else
            {
                //占领的城市
                CityData cityInfo = WorldProxy._instance.GetCityDataInPostionRange(wolrdPos.x, wolrdPos.y);
                if (cityInfo != null && cityInfo.Visible)
                {
                    HomeLandManager.GetInstance().OnClickNpcCity(cityInfo.ID);
                }
                else if (cityInfo == null)
                {
                    HomeLandManager.GetInstance().OnClickSpot(new Vector3(wolrdPos.x, 0, wolrdPos.y));
                }
            }
         
            if (callBack != null)
                callBack.Invoke(param);
        };
        PopupFactory.Instance.Hide();
        this._translateX = wolrdPos.x;
        this._translateY = wolrdPos.y;
        return true;
    }

    
    private float _translateX = 0;
    private float _translateY = 0;
    void JudgeDragMap()
    {
#if UNITY_EDITOR
        
        if (this._DoUpdateDrag && UtilTools.isFingerOverUI() == false && this._isOverBuilding == false && !this._isTryGoTo)
        {
            float xMove = -Input.GetAxisRaw("Mouse X") * Time.deltaTime * _realDragSpeed;
            float yMove = -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * _realDragSpeed;
            if (xMove != 0 || yMove != 0)
                this.DoDrag(xMove, yMove);
        }
#else
        if (Input.touchCount == 1 &&  UtilTools.isFingerOverUI() == false && this._isOverBuilding == false && !this._isTryGoTo)
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

    private void DoDrag2(float x, float y)
    {
        Vector3 pos = this.transform.position;
        pos.x += (x - y)*this.CosDegreeValue;
        //y = y * CosDegreeValue;
        pos.z += (x+y)*this.CosDegreeValue;
       

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

        this.SetMinMapPostion();
        MediatorUtil.SendNotification(NotiDefine.CordinateChange);
    }

    private void SetMinMapPostion()
    {
        float followX = this.transform.position.x - this._StartPos.x;
        float followZ = this.transform.position.z - this._StartPos.z;
        this._minFollowTran.position = new Vector3(followX, 0.2f, followZ);
    }

    private void DoDrag(float xMove, float yMove)
    {
        this.DoDrag2(xMove, yMove);
       /* yMove = yMove * CosDegreeValue;
        float endX = this._translateX + xMove;
        if (endX > this._xMax)
        {
            xMove = this._xMax - this._translateX;
        }
        else if (endX < this._xMin)
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
        Vector3 pos = this.transform.position;
        pos.y = _constY;
        this.transform.position = pos;
        MediatorUtil.SendNotification(NotiDefine.CordinateChange);
       */
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
