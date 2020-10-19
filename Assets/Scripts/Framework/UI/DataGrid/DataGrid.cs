using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IScollItemClickListener
{
    void onClickScrollItem(ScrollData data);
}

public class DataGrid : UIBase
{
    public static int SCROLL_NONE = -1;
    public static int SCROLL_BOTTOM = 0;
    public static int SCROLL_TOP = 1;

    //是否使用无限循环列表，对于列表项中OnDataSet方法执行消耗较大时不宜使用，因为OnDataSet方法会在滚动的时候频繁调用
    public bool useLoopItems = true;
    public GameObject m_goItemRender;
    public bool isAbsolute = true;

    private RectTransform m_content;
    private List<ScrollData> m_data = new List<ScrollData>();
    
    private readonly List<ItemRender> m_items = new List<ItemRender>();
    private readonly List<ItemRender> m_caches = new List<ItemRender>();
    private ScrollData m_selectedData;
    private LayoutGroup m_LayoutGroup;
    private RectOffset m_oldPadding;

    private ScrollRect m_scrollRect;
 
    private int m_itemSpace; //每个Item的空间
    private int m_viewItemCount; //可视区域内Item的数量（向上取整）
    private bool m_isVertical; //是否是垂直滚动方式，否则是水平滚动
    private int m_startIndex; //数据数组渲染的起始下标
    private bool m_isGridTable = false;
   

    private Vector2 sizeDelta = Vector2.zero;
 
    private IScollItemClickListener _onClickListnerer = null;
    public Text _sizeTxt;
    public void AddClickEvent(IScollItemClickListener listener)
    {
        this._onClickListnerer = listener;
    }


    private void Awake()
    {
        this.m_scrollRect = this.GetComponent<ScrollRect>();
       
        m_content = this.m_scrollRect.content;
        m_LayoutGroup = m_content.GetComponent<LayoutGroup>();

        if (m_LayoutGroup != null)
            m_oldPadding = m_LayoutGroup.padding;

        m_isVertical = m_scrollRect.vertical;

        var rt = this.gameObject.GetComponent<RectTransform>();

        if (this.isAbsolute)
        {
            this.sizeDelta.x = rt.sizeDelta.x;
            this.sizeDelta.y = rt.sizeDelta.y;
        }
        else
        {
            if (this.m_isVertical)
            {
                this.sizeDelta.y = Screen.currentResolution.height + rt.sizeDelta.y;
                this.sizeDelta.x = rt.sizeDelta.x;
            }
            else
            {
                this.sizeDelta.x = Screen.currentResolution.width + rt.sizeDelta.x;
                this.sizeDelta.y = rt.sizeDelta.y;
            }

            this._sizeTxt.text = UtilTools.combine("Screen.currentResolution.width:", Screen.currentResolution.width, " ScreenWidth:", Screen.width);
        }
       
        this.m_goItemRender.SetActive(false);
        LayoutElement element = this.m_goItemRender.GetComponent<LayoutElement>();
        this.m_isGridTable = m_LayoutGroup is GridLayoutGroup;
        if (m_isGridTable)
        {
            GridLayoutGroup gridGroup = m_LayoutGroup as GridLayoutGroup;
            m_itemSpace = (int)(m_isVertical ? (gridGroup.cellSize.y + gridGroup.spacing.y) : (gridGroup.cellSize.x + gridGroup.spacing.x));
            m_viewItemCount = Mathf.CeilToInt(ViewSpace / m_itemSpace);
        }
        else
        {
            var layoutGroup = m_LayoutGroup as HorizontalOrVerticalLayoutGroup;
            if (m_isVertical)
                m_itemSpace = (int)(element.preferredHeight + (int)layoutGroup.spacing);
            else
                m_itemSpace = (int)(element.preferredWidth + (int)layoutGroup.spacing);
            m_viewItemCount = Mathf.CeilToInt(ViewSpace / m_itemSpace);
        }

        if (this.useLoopItems)
        {
            m_scrollRect.onValueChanged.AddListener(OnScroll);
        }
    }

