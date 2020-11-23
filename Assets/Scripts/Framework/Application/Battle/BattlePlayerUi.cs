using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class BattlePlayerUi : UIBase
{
    public Transform _AttackRoot;
    public Transform _DefenseRoot;
    public Text _NameTxt;
    public Slider _bloodSlider;
    private SpineUiPlayer _curModel;

    private int _teamid;
    private float _maxBlood;

    public int ID => this._teamid;

    public void SetData(BattlePlayer player)
    {
        this._teamid = player.TeamID;
        HeroConfig config = HeroConfig.Instance.GetData(player.HeroID);
        this._NameTxt.text = config.Name;
        this._maxBlood = player.Attributes[AttributeDefine.OrignalBlood];
        if (this._curModel != null)
            Destroy(this._curModel.gameObject);
        GameObject prefab = ResourcesManager.Instance.LoadSpine(config.Model);
        GameObject obj;
        if (player.Place == BattlePlace.Attack)
            obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, this._AttackRoot);
        else
            obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, this._DefenseRoot);

        this._curModel = obj.GetComponent<SpineUiPlayer>();
        this._curModel.transform.localPosition = new Vector3(0, 0, 0);
        this._curModel.transform.localScale = Vector3.one;
        this._curModel.transform.localRotation = Quaternion.Euler(Vector3.zero);
        this._curModel.Play(SpineUiPlayer.STATE_IDLE, true);
    }


    public void UpdateBlood()
    {
        BattlePlayer player = BattleProxy._instance.GetPlayer(this.ID);
        this._bloodSlider.value = player.Attributes[AttributeDefine.Blood] / this._maxBlood;
    }

}


