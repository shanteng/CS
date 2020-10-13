using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginView : MonoBehaviour
{
    public UIButton _BtnStart;
 
    void Start()
    {
        _BtnStart.AddEvent(this.OnClickStart);
    }

    private void OnClickStart(UIButton btn)
    {
    //    string json = CloudDataTool.LoadFile("BuildingUpgrade");
     //   List<BuildingUpgradeConfig> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BuildingUpgradeConfig>>(json);
    //    CloudDataTool.SaveFile("1231", list);
   //     return;
        MediatorUtil.SendNotification(NotiDefine.DoLoadScene, SceneDefine.Home);
    }
}
