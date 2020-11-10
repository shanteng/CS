using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PatrolItemData : ScrollData
{
    public int City;
    public int Index;
    public VInt2 Target;
    public PatrolData _data;
    public PatrolItemData(PatrolData cg,VInt2 target,int city,int index)
    {
        this.Index = index;
        this._data = cg;
        this.Target = target;
        this.City = city;
        this._Key = "PatrolItemRender";
    }
}



public class PatrolItemRender : ItemRender
{
    public GameObject Mask;
    public Text _cityTxt;
    public TextMeshProUGUI _EndTxt;
    
    public Text _IDTxt;
    public CountDownText _CDTxt;

    public UIButton _btnSend;
    private VInt2 Target;
    Dictionary<string, object> vo = new Dictionary<string, object>();
    private void Start()
    {
        _btnSend.AddEvent(OnStartClick);
        this._EndTxt.GetComponent<UIClickHandler>().AddListener(OnEndClick);
    }

    private void OnStartClick(UIButton btn)
    {
        vo["x"] = this.Target.x;
        vo["z"] = this.Target.y;
        MediatorUtil.SendNotification(NotiDefine.PatrolDo, vo);
        PopupFactory.Instance.Hide();
    }

    private void OnEndClick(object param)
    {
        PatrolItemData curData = (PatrolItemData)this.m_renderData;
        this.TryGoTo(this.Target);
    }

    private void TryGoTo(VInt2 worldPos)
    {
        ViewControllerLocal.GetInstance().TryGoto(worldPos);
    }

    protected override void setDataInner(ScrollData data)
    {
        PatrolItemData curData = (PatrolItemData)data;

        vo["city"] = curData.City;

        this._cityTxt.text = WorldProxy._instance.GetCityName(curData.City);
        VInt2 cityPos = WorldProxy._instance.GetCityCordinate(curData.City);
        this._btnSend.gameObject.SetActive(curData._data == null);
        this.Mask.SetActive(curData._data != null);
        if (curData._data != null)
        {
            this.Target = curData._data.Target;
            VInt2 gamePos = UtilTools.WorldToGameCordinate(curData._data.Target.x, curData._data.Target.y);
            this._IDTxt.text = LanguageConfig.GetLanguage(LanMainDefine.BusyPartoller);
            this._EndTxt.text = LanguageConfig.GetLanguage(LanMainDefine.SpotCordinate, gamePos.x, gamePos.y);
            this._CDTxt.DoCountDown(curData._data.ExpireTime,LanMainDefine.LeftTime);
        }
        else
        {
            VInt2 gamePos = UtilTools.WorldToGameCordinate(curData.Target.x, curData.Target.y);
            this.Target = curData.Target;
            this._IDTxt.text = LanguageConfig.GetLanguage(LanMainDefine.FreePartoller);
            this._EndTxt.text = LanguageConfig.GetLanguage(LanMainDefine.SpotCordinate, gamePos.x, gamePos.y);
            this._CDTxt.Stop();
            ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.PatrolDeltaSces);
            int SecsDelta = cfgconst.IntValues[0];
            long Expire = WorldProxy._instance.GetMoveExpireTime(cityPos.x, cityPos.y, curData.Target.x, curData.Target.y, SecsDelta);
            string str = UtilTools.GetCdStringExpire(Expire);
            this._CDTxt._CDTxt.text = LanguageConfig.GetLanguage(LanMainDefine.NeedTime, str);//空闲
        }
    }//end func


}


