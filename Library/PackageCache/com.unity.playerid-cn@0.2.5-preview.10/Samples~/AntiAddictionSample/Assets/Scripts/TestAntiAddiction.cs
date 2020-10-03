using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerIdentity;
using System;

public class TestAntiAddiction : MonoBehaviour
{
    bool flag = false;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<AntiAddictionClientApi>().OnKickOff = OnKickOff;
        GetComponent<AntiAddictionClientApi>().OnMessage = OnMessage;
        GetComponent<AntiAddictionClientApi>().OnJudgePay = OnJudgePay;
        GetComponent<AntiAddictionClientApi>().OnJudgeTime = OnJudgeTime;
        StartCoroutine("Beat");
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerIdentityManager.Current.loginStatus == LoginStatus.LoggedIn)
            flag = true;
        else
            flag = false;
    }

    public void OnMessage(string title,string msg,string context) 
    {
        Debug.Log("OnMessage");
        Debug.Log("title:" + title);
        Debug.Log("msg:" + msg);
        Debug.Log("context:" + context);
    }

    public void OnKickOff(string traceId, string ruleName)
    {
        Debug.Log("OnKickOff");
        Debug.Log("traceId:" + traceId);
        Debug.Log("ruleName:" + ruleName);
        ReportExectuion(traceId, ruleName);
        PlayerIdentityManager.Current.Logout();
    }

    public void OnJudgePay(int age)
    {
        Debug.Log("OnPayAmount");
        Debug.Log("age:" + age);
        switch (age)
        {
            case (-1):
                Debug.Log("user's age is unknown.");
                break;
            case (1):
                Debug.Log("user's age is in [0,8)");
                break;
            case (8):
                Debug.Log("user's age is in [8,16)");
                break;
            case (16):
                Debug.Log("user's age is in [16,18)");
                break;
            default:
                Debug.Log("user's age is in more than 18");
                break;
        }
    }

    public void OnJudgeTime(int duration)
    {
        Debug.Log("OnJudgeTime");
        int hours = duration / 3600;
        int minutes = (duration - hours * 3600) / 60;
        int seconds = duration % 60;
        Debug.Log("current play time: " + hours + "h" + minutes + "min" + seconds + "s");
    }
    public void ContinueBeat()
    {
        GetComponent<AntiAddictionClientApi>().ContinueHeartbeat("continue beat");
    }

    public void JudgePay()
    {
        GetComponent<AntiAddictionClientApi>().JudgePay();
    }

    public void ReportExectuion(string traceId,string ruleName)
    {
        var timedelta = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        long time = timedelta.Ticks;
        long timeStamp = long.Parse(time.ToString().Substring(0, time.ToString().Length - 4));
        GetComponent<AntiAddictionClientApi>().ReportExecution(traceId, ruleName, timeStamp, "ReportExecution");
    }

    IEnumerator Beat()
    {
        while (true)
        {
            if (flag)
            {
                GetComponent<AntiAddictionClientApi>().ContinueHeartbeat("continue beat");
                yield return new WaitForSeconds(250f);
            }
            else
            {
                yield return null;
            }
        }
    }
}