    private float _delta = 0;
    private void OnScroll(Vector2 data)
    {
        this._delta += Time.deltaTime;
        if (this._delta < 0.05f)//更新频率
            return;
        this._delta = 0;

        var value = (ContentSpace - ViewSpace) * (m_isVertical ? data.y : 1 - data.x);
        var start = ContentSpace - value - ViewSpace;
        var startIndex = Mathf.FloorToInt(start / m_itemSpace) * ConstraintCount;
        startIndex = Mathf.Max(0, startIndex);
        if (startIndex != m_startIndex)
        {
            m_startIndex = startIndex;
            UpdateView();
        } //end if
    }

    public List<ScrollData> Data
    {
        set
        {
            m_data = value;
            UpdateView();
        }
        get
        {
            return m_data;
        }
    }

    public List<ItemRender> ItemRenders
    {
        get { return m_items; }
    }

    public void Remove(ScrollData item)
    {
        if (item == null || Data == null)
            return;
        if (this.m_data.Contains(item))
            m_data.Remove(item);
    }

    /// <summary>
    /// 下一帧把指定项显示在最顶端并选中，这个比ResetScrollPosition保险，否则有些在UI一初始化完就执行的操作会不生效
    /// </summary>
    /// <param name="index"></param>
    public void ShowItemOnTopBy(int index, bool NotifySelect = false)
    {
        if (index < 0 || index >= this.m_data.Count)
            return;
        if (m_data.Count > index && NotifySelect)
            NotifyClickItem((ScrollData)m_data[index]);
        ResetScrollPosition(index);
    }

    /// <summary>
    /// 重置滚动位置，
    /// </summary>
    /// <param name="top">true则跳转到顶部，false则跳转到底部</param>
    public void ResetScrollPosition(bool top = true)
    {
        if (m_data == null)
            return;
        int index = top ? 0 : m_data.Count;
        ResetScrollPosition(index);
    }

    /// <summary>
    /// 重置滚动位置，如果同时还要赋值新的Data，请在赋值之前调用本方法
    /// </summary>
    public void ResetScrollPosition(int index)
    {
        if (m_data == null)
            return;
        var unitIndex = Mathf.Clamp(index / ConstraintCount, 0, DataUnitCount - m_viewItemCount > 0 ? DataUnitCount - m_viewItemCount : 0);
        var value = (unitIndex * m_itemSpace) / (Mathf.Max(ViewSpace, ContentSpace - ViewSpace));
        value = Mathf.Clamp01(value);

        //特殊处理无法使指定条目置顶的情况——拉到最后
        if (unitIndex != index / ConstraintCount)
            value = 1;

        if (m_scrollRect)
        {
            if (m_isVertical)
            {
                var pos = 1 - value;
                if (!UtilTools.IsFloatSimilar(m_scrollRect.verticalNormalizedPosition, pos))
                {
                    m_scrollRect.verticalNormalizedPosition = pos;
                }

            }
            else
            {
                if (!UtilTools.IsFloatSimilar(m_scrollRect.horizontalNormalizedPosition, value))
                {
                    m_scrollRect.horizontalNormalizedPosition = value;
                }
            }
        }

        m_startIndex = unitIndex * ConstraintCount;
        UpdateView();
    }

    public void addList(List<ScrollData> list)
    {
        if (this.m_data == null)
            this.m_data = new List<ScrollData>();
        this.m_data.AddRange(list);
        this.UpdateView();
    }

    public void justAddOne(ScrollData data, bool tofirst = false)
    {
        if (this.m_data == null)
            this.m_data = new List<ScrollData>();
        if (tofirst)
            this.m_data.Insert(0, data);
        else
            this.m_data.Add(data);
    }

    public void justRemoveOne(ScrollData data)
    {
        if (this.m_data == null)
            return;
        this.m_data.Remove(data);
    }

