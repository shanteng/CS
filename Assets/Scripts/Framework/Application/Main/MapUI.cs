using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.PlayerIdentity;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MapUI : MonoBehaviour
{
    public TMP_InputField _inputX;
    public TMP_InputField _inputY;

    public UIButton _btnGoTo;

    public GameObject _btnHomeCon;
    public UIButton _btnHome;
    public RectTransform _RotateRect;



    private VInt2 _pos = new VInt2();
    
    void Start()
    {
        _btnGoTo.AddEvent(this.OnClickGoTo);
        _btnHome.AddEvent(this.OnClickHome);
        this._inputX.onValueChanged.AddListener(OnEditChange);
        this._inputY.onValueChanged.AddListener(OnEditChange);
    }

    private void OnEditChange(string str)
    {
        this.SetGotoState();
    }

    private void SetGotoState()
    {
        int x = UtilTools.ParseInt(this._inputX.text);
        int y = UtilTools.ParseInt(this._inputY.text);
        bool isSame = x == this._pos.x && y == this._pos.y;
        this._btnGoTo.gameObject.SetActive(isSame == false);
    }
   

    public void SetCordinate()
    {
        VInt2 pos = ViewControllerLocal.GetInstance().GetCurrentCordinate();
        VInt2 gamePos = UtilTools.WorldToGameCordinate(pos.x, pos.y);

       
        this._pos.x = gamePos.x;
        this._pos.y = gamePos.y;
        this._inputX.text = gamePos.x.ToString();
        this._inputY.text = gamePos.y.ToString();
        this._btnGoTo.gameObject.SetActive(false);

        var distance = Mathf.Sqrt(pos.x * pos.x + pos.y * pos.y);
        var distanceInt = Mathf.FloorToInt(distance);
        string formatDistance = UtilTools.NumberFormat(distanceInt);
        this._btnHome.Label.text = LanguageConfig.GetLanguage(LanMainDefine.Distance, formatDistance);
        Vector3 zeroScreen = Camera.main.WorldToScreenPoint(Vector3.zero);
        Vector3 curScreen = Camera.main.WorldToScreenPoint(new Vector3(pos.x, 0, pos.y));


        var postionTarget = zeroScreen;
        var postionMy = curScreen;
        var from = Vector3.up;
        var to = postionTarget - postionMy;
        _RotateRect.rotation = Quaternion.FromToRotation(from, to);
        this._RotateRect.gameObject.SetActive(distance > 10);
    }

    private void OnClickHome(UIButton btn)
    {
        VInt2 main = WorldProxy._instance.GetCityCordinate(0);
        ViewControllerLocal.GetInstance().TryGoto(main);
    }

    private void OnClickGoTo(UIButton btn)
    {
        int x = UtilTools.ParseInt(this._inputX.text);
        int y = UtilTools.ParseInt(this._inputY.text);
        VInt2 worldPos = UtilTools.GameToWorldCordinate(x, y);
        bool isGoto =  ViewControllerLocal.GetInstance().TryGoto(worldPos);
    }


}
