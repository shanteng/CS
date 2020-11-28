using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class SkillPassiveUi : UIBase
{
    public Text _TypeTxt;
    public Image _Icon;
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
        this._TypeTxt.text = SkillProxy._instance.GetSkillTypeName(id);
        this._Icon.sprite = ResourcesManager.Instance.GetSkillSprite(id);
        int count = this._ranks.Count;
        for (int i = 0; i < count; ++i)
        {
            int curRank = i + 1;
            _ranks[i].SetActive(level > curRank);
        }
    }
}


