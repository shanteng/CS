using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainView : MonoBehaviour
{
    public UIButton _BtnBuild;
 
    void Start()
    {
        _BtnBuild.AddEvent(this.OnClickBuild);
    }

    private void OnClickBuild(UIButton btn)
    {
        MediatorUtil.ShowMediator(MediatorDefine.BUILD_CENTER);
    }
}
