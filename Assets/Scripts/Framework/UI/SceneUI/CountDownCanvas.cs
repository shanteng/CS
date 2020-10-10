using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class CountDownCanvas : UIBase
{
    public Slider _progress;
    public Text _CDTxt;

    private long _expire =0;
    private long _totleSces = 0;
    private Coroutine _curCor;

    private void Start()
    {
        this._progress.wholeNumbers = true;
        this._progress.minValue = 0;
    }

    public void DoCountDown(long expire,int totleSecs)
    {
        this.Show();
        this._expire = expire;
        this._totleSces = totleSecs;
        this._progress.maxValue = this._totleSces;
        this._progress.value = 0;
        if (this._curCor != null)
            this.StopCoroutine(_curCor);
        _curCor = StartCoroutine(CountDown());
    }

    IEnumerator CountDown()
    {
        WaitForSeconds waitYield = new WaitForSeconds(1f);
        while (this._expire > GameIndex.ServerTime)
        {
            int leftSces = (int)(this._expire - GameIndex.ServerTime);
            string cdStr = UtilTools.GetCdString(leftSces);
            this._progress.value = this._totleSces - leftSces;
            this._CDTxt.text = cdStr;
            yield return waitYield;
        }
        this._curCor = null;
    }

}
