using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class SkillDetailUi : UIBase
{
    public SkillItemUi _itemUi;
   
    public Text _TypeTxt;
    public TextMeshProUGUI _MpCostTxt;
    public Text _DescTxt;
    public Text _ReleaseTxt;

    public SkillRangeUi _AttackRgUi;
    public SkillRangeUi _DemageRgUi;
  
    private int _id;
    public int ID => this._id;
    private void Awake()
    {
        
    }


    public void SetData(int id, int level = 1, bool isOpen = true)
    {
        this._id = id;
        SkillConfig config = SkillConfig.Instance.GetData(id);
        this._itemUi.SetData(id, level, isOpen);
       
        this._TypeTxt.text = SkillProxy._instance.GetSkillTypeName(id);
        this._MpCostTxt.text = config.MpCost.ToString();
        this._MpCostTxt.gameObject.SetActive(config.MpCost > 0);
        List<string> descs = new List<string>();
        int count = config.EffectIDs.Length;
        for (int i = 0; i < count; ++i)
        {
            string desc = config.Descs[i];
            SkillEffectConfig configEffect = SkillEffectConfig.Instance.GetData(config.EffectIDs[i]);
            string valueStr = SkillProxy._instance.GetExpressionValueString(configEffect.Value, "$level", level);
            string rateStr = SkillProxy._instance.GetExpressionValueString(configEffect.Rate, "$level", level);
            string activeRateStr = SkillProxy._instance.GetExpressionValueString(configEffect.Active_Rate, "$level", level);

            string finalStr =  UtilTools.formatCustomize(desc,"{Value}",valueStr,"{Rate}", rateStr, "{Active_Rate}", activeRateStr, "{Duration}",configEffect.Duration);
            descs.Add(finalStr);
        }

        string str = string.Join("，", descs);
        this._DescTxt.text = str;

        this._ReleaseTxt.text = SkillProxy._instance.GetSkillReleasDesc(id);
        SkillLevelConfig configLv = SkillProxy._instance.GetSkillLvConfig(id, level);
        bool NoRange = configLv.DemageRangeID.Equals("");

        this._ReleaseTxt.gameObject.SetActive(NoRange);
        this._DemageRgUi.gameObject.SetActive(NoRange == false);
        this._AttackRgUi.gameObject.SetActive(NoRange == false);

        if (NoRange == false)
        {
            this._AttackRgUi.SetData(configLv.AttackRangeID);
            this._DemageRgUi.SetData(configLv.DemageRangeID);
        }
    }
}


