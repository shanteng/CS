using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;

public class TimeCallData
{
    public string _key;//唯一标识
    public string _notifaction;//完成后回调的noti
    public object _param;

    public bool _isTimeStep = true;
    public int _needSecs;//
    public long TimeStep;//通知到达的时间戳 TimeCenterProxy 根据当前系统时间自动赋值
}

//时间戳回调管理
public class TimeCenterProxy : BaseRemoteProxy
{
    //按照时间排序好的回调列表
    private List<TimeCallData> _sortCallList = new List<TimeCallData>();
    private TimeCallData _latestCallData = null;
    private bool _isOver = false;
    private Coroutine _curCor;
    private string Noti;
    private object param;
     
    public TimeCenterProxy() : base(ProxyNameDefine.TIME_CENTER)
    {
        
    }

    private int  Compare(TimeCallData x, TimeCallData y)
    {
        return (int)(y.TimeStep - x.TimeStep);
    }

    public void RemoveCallBack(string key)
    {
        int rmIndex = -1;
        int count = this._sortCallList.Count;
        for (int i = 0; i < count; ++i)
        {
            if (this._sortCallList[i]._key.Equals(key))
            {
                rmIndex = i;
                break;
            }
        }

        if (rmIndex >= 0)
        {
            this._sortCallList.RemoveAt(rmIndex);
            this.DoLatestTimeCallBack();
        }
    }

    public void AddCallBack(TimeCallData data)
    {
        if (data._isTimeStep == false)
            data.TimeStep = UtilTools.GetExpireTime(data._needSecs);
        this._sortCallList.Add(data);
        this._sortCallList.Sort(this.Compare);
        if (this._curCor != null)
        {
            //等待下一帧再执行
            CoroutineUtil.GetInstance().WaitTime(0, true, WaitInitEnd);
        }
        else
        {
            this.DoLatestTimeCallBack();
        }
    }

    private void WaitInitEnd(object[] param)
    {
        this.DoLatestTimeCallBack();
    }

    private void DoLatestTimeCallBack()
    {
        if (this._curCor != null)
            TimeCenterMono.GetInstance().StopCoroutine(_curCor);
        
        int latestindex = this._sortCallList.Count - 1;
        if (latestindex >= 0)
        {
            this._latestCallData = this._sortCallList[latestindex];
            this._isOver = false;
            _curCor = TimeCenterMono.GetInstance().StartCoroutine(DoTimeStepCallBack());
        }
        else
        {
            this._isOver = true;
            this._latestCallData = null;
        }
    }

    IEnumerator DoTimeStepCallBack()
    {
        WaitForSeconds waitYield = new WaitForSeconds(1f);
        while (this._isOver == false)
        {
            if (this._latestCallData.TimeStep > GameIndex.ServerTime)
            {
                //还未到期，等待下一秒
                yield return waitYield;
            }
            else
            {
                this.CallBack();//通知回调;
            }
        }
    }//end func

    void CallBack()
    {
        Debug.LogWarning("TimeCenter CallBack:" + this._latestCallData._notifaction+"--param:"+ this._latestCallData._param);

        this.Noti = this._latestCallData._notifaction;
        this.param = this._latestCallData._param;
        MediatorUtil.SendNotification(this.Noti, this.param);

        _sortCallList.Remove(this._latestCallData);
        this._latestCallData = null;
        this.DoLatestTimeCallBack();
       
    }
}//end class
