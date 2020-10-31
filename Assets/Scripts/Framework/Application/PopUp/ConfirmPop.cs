using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public interface IConfirmListener
{
    void OnConfirm(ConfirmData data);
}

public class ConfirmData
{
    public string titleText = "";
    public string contentText = "";
    public string doubleCheckText = "";
    public string resTitleText = "";
    public bool isDoubleCheckOn;//是否默认选中
    public List<CostData> Reses;
    //按钮
    public string sureText = "";
    public string cancelText = "";
    public IConfirmListener listener;
    public string userKey = "";
    public object param;
}

public class ConfirmPop : Popup
{
    public Text _titleTxt;
    public Text _contentTxt;
    public UIButton _BtnSure;
    public UIButton _BtnCancel;
    public UIToggle _BtnDoubleCheck;
    public GameObject _ResCon;
    public Text _resTitleTxt;
    public List<CostItem> _ResList;
    private ConfirmData _data;
    void Start()
    {
        _BtnSure.AddEvent(this.OnClick);
        _BtnCancel.AddEvent(this.OnClick);
    }

    private void OnClick(UIButton btn)
    {
        if (this._data.listener != null && btn.Equals(this._BtnSure))
        {
            this._data.isDoubleCheckOn = this._BtnDoubleCheck.IsOn;
            this._data.listener.OnConfirm(this._data);
        }
        this.HidePop();
    }

    public override void setContent(object data)
    {
        this._data = (ConfirmData)data;
        this._titleTxt.text = !this._data.titleText.Equals("") ? this._data.titleText : LanguageConfig.GetLanguage(LanMainDefine.ConfirmTitle);
        this._contentTxt.text = this._data.contentText;
        this._BtnSure.Label.text = !this._data.sureText.Equals("") ? this._data.sureText : LanguageConfig.GetLanguage(LanMainDefine.ConfirmSure);
        this._BtnCancel.Label.text = !this._data.cancelText.Equals("") ? this._data.cancelText : LanguageConfig.GetLanguage(LanMainDefine.ConfirmCancel);
      
        this._BtnDoubleCheck.gameObject.SetActive(this._data.doubleCheckText.Equals("") == false);
        this._BtnDoubleCheck.Label.text = this._data.doubleCheckText;
        this._BtnDoubleCheck.IsOn = this._data.isDoubleCheckOn;

        this._resTitleTxt.text = this._data.resTitleText;
        this._ResCon.SetActive(this._data.Reses != null);
        if (this._data.Reses != null)
        {
            int count = this._data.Reses.Count;
            int len = this._ResList.Count;
            for (int i = 0; i < len; ++i)
            {
                if (i >= count)
                {
                    this._ResList[i].Hide();
                    continue;
                }
                this._ResList[i].Show();
                this._ResList[i].SetData(this._data.Reses[i]);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(_ResCon.GetComponent<RectTransform>());
        }
    }//end func
}//end class
