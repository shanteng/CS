using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

public class ArmyAttributeUi : UIBase
{
    public Text _AttackTxt;
    public Text _DefenseTxt;
    public Text _SpeedTxt;
    public Text _BloodTxt;
    public void SetData(int armyid)
    {
        ArmyConfig armyConfig = ArmyConfig.Instance.GetData(armyid);
        this._AttackTxt.text = armyConfig.Attack.ToString();
        this._DefenseTxt.text = armyConfig.Defense.ToString();
        this._SpeedTxt.text = armyConfig.SpeedRate.ToString();
        this._BloodTxt.text = armyConfig.Blood.ToString();
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
    }//end func

}