    public bool addOneData(ScrollData data, bool forceToBottom)
    {
        float scrollValue = 0;
        if (this.m_isVertical)
            scrollValue = this.m_scrollRect.verticalNormalizedPosition;
        else
            scrollValue = 1 - this.m_scrollRect.horizontalNormalizedPosition;
        bool isBottom = UtilTools.IsFloatSimilar(scrollValue, 0f);
        this.justAddOne(data);

        this.UpdateView();

        int itemLength = useLoopItems ? m_viewItemCount * ConstraintCount + CacheCount : m_data.Count;
        itemLength = Mathf.Min(itemLength, m_data.Count);
        var len = m_items.Count;

        if (isBottom || forceToBottom)
        {
            CoroutineUtil.GetInstance().WaitTime(0f, true, waitFrameEnd);
        }
        return isBottom || itemLength > len || forceToBottom;
    }
    private void waitFrameEnd(object[] param)
    {
        ResetScrollPosition(false);
    }

    private void setLayoutPadding()
    {
        if (m_data != null)
            m_startIndex = Mathf.Max(0, Mathf.Min(m_startIndex / ConstraintCount, DataUnitCount - m_viewItemCount - CacheUnitCount)) * ConstraintCount;

        var frontSpace = m_startIndex / ConstraintCount * m_itemSpace;
        var behindSpace = Mathf.Max(0, m_itemSpace * (DataUnitCount - CacheUnitCount) - frontSpace - (m_itemSpace * m_viewItemCount));
        if (m_isVertical)
            m_LayoutGroup.padding = new RectOffset(m_oldPadding.left, m_oldPadding.right, frontSpace, behindSpace);
        else
            m_LayoutGroup.padding = new RectOffset(frontSpace, behindSpace, m_oldPadding.top, m_oldPadding.bottom);
    }

    /// <summary>
    /// 更新视图
    /// </summary>
    public void UpdateView()
    {
        if (useLoopItems)
            this.setLayoutPadding();
        else
            m_startIndex = 0;

        int itemLength = useLoopItems ? m_viewItemCount * ConstraintCount + CacheCount : m_data.Count;
        itemLength = Mathf.Min(itemLength, m_data.Count);
        var len = m_items.Count;
        if (itemLength > len)
        {
            int cacheindex = 0;
            for (int i = len; i < itemLength; ++i)
            {
                if (cacheindex < this.m_caches.Count)
                {
                    int lastindex = this.m_caches.Count - 1;
                    ItemRender item = this.m_caches[lastindex];
                    item.gameObject.SetActive(true);
                    this.m_items.Add(item);
                    this.m_caches.RemoveAt(lastindex);
                }
            }
        }
        else if (itemLength < len)
        {
            for (int i = itemLength; i < len; i++)
            {
                int lastindex = this.m_items.Count - 1;
                ItemRender item = this.m_items[lastindex];
                item.gameObject.SetActive(false);
                this.m_caches.Add(item);
                this.m_items.RemoveAt(lastindex);
            }
        }

        for (int i = 0; i < itemLength; i++)
        {
            var index = m_startIndex + i;
            if (index >= m_data.Count || index < 0)
                continue;
            ScrollData curData = (ScrollData)m_data[index];
            if (i < m_items.Count)
            {
                m_items[i].SetData(curData);
                continue;
            }

            var go = GameObject.Instantiate(this.m_goItemRender) as GameObject;
            go.name = m_goItemRender.name+i;
            go.transform.SetParent(m_content, false);
            go.SetActive(true);

            ItemRender script = (ItemRender)go.GetComponent<ItemRender>();
            script._listener = this._onClickListnerer;
            script.SetData(curData);
            m_items.Add(script);
        }//end for
    }//end func


    private void NotifyClickItem(object renderData)
    {
        m_selectedData = (ScrollData)renderData;
        if (this._onClickListnerer != null)
            _onClickListnerer.onClickScrollItem(this.m_selectedData);
    }

    void Destroy()
    {
        m_items.Clear();
    }

    /// <summary>
    /// 选择指定项
    /// </summary>
    /// <param name="index"></param>
    public void Select(int index)
    {
        if (index >= m_data.Count || index < 0)
            return;

        if (m_data[index] != m_selectedData)
            NotifyClickItem((ScrollData)m_data[index]);
        UpdateView();
    }

