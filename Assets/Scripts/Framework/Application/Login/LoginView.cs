using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginView : MonoBehaviour
{
    public UIButton _BtnStart;
    public DataGrid _hGrid;
    private List<ScrollData> _showList = new List<ScrollData>();
    void Start()
    {
        _BtnStart.AddEvent(this.OnClickStart);
    }

    private void OnClickStart(UIButton btn)
    {
      /*  for (int i = 0; i < 10; ++i)
        {
            TimeCallData data = new TimeCallData();
            data._notifaction = NotiDefine.TEST_CALLBACK_NOTI;
            data._needSecs = Random.Range(1, 10);
            data._params = new Dictionary<string, object>();
            data._params["key"] = data._needSecs;
            MediatorUtil.SendNotification(NotiDefine.ADD_TIMESTEP_CALLBACK, data);
        }
      */
       /* _showList.Clear();
        for (int i = 0; i < 50; ++i)
        {
            DataTestItemData data = new DataTestItemData();
            data._Key = "test";
            data._text = i.ToString();
            this._showList.Add(data);
        }

        _hGrid.initCount(_showList);
       */

        MediatorUtil.SendNotification(NotiDefine.DoLoadScene, SceneDefine.Home);
    }
}
