using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BattlePlayerUi : UIBase
{
    public Transform _AttackRoot;
    public Transform _DefenseRoot;
    public Text _NameTxt;
    public Slider _bloodSlider;
    public GameObject _actionObj;
    public Text _BloodChangeTxt;
    private SpineUiPlayer _curModel;

    private int _teamid;
    private float _maxBlood;

    public int ID => this._teamid;

    public void SetData(BattlePlayer player, BattlePlace myPlace)
    {
        this._bloodSlider.wholeNumbers = false;
        this._teamid = player.TeamID;
        HeroConfig config = HeroConfig.Instance.GetData(player.HeroID);
        this._NameTxt.text = config.Name;
        this._maxBlood = player.Attributes[AttributeDefine.OrignalBlood];
        if (this._curModel != null)
            Destroy(this._curModel.gameObject);
        GameObject prefab = ResourcesManager.Instance.LoadSpine(config.Model);
        GameObject obj;
        if (player.TeamID > 0 && myPlace == BattlePlace.Attack)
            obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, this._AttackRoot);
        else if (player.TeamID > 0 && myPlace == BattlePlace.Defense)
            obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, this._DefenseRoot);
        else if (player.TeamID < 0 && myPlace == BattlePlace.Attack)
            obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, this._DefenseRoot);
        else
            obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, this._AttackRoot);
     
        this._curModel = obj.GetComponent<SpineUiPlayer>();
        this._curModel.transform.localPosition = Vector3.zero;
        this._curModel.transform.localScale = Vector3.one;
        this._curModel.transform.localRotation = Quaternion.Euler(Vector3.zero);
        this._curModel.Play(SpineUiPlayer.STATE_IDLE, true);
        this.SetPostion();
        this.SetDoAction();
    }

    public void SetPostion()
    {
        BattlePlayer data = BattleProxy._instance.GetPlayer(this.ID);
        this.transform.position = new Vector3(data.Postion.x,0,data.Postion.y);
    }

    public void SetDoAction()
    {
        BattlePlayer pl = BattleProxy._instance.GetPlayer(this.ID);
        this._actionObj.SetActive(pl.Status == PlayerStatus.Action);
        this._BloodChangeTxt.gameObject.SetActive(false);
    }

    public void PlayAnimation(string ani, float backIdleSces = 0)
    {
        this._curModel.Play(ani, backIdleSces == 0);
        if (backIdleSces > 0)
        {
            CoroutineUtil.GetInstance().WaitTime(backIdleSces, true, OnBackToIdle);
        }
    }

    public void ReponseToEffect(PlayerEffectChangeData data)
    {
        //伤血
        this._curModel.transform.DOShakePosition(2f, new Vector3(30, 0, 0), 8).onComplete = () =>
        {
            this._curModel.transform.localPosition = Vector3.zero;
            this.UpdateBlood();
        };

        //飘一下伤血数字
        this.PlayBloodChange(data.ChangeValue);
    }

    private void PlayBloodChange(int changeValue)
    {
        if (changeValue < 0)
        {
            //减血
            this._BloodChangeTxt.text = LanguageConfig.GetLanguage(LanMainDefine.BloodDesc, changeValue);
        }
        else
        {
            this._BloodChangeTxt.text = LanguageConfig.GetLanguage(LanMainDefine.BloodAdd, changeValue);
        }
        this._BloodChangeTxt.gameObject.SetActive(true);
    }

    private void OnBackToIdle(object[] param)
    {
        //进入下一轮
        this._curModel.Play(SpineUiPlayer.STATE_IDLE, true);
    }

    public void UpdateBlood()
    {
        BattlePlayer player = BattleProxy._instance.GetPlayer(this.ID);
        this._bloodSlider.value = (float)player.Attributes[AttributeDefine.Blood] / (float)this._maxBlood;
    }

}