    public float verticalPos
    {
        get { return m_scrollRect.verticalNormalizedPosition; }
        set { m_scrollRect.verticalNormalizedPosition = value; }
    }

    public float horizonPos
    {
        get { return m_scrollRect.horizontalNormalizedPosition; }
        set { m_scrollRect.horizontalNormalizedPosition = value; }
    }

    public float getScrollPos()
    {
        if (this.m_isVertical)
            return this.verticalPos;
        else
            return this.horizonPos;
    }

    //内容长度
    private float ContentSpace
    {
        get
        {
            return m_isVertical ? m_content.sizeDelta.y : m_content.sizeDelta.x;
        }
    }
    //可见区域长度
    private float ViewSpace
    {
        get
        {
            return m_isVertical ? (this.sizeDelta.y) : (this.sizeDelta.x);
        }
    }

    //约束常量（固定的行（列）数）
    private int ConstraintCount
    {
        get
        {
            return m_LayoutGroup is GridLayoutGroup ? (m_LayoutGroup as GridLayoutGroup).constraintCount : 1;
        }
    }
    //数据量个数
    public int DataCount
    {
        get
        {
            return m_data == null ? 0 : m_data.Count;
        }
    }

    //缓存数量
    public int CacheCount
    {
        get
        {
            return ConstraintCount + DataCount % ConstraintCount;
        }
    }
    //缓存单元的行（列）数
    public int CacheUnitCount
    {
        get
        {
            return m_LayoutGroup == null ? 1 : Mathf.CeilToInt((float)CacheCount / ConstraintCount);
        }
    }

    //数据单元的行（列）数
    public int DataUnitCount
    {
        get
        {
            return m_LayoutGroup == null ? DataCount : Mathf.CeilToInt((float)DataCount / ConstraintCount);
        }
    }

    public int viewItemCount
    {
        get
        {
            return this.m_viewItemCount;
        }
    }

    public int itemLineCount
    {
        get
        {
            if (this.m_data == null)
                return 1;

            int itemLength = useLoopItems ? m_viewItemCount * ConstraintCount + CacheCount : m_data.Count;
            itemLength = Mathf.Min(itemLength, m_data.Count);
            if (itemLength % ConstraintCount == 0)
                return itemLength / ConstraintCount;
            else
                return itemLength / ConstraintCount + 1;

        }
    }

    public ScrollRect GetScrollRect()
    {
        return this.m_scrollRect;
    }

    public void refreshImmediate()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.m_scrollRect.content);
    }

    public void ToBottom()
    {
        this.ResetScrollPosition(false);
    }

    public void ToTop()
    {
        this.ResetScrollPosition(true);
    }

    public void DoRefreshData()
    {
        int count = this.ItemRenders.Count;
        for (int i = 0; i < count; ++i)
        {
            this.ItemRenders[i].updateData();
        }
    }

    public void ShowGrid(IScollItemClickListener listner = null,int topIndex = -1, bool notifySelect = false, float scroll = 1)//默认置顶显示
    {
        this.AddClickEvent(listner);
        this.ResetScrollPosition();
        int viewItemCount = this.viewItemCount;
        int itemLine = this.itemLineCount;
        this.refreshImmediate();
        if (scroll != SCROLL_NONE)
        {
            CoroutineUtil.GetInstance().WaitTime(0, true, WaitInitEnd, scroll,topIndex, notifySelect);
        }
    }

    private void WaitInitEnd(object[] param)
    {
        if (this.GetScrollRect() == null)
            return;
        float scroll = (float)param[0];
        int topIndex = (int)param[1];
        bool notifySelect = (bool)param[2];

        if (scroll != 0)
        {
            if (this.m_isVertical)
                this.verticalPos = scroll;
            else
                this.horizonPos = 1 - scroll;
        }
        else if(topIndex >= 0)
        {
            this.ShowItemOnTopBy(topIndex, notifySelect);
        }
        
    }


}//end class
