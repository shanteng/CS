using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PathUi : MonoBehaviour
    , IScollItemClickListener
{
    public DataGrid _hGrid;
    void Awake()
    {

    }

    private int Compare(PathData x, PathData y)
    {
        int compare = UtilTools.compareLong(x.Type, y.Type);
        if (compare != 0)
            return compare;
        return UtilTools.compareLong(x.ExpireTime, y.ExpireTime);
    }

    public void SetList()
    {
        Dictionary<string, PathData> paths = PathProxy._instance.AllPaths;
        _hGrid.Data.Clear();
        List<PathData> list = new List<PathData>();
        foreach (PathData config in paths.Values)
        {
            list.Add(config);
          
        }

        list.Sort(Compare);

        foreach (PathData datap in list)
        {
            PathItemData data = new PathItemData(datap);
            this._hGrid.Data.Add(data);
        }
        _hGrid.ShowGrid(this);
    }

    public void onClickScrollItem(ScrollData data)
    {
        if (data._Key.Equals("PathItem"))
        {
            PathItemData curData = (PathItemData)data;
            bool isSelect = HomeLandManager.GetInstance().GoToPathCurrentPostion(curData._data.ID);
            foreach (ItemRender render in this._hGrid.ItemRenders)
            {
                PathItem rd = (PathItem)render;
                if (rd.gameObject.activeSelf == false)
                    continue;
                if (rd.IDs.Equals(curData._data.ID))
                {
                    rd.m_renderData._IsSelect = isSelect;
                }
                else
                {
                    rd.m_renderData._IsSelect = false;
                }
                rd.SetSelectState();
            }

        }
    }

}
