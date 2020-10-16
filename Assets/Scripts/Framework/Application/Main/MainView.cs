using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerIdentity;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainView : MonoBehaviour
{
    public UIButton _BtnBuild;
    public UIButton _BtnSdk;
    public InfoCanvas _InfoUI;
 
    void Start()
    {
        _BtnBuild.AddEvent(this.OnClickBuild);
        _BtnSdk.AddEvent(this.OnSdk);
        this._InfoUI.gameObject.SetActive(false);
    }

    private void OnSdk(UIButton btn)
    {
        UIRoot.Intance._SdkView.SetActive(true);
    }

    private void OnClickBuild(UIButton btn)
    {
        MediatorUtil.ShowMediator(MediatorDefine.BUILD_CENTER);
    }
}
