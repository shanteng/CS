using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;

public class TimeCallData
{
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
     
    public TimeCenterProxy() : base(ProxyNameDefine.TIME_CENTER)
    {
        
    }

    private int  Compare(TimeCallData x, TimeCallData y)
    {
        return (int)(y.TimeStep - x.TimeStep);
    }

    public void AddCallBack(TimeCallData data)
    {
        if (data._isTimeStep == false)
            data.TimeStep = UtilTools.GetExpireTime(data._needSecs);
        this._sortCallList.Add(data);
        this._sortCallList.Sort(this.Compare);
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
                this._isOver = true;
                this.CallBack();//通知回调
                yield return null;
            }
        }
        this.DoLatestTimeCallBack();//下一帧进行下一个
    }//end func

    private void CallBack()
    {
        Debug.LogWarning("TimeCenter CallBack:" + this._latestCallData._notifaction+"--param:"+ this._latestCallData._param);
        MediatorUtil.SendNotification(this._latestCallData._notifaction, this._latestCallData._param);
        _sortCallList.Remove(this._latestCallData);
        this._latestCallData = null;
    }
}//end class
