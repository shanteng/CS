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

        MediatorUtil.SendNotification(NotiDefine.DO_LOAD_SCENE, SceneDefine.Home);
    }
}
