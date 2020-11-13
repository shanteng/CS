using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class QuesttemData : ScrollData
{
    public int HeroID;
    public int Target;
    public QuesttemData(int target,int hero)
    {
        this.HeroID = hero;
        this.Target = target;
        this._Key = "QuesttemRender";
    }
}



public class QuesttemRender : ItemRender
{
    public Text _HeroNameTxt;
    public Text _cityTxt;
    
    public Text _CDTxt;
    public HeroTalents _talentUi;
    public Slider _enegry;
    public TextMeshProUGUI _enegryTxt;
    public UIButton _btnSend;
    private VInt2 Target;
    Dictionary<string, object> vo = new Dictionary<string, object>();
    private void Start()
    {
        _btnSend.AddEvent(OnStartClick);
    }

    private void OnStartClick(UIButton btn)
    {
       bool isSuccess =  WorldProxy._instance.DoQuestCity(this.vo);
        if (isSuccess)
            PopupFactory.Instance.Hide();
    }


    protected override void setDataInner(ScrollData data)
    {
        QuesttemData curData = (QuesttemData)data;

        vo["TargetCity"] = curData.Target;
        vo["HeroID"] = curData.HeroID;

        Hero hero = HeroProxy._instance.GetHero(curData.HeroID);
        int cityid = hero.Belong;
        this._cityTxt.text = WorldProxy._instance.GetCityName(cityid);
        VInt2 cityPos = WorldProxy._instance.GetCityCordinate(cityid);
        VInt2 targetPos = WorldProxy._instance.GetCityCordinate(curData.Target);

        HeroConfig configGone = HeroConfig.Instance.GetData(curData.HeroID);
        this._HeroNameTxt.text = configGone.Name;

        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.QuestDeltaSces);
        int SecsDelta = cfgconst.IntValues[0];
        float HeroSecs = (float)SecsDelta / (float)configGone.Speed;

        long Expire = WorldProxy._instance.GetMoveExpireTime(cityPos.x, cityPos.y, targetPos.x, targetPos.y, HeroSecs);
        this._CDTxt.text = UtilTools.GetCdStringExpire(Expire);

        int cur = hero.GetEnegry();
        this._enegryTxt.text = LanguageConfig.GetLanguage(LanMainDefine.Progress, cur, hero.MaxEnegry);
        this._enegry.value = (float)cur / (float)hero.MaxEnegry;

        this._talentUi.SetData(curData.HeroID);

    }//end func


}


