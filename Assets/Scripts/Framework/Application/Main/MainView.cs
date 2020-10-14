using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerIdentity;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainView : MonoBehaviour
{
    public UIButton _BtnBuild;
    public Text _name;
    public Text _Uid;
 
    void Start()
    {
        _BtnBuild.AddEvent(this.OnClickBuild);
        this._name.text = PlayerIdentityManager.Current.userInfo.displayName;
        this._Uid.text = PlayerIdentityManager.Current.userInfo.userId;
    }

    private void OnClickBuild(UIButton btn)
    {
        MediatorUtil.ShowMediator(MediatorDefine.BUILD_CENTER);
    }
}
