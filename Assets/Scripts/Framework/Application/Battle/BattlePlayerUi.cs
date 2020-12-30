using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;

public class BattlePlayerUi : UIBase
{
    public Transform _AttackRoot;
    public Transform _DefenseRoot;
    public Text _NameTxt;
    public Slider _bloodSlider;
    public Image _bloodImage;
    public GameObject _actionObj;

    public Transform _EffectRoot;
    public Text _ChangeTxt;

    public Transform _SkillCallRoot;
    public UITexts _SkillCallTxt;


    private SpineUiPlayer _curModel;

    private int _teamid;
    private float _maxBlood;

    public List<Color> _bloodColors;

    private Queue<BattleEffectShowData> _WaitEffects = new Queue<BattleEffectShowData>();
    private Queue<int> _WaitSkills = new Queue<int>();

    public int ID => this._teamid;

    private void Awake()
    {
        this._ChangeTxt.gameObject.SetActive(false);
        this._SkillCallTxt.gameObject.SetActive(false);
    }



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

        int index = player.TeamID > 0 ? 0 : 1;
        this._bloodImage.color = this._bloodColors[index];
        this._NameTxt.color = this._bloodColors[index];

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
        this._ChangeTxt.gameObject.SetActive(false);
    }

    private UnityAction _callBack;
    public void PlayAnimation(string ani, float backIdleSces = 0,UnityAction callback = null)
    {
        this._curModel.Play(ani, backIdleSces == 0);
        if (backIdleSces > 0)
        {
            _callBack = callback;
            CoroutineUtil.GetInstance().WaitTime(backIdleSces, true, OnBackToIdle);
        }
    }

    private void OnBackToIdle(object[] param)
    {
        //进入下一轮
        this._curModel.Play(SpineUiPlayer.STATE_IDLE, true);
        if (this._callBack != null)
        {
            this._callBack.Invoke();
            this._callBack = null;
        }
    }

    private Coroutine _corSkill;
    private UnityAction _skillActionCallBack;
    public void SkillCall(int id,UnityAction callBack)
    {
        this._skillActionCallBack = callBack;
        _WaitSkills.Enqueue(id);
        if (this._corSkill == null)
        {
            _corSkill = StartCoroutine(PlaySkillCalls());
        }
    }

    IEnumerator PlaySkillCalls()
    {
        WaitForSeconds waitYield = new WaitForSeconds(2f);
        while (this._WaitSkills.Count > 0)
        {
            int id = this._WaitSkills.Dequeue();
            this.PlayOneSkillCall(id);
            yield return waitYield;
        }

        this._corSkill = null;
        if (_skillActionCallBack != null)
        {
            _skillActionCallBack.Invoke();
            this._skillActionCallBack = null;
        }
    }

    private void PlayOneSkillCall(int id)
    {
      /*  GameObject obj = GameObject.Instantiate(this._SkillCallTxt.gameObject, Vector3.zero, Quaternion.identity, this._SkillCallRoot);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
        obj.SetActive(true);
        UITexts changeTxt = obj.GetComponent<UITexts>();
        GameObject.Destroy(obj, 2f);

        if (id == 0)
            changeTxt.FirstLabel.text = LanguageConfig.GetLanguage(LanMainDefine.NormalAttack);
        else
        {
            SkillConfig config = SkillConfig.Instance.GetData(id);
            changeTxt.FirstLabel.text = config.Name;
        }
        //发送给UI，显示喊招特效
      */

        VInt2 kv = new VInt2(this.ID, id);
        MediatorUtil.SendNotification(NotiDefine.CallSkillUIShow, kv);
    }

    private Coroutine _cor;
    public void ReponseToEffect(PlayerEffectChangeData data)
    {
        //启动协程播放特效
        foreach (BattleEffectShowData effect in data.ChangeShowDatas.Values)
        {
            this._WaitEffects.Enqueue(effect);
        }

        if (this._cor == null)
        {
            _cor = StartCoroutine(PlayEffects());
        }
        this.UpdateBlood();
        //伤血
        this._curModel.transform.DOShakePosition(2f, new Vector3(30, 0, 0), 8).onComplete = () =>
        {
            this._curModel.transform.localPosition = Vector3.zero;
        };
    }

    IEnumerator PlayEffects()
    {
        WaitForSeconds waitYield = new WaitForSeconds(1f);
        while (this._WaitEffects.Count > 0)
        {
            BattleEffectShowData effect = this._WaitEffects.Dequeue();
            this.PlayOneEffect(effect);
            yield return waitYield;
        }
        this._cor = null;
    }


    private void PlayOneEffect(BattleEffectShowData effect)
    {
        GameObject obj = GameObject.Instantiate(this._ChangeTxt.gameObject, Vector3.zero, Quaternion.identity, this._EffectRoot);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
        obj.SetActive(true);
        Text changeTxt = obj.GetComponent<Text>();
        GameObject.Destroy(obj, 2f);
        string typeName = LanguageConfig.GetLanguage(UtilTools.combine("BuffName", effect.Type));
        string text = typeName;

        bool isUpValue = 
            effect.Type == SkillEffectType.Defense_Up ||
            effect.Type == SkillEffectType.Heal ||
            effect.Type == SkillEffectType.Attack_Up ||
            effect.Type == SkillEffectType.Speed_Up;

        obj.transform.Find("Up").gameObject.SetActive(isUpValue);
        obj.transform.Find("Down").gameObject.SetActive(!isUpValue);
        if (isUpValue)
        {
            if (effect.ChangeValue != 0)
                text = UtilTools.combine(typeName, "+", effect.ChangeValue.ToString());
            text = LanguageConfig.GetLanguage(LanMainDefine.EffectAdd, text);
        }
        else
        {
            if (effect.ChangeValue != 0)
                text = UtilTools.combine(typeName, "-", effect.ChangeValue.ToString());
            text = LanguageConfig.GetLanguage(LanMainDefine.EffectDesc, text);
        }
        changeTxt.text = text;
    }


    public void UpdateBlood()
    {
        BattlePlayer player = BattleProxy._instance.GetPlayer(this.ID);
        this._bloodSlider.value = (float)player.Attributes[AttributeDefine.Blood] / (float)this._maxBlood;
    }

}


