using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class BuildingUI : UIBase
{
    public CountDownCanvas _CdUi;
    public Text _NameTxt;
    public GameObject _AddCon;
    public Image _icon;
    private int _needValueShow = 0;
    void Start()
    {
        
    }

    public void DoCountDown(long expire, int totle)
    {
        this._CdUi.Show();
        this._CdUi.DoCountDown(expire, totle);
    }

    public void HideCD()
    {
        this._CdUi.Hide();
    }

    public void SetUI(BuildingConfig config)
    {
        //调整内部坐标
        int desc = config.RowCount;// (config.RowCount + config.ColCount) / 2;
        if (desc > 0)
            desc--;
        Vector2 pos = this._NameTxt.rectTransform.anchoredPosition;
        pos.y += desc * 4;
        this._NameTxt.rectTransform.anchoredPosition = pos;

        pos = this._CdUi.GetComponent<RectTransform>().anchoredPosition;
        pos.y -= desc * 25;
        this._CdUi.GetComponent<RectTransform>().anchoredPosition = pos;

        this._CdUi.GetComponent<RectTransform>().sizeDelta = new Vector2(20 * config.RowCount, 4);

        //税收
        this._AddCon.SetActive(false);
        if (config.AddType.Equals(ValueAddType.HourTax) == false)
            return;
        BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(config.ID, 1);
        if (configLv == null || configLv.AddValues == null || configLv.AddValues.Length == 0)
            return;

        ConstConfig cfgconst = ConstConfig.Instance.GetData(ConstDefine.IncomeShowValue);
        _needValueShow = cfgconst.IntValues[0];
        CostData add = new CostData();
        add.Init(configLv.AddValues[0]);
        this._icon.sprite = ResourcesManager.Instance.getAtlasSprite(AtlasDefine.Common, add.id);
    }

    public bool UpdateIncome(string key)
    {
        int value = RoleProxy._instance.GetCanAcceptIncomeValue(key);
        bool isShow = value >= this._needValueShow;
        this._AddCon.SetActive(isShow);
        return isShow;
    }
}
