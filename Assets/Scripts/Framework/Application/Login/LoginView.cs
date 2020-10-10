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
        MediatorUtil.SendNotification(NotiDefine.DoLoadScene, SceneDefine.Home);
    }
}
