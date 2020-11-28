using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SkillDetalisPop : Popup
{
    public SkillDetailUi _ui;

    public override void setContent(object data)
    {
        BattleSkill skill = (BattleSkill)data;
        this._ui.SetData(skill.ID, skill.Level);
    }
}//end class
