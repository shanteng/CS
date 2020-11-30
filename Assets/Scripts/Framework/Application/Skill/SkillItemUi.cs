using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class SkillItemUi : UIBase
{
    public Text _NameTxt;
    public Image _Icon;
    public GameObject _Light;
    public Image _Base;
    public GameObject _LockGray;
    public GameObject _Lock;
    public List<GameObject> _ranks;
    private UIClickHandler _handler;
    private UnityAction<int> _callBack;
    private int _id;
    public int ID => this._id;
    private void Awake()
    {
        
    }

    public void AddEvent(UnityAction<int> Fun)
    {
        if (this._handler == null)
        {
            this._handler = this.GetComponent<UIClickHandler>();
            this._handler.AddListener(OnClick);
        }

        this._callBack = Fun;
    }

    private void OnClick(object obj)
    {
        if (this._callBack != null)
            this._callBack.Invoke(this.ID);
    }

    public void SetData(int id,int level = 1,bool isOpen = true)
    {
        this._id = id;
        if (id == 0)
            return;
        SkillConfig config = SkillConfig.Instance.GetData(id);
        this._NameTxt.text = config.Name;
        this._Icon.sprite = ResourcesManager.Instance.GetSkillSprite(id);

        UIRoot.Intance.SetImageGray(this._Icon, !isOpen);
        UIRoot.Intance.SetImageGray(this._Base, !isOpen);
        this._Light.SetActive(isOpen);
        this._Lock.SetActive(!isOpen);
        this._LockGray.SetActive(!isOpen);

        int count = this._ranks.Count;
        for (int i = 0; i < count; ++i)
        {
            int curRank = i + 1;
            _ranks[i].SetActive(level > curRank);
        }
    }
}


