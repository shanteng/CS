using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CallSkillUi : MonoBehaviour
{
    public GameObject _AnimationObj;
    public UITexts _skillTxt;
    public Transform _SpineRoot;
    private  SpineUiPlayer _curModel;
    private Coroutine _cor;

    public void HideAnimation()
    {
        this._AnimationObj.SetActive(false);
    }

    public void SetDetalis(VInt2 data)
    {
        this.HideAnimation();
        this._AnimationObj.SetActive(true);
        int teamid = data.x;
        int skillid = data.y;
        BattlePlayer pl = BattleProxy._instance.GetPlayer(teamid);
        HeroConfig config = HeroConfig.Instance.GetData(pl.HeroID);
        SkillConfig configSkill = SkillConfig.Instance.GetData(skillid);

        string name = configSkill != null ? configSkill.Name : LanguageConfig.GetLanguage(LanMainDefine.NormalAttack);
        this._skillTxt.FirstLabel.text = name;

        if (this._curModel != null)
            Destroy(this._curModel.gameObject);

        GameObject prefab = ResourcesManager.Instance.LoadSpine(config.Model);
        GameObject obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, this._SpineRoot);
        this._curModel = obj.GetComponent<SpineUiPlayer>();
        this._curModel.transform.localPosition = new Vector3(0, 0, 0);
        this._curModel.transform.localScale = Vector3.one;
        this._curModel.transform.localRotation = Quaternion.Euler(Vector3.zero);
        this._curModel.Play(SpineUiPlayer.STATE_IDLE, true);
        UtilTools.ChangeLayer(this._curModel.gameObject, Layer.UI);
        //等待0.5f播放攻击动画

        if (this._cor != null)
            CoroutineUtil.GetInstance().Stop(this._cor);

        _cor = CoroutineUtil.GetInstance().WaitTime(0.5f, true, WaitEnd);

    }

    private void WaitEnd(object[] param)
    {
        this._curModel.Play(SpineUiPlayer.STATE_ATTACK, false);
        this._cor = null;
    }
}
