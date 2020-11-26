using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class PathItemData : ScrollData
{
    public PathData _data;
    public PathItemData(PathData cg)
    {
        this._data = cg;
        this._Key = "PathItem";
    }
}

public class PathItem : ItemRender
{
    public Text TypeTxt;
    public Image Head;
    public Image Filled;

    private string _pathID;
    private long _ExpireTime = -1;
    private long _TotleSecs = 0;
    private Coroutine _curCor;
    

    public string IDs => this._pathID;
    protected override void setDataInner(ScrollData dataScroll)
    {
        PathData data = ((PathItemData)dataScroll)._data;
        this._pathID = data.ID;
        this._ExpireTime = data.ExpireTime;
        this._TotleSecs = data.ExpireTime - data.StartTime;

        string key = UtilTools.combine(LanMainDefine.PathType, data.Type);
        this.TypeTxt.text = LanguageConfig.GetLanguage(key);
        this.Head.sprite = ResourcesManager.Instance.getAtlasSprite(AtlasDefine.HeroSmall, data.Picture);
        if (this._curCor != null)
            this.StopCoroutine(_curCor);
        if (this._ExpireTime > GameIndex.ServerTime)
        {
            _curCor = StartCoroutine(CountDown());
        }
        else
        {
            this.SetCD();
        }
    }

    IEnumerator CountDown()
    {
        WaitForSeconds waitYield = new WaitForSeconds(1f);
        while (this._ExpireTime >= GameIndex.ServerTime)
        {
            this.SetCD();
            yield return waitYield;
        }
        this._curCor = null;
    }

    private void SetCD()
    {
        int leftSces = (int)(this._ExpireTime - GameIndex.ServerTime);
        if (leftSces < 0)
            leftSces = 0;
        this.Filled.fillAmount =(float)leftSces/ (float)this._TotleSecs;
    }
}


