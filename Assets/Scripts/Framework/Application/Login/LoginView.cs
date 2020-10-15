using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginView : MonoBehaviour
{
    public UIButton _BtnStart;
    public UIButton _BtnClear;
    void Start()
    {
        _BtnStart.AddEvent(this.OnClickStart);
        _BtnClear.AddEvent(this.OnClear);
    }

    private void OnClickStart(UIButton btn)
    {
    //    string json = CloudDataTool.LoadFile("BuildingUpgrade");
     //   List<BuildingUpgradeConfig> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BuildingUpgradeConfig>>(json);
    //    CloudDataTool.SaveFile("1231", list);
   //     return;
        MediatorUtil.SendNotification(NotiDefine.DoLoadScene, SceneDefine.Home);
    }

    private void OnClear(UIButton btn)
    {
        UIRoot.Intance.WipeOut();
    }
}
