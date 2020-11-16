using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
public class GameLogView : MonoBehaviour
    , IScollItemClickListener
{
    public Transform _InnerTran;
    public DataGrid _hGrid;
    public UIScreenHideHandler _click;
    
    void Awake()
    {
        this._click.AddListener(DoHide);
    }

    private void DoHide()
    {
        this._InnerTran.DOLocalMoveX(-1200, 0.1f).onComplete = () =>
         {
             RoleProxy._instance.SetLogOld();
             MediatorUtil.HideMediator(MediatorDefine.GAME_LOG);
         };
    }

    public void DoShow()
    {
        this._InnerTran.localPosition = new Vector3(-1200, 0, 0);
        this._InnerTran.DOLocalMoveX(0, 0.3f);
    }

    public void SetList()
    {
        Queue<LogData> logs = RoleProxy._instance.GetLogs();
        _hGrid.Data.Clear();
        foreach (LogData datap in logs)
        {
            LogItemData data = new LogItemData(datap);
            this._hGrid.Data.Add(data);
        }
        _hGrid.ShowGrid(this, DataGrid.SCROLL_BOTTOM);
    }

    public void AddOne(LogData data)
    {
        LogItemData itemData = new LogItemData(data);
        this._hGrid.justAddOne(itemData);
        this._hGrid.UpdateView();
    }

    public void onClickScrollItem(ScrollData data)
    {
        LogData curData = ((LogItemData)data)._data;
        if (curData.Position != null)
        {
            ViewControllerLocal.GetInstance().TryGoto(curData.Position as VInt2);
        }
    }

}
