using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




public class NumberAnimation : MonoBehaviour
{
    public int change_speed = 5; //加的次数
    private Text _txt;
    private Coroutine _curCor;
    private int min;
    private int max;
    int result = 0;
    void Awake()
    {
        this._txt = this.GetComponent<Text>();
    }

    public void DoAnimation(int curValue, int endValue)
    {
        this.min = curValue;
        this.max = endValue;

        this._txt.text = UtilTools.NumberFormat(curValue);
        if (this._curCor != null)
            this.StopCoroutine(_curCor);
        if(curValue != endValue)
            _curCor = StartCoroutine(Change());
    }


    IEnumerator Change()
    {
        int delta = (max - min) / change_speed;   //delta为速度，每次加的数大小
        result = min;
        for (int i = 0; i < change_speed; i++)
        {
            result += delta;
            this._txt.text = UtilTools.NumberFormat(result);
            yield return new WaitForSeconds(0.1f);     //每 0.1s 加一次
        }
        this._txt.text = UtilTools.NumberFormat(max);
        this._curCor = null;
    }
}

