using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HeroView : MonoBehaviour
    ,IScollItemClickListener
{
    public UIDrag _dragHandler;
    public DataGrid _hGrid;
    public List<UIToggle> _toggleList;
    public TextMeshProUGUI _countTxt;
    public HeroDetailsUI _detailsUi;
    private string _Element = "";
    private int _selectHeroId;
    private int _TotleCount = 0;
    private int _selectIndex = 0;

    public float _speed = 10f;
    public int _offsetChange = 200;
   

    void Awake()
    {
        foreach (UIToggle item in this._toggleList)
        {
            item._param._value = item.gameObject.name;
            item.AddEvent(OnSelectToggle);
        }
        this._dragHandler.AddEvent(this.OnBeginDrag, this.OnDrag, this.OnEndDrag);
    }

    public void DoChangeNextHero(int index)
    {
        int next = this._selectIndex + index;
        if (next >= this._TotleCount || next < 0)
            return;
        HeroItemRData curData = (HeroItemRData)this._hGrid.Data[next];
        this.SetDetails(curData._hero.Id);
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
      
    }

    public void OnDrag(PointerEventData eventData)
    {
        float xMove = eventData.delta.x * Time.deltaTime * _speed;
        HeroScene.GetInstance().DoDrag(-xMove);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float curX =  eventData.position.x;
        float StartX = eventData.pressPosition.x;
        float descX = curX - StartX;
        float abs = Mathf.Abs(curX - StartX);

        if (abs > _offsetChange)
        {
            int index = descX > 0 ? -1 : 1;
            this.DoChangeNextHero(index);
        }
        UIRoot.Intance._event.enabled = false;
        HeroScene.GetInstance().DoBackToOrignal();
        CoroutineUtil.GetInstance().WaitTime(0.4f, true, BackCallBack);
    }

    public void BackCallBack(object[] param)
    {
        UIRoot.Intance._event.enabled = true;
    }

    private void OnSelectToggle(UIToggle btnSelf)
    {
        UIToggle item = (UIToggle)btnSelf;

        string Element = (string)item._param._value;
        if (Element.Equals(this._Element))
            this._Element = "";
        else
            this._Element = Element;
        this.SetList();
    }

    public void InitData()
    {
        this._selectHeroId = 0;
        this._selectIndex = -1;
        this._Element = "";
        this._loadName = SceneDefine.Hero;
        StartCoroutine(LoadScene(this._loadName));
        
    }

    public void onClickScrollItem(ScrollData data)
    {
        if (data._Key.Equals("Hero"))
        {
            HeroItemRData curData = (HeroItemRData)data;
            if (curData._hero.Id == this._selectHeroId)
                return;
            this.SetDetails(curData._hero.Id);
        }
    }

    private string _loadName;
    private void SetDetails(int id)
    {
        int oldSelect = this._selectIndex;
        this._selectHeroId = id;
        foreach (ItemRender render in this._hGrid.ItemRenders)
        {
            HeroItemRender rd = (HeroItemRender)render;
            if (rd.gameObject.activeSelf == false)
                continue;
            rd.m_renderData._IsSelect = id == rd.ID;
            rd.SetSelectState();
            if (rd.m_renderData._IsSelect)
                this._selectIndex = this._hGrid.Data.IndexOf(rd.m_renderData);
        }

        this._detailsUi.SetData(id);
        HeroConfig config = HeroConfig.Instance.GetData(this._selectHeroId);

        if (oldSelect == -1)
        {
            HeroScene.GetInstance().SetModel(config.Model, 0);
        }
        else
        {
            HeroScene.GetInstance().SetModel(config.Model, this._selectIndex-oldSelect);
        }
    }

    public void UnLoadCurrentScene()
    {
        SceneManager.UnloadSceneAsync(this._loadName);
    }

    IEnumerator LoadScene(string name)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        asyncOperation.allowSceneActivation = true;
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        this.SetList();
    }

    public void SetList()
    {
        foreach (UIToggle toggle in this._toggleList)
        {
            string curEle = (string)toggle._param._value;
            toggle.IsOn = curEle.Equals(this._Element);
        }

        Dictionary<int, Hero> dic = HeroProxy._instance.GetAllHeros();
        List<Hero> owns = new List<Hero>();
        List<Hero> others = new List<Hero>();

        _hGrid.Data.Clear();
        foreach (Hero hero in dic.Values)
        {
            HeroConfig config = HeroConfig.Instance.GetData(hero.Id);
            if (config.Element.Equals(this._Element) || this._Element.Equals(""))
            {
                if (hero.IsMy)
                    owns.Add(hero);
                else if(config.DarkSide == 0)
                    others.Add(hero);
            }
        }


        owns.Sort(this.Compare);
        others.Sort(this.CompareID);

        this._TotleCount = owns.Count + others.Count;

        this._countTxt.text = LanguageConfig.GetLanguage(LanMainDefine.Progress, owns.Count, this._TotleCount);

        foreach (Hero hero in owns)
        {
            HeroItemRData data = new HeroItemRData(hero);
            this._hGrid.Data.Add(data);
        }

        foreach (Hero hero in others)
        {
            HeroItemRData data = new HeroItemRData(hero);
            this._hGrid.Data.Add(data);
        }

        _hGrid.ShowGrid(this);
        this.SetDetails((this._hGrid.Data[0] as HeroItemRData)._hero.Id);
    }

    private int Compare(Hero a, Hero b)
    {
        HeroConfig aConfig = HeroConfig.Instance.GetData(a.Id);
        HeroConfig bConfig = HeroConfig.Instance.GetData(b.Id);

        int compare = UtilTools.compareInt(bConfig.Star, aConfig.Star);
        if (compare != 0)
            return compare;
        return UtilTools.compareInt(b.Level, a.Level);
    }

    private int CompareID(Hero a, Hero b)
    {
        return UtilTools.compareInt(a.Id, a.Id);
    }
}
