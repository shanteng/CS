using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BuildCenterView : MonoBehaviour
    , IScollItemClickListener
{
    public DataGrid _hGrid;

    void Start()
    {
       
    }

    public void SetList()
    {
        Dictionary<int, BuildingConfig> dic =  BuildingConfig.Instance.getDataArray();
        _hGrid.Data.Clear();

        foreach (BuildingConfig config in dic.Values)
        {
            if (config.BuildMax == 0)
                continue;
            BuidItemData data = new BuidItemData(config);
            this._hGrid.Data.Add(data);
        }
        _hGrid.ShowGrid(this);
    }

    public void onClickScrollItem(ScrollData data)
    {
        if (data._Key.Equals("BuidItemRender"))
        {
            BuidItemData curdata = (BuidItemData)data;
            MediatorUtil.SendNotification(NotiDefine.TryBuildBuilding, curdata._config.ID);
            MediatorUtil.HideMediator(MediatorDefine.BUILD_CENTER);
        }
    }

}
