using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PatrolPop : Popup
{
    public DataGrid _vGrid;
    VInt2 Target;
    public override void setContent(object data)
    {
        Target = (VInt2)data;
        this.SetList();
    }

    public void SetList()
    {
        List<int> citys = WorldProxy._instance.GetAllOwnCity();
        _vGrid.Data.Clear();
        foreach (int city in citys)
        {
            VInt2 kv = WorldProxy._instance.GetCityPatrolInfo(city);
            int EmptyPatrol = kv.y - kv.x;
            int index = 1;
            for (int i = 0; i < EmptyPatrol; ++i)
            {
                PatrolItemData dataitem = new PatrolItemData(null, Target, city, index);
                this._vGrid.Data.Add(dataitem);
                index++;
            }

            List<PatrolData> datas = WorldProxy._instance.GetCItyPatrolDatas(city);
            for (int i = 0; i < datas.Count; ++i)
            {
                PatrolItemData dataitem = new PatrolItemData(datas[i], Target, city, index);
                this._vGrid.Data.Add(dataitem);
                index++;
            }
        }//end for city
        _vGrid.ShowGrid();
    }

}
