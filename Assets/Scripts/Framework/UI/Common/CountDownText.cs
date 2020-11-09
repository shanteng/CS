using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class CountDownText : UIBase
{
    public Text _CDTxt;
    private long _expire = 0;
    private Coroutine _curCor;
    private string _LanKey;


    private void Start()
    {
      
    }

    public void Stop()
    {
        this._expire = 0;
        if (this._curCor != null)
            this.StopCoroutine(_curCor);
    }

    public void DoCountDown(long expire,string lanKey = "")
    {
        this._LanKey = lanKey;
        this._expire = expire;
        if (this._curCor != null)
            this.StopCoroutine(_curCor);
        if (this._expire > GameIndex.ServerTime)
        {
            _curCor = StartCoroutine(CountDown());
        }
        else
        {
            this.SetCD();
        }
    }

    private void SetCD()
    {
        string cdStr = UtilTools.GetCdStringExpire(this._expire);
        if (this._LanKey.Equals("") == false)
            this._CDTxt.text = LanguageConfig.GetLanguage(this._LanKey, cdStr);
        else
            this._CDTxt.text = cdStr;
    }

    IEnumerator CountDown()
    {
        WaitForSeconds waitYield = new WaitForSeconds(1f);
        while (this._expire > GameIndex.ServerTime)
        {
            this.SetCD();
            yield return waitYield;
        }
        this._curCor = null;
    }

}
