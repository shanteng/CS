using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class SkillRangeUi : UIBase
{
    public Color _StartPointColor = Color.white;
    public Color _OtherPointColor = Color.red;

    public float Size = 100f;
    public Text _nameTxt;
    public Image _Templete;
    private List<GameObject> _saves = new List<GameObject>();
    private int _id;
    public int ID => this._id;
    private void Awake()
    {
        this._Templete.gameObject.SetActive(false);
    }

    public void SetData(string rangeID)
    {
        int count = this._saves.Count;
        for (int i = 0; i < count; ++i)
        {
            GameObject.Destroy(this._saves[i]);
        }

        this._saves.Clear();

        RangeFunctionConfig config = RangeFunctionConfig.Instance.GetData(rangeID);
        this._nameTxt.text = config.Name;
        Vector2 StartPos = new Vector2(0, 0);
        int maxRange = 1;
        if (config.Function.Equals(RangeTypeDefine.Point))
        {
            maxRange = 1;
        }
        else if (config.Function.Equals(RangeTypeDefine.Line))
        {
            maxRange = config.ComputeParams[0];
        }
        else
        {
            maxRange = config.ComputeParams.Length > 0 ? config.ComputeParams[0] : 1;
            if (config.ComputeParams.Length > 1 && config.ComputeParams[1] > maxRange)
                maxRange = config.ComputeParams[1];
            maxRange = maxRange * 2 + 1;
        }

        float cellSize = Size / maxRange;//正方形
        if (maxRange == 1)
        {
            //最大显示三分之一
            cellSize = Size / 3;
        }

        if (config.Function.Equals(RangeTypeDefine.Line))
        {
            maxRange = config.ComputeParams[0];
            StartPos.x = 0;
            StartPos.y = -cellSize * (float)maxRange / 2f + (cellSize / 2);
        }

        List<VInt2> cordinates = SkillProxy._instance.GetRangeCordinate(rangeID, new VInt2(0, 0));
        count = cordinates.Count;
        for (int i = 0; i < count; ++i)
        {
            float curX =  StartPos.x + cellSize * cordinates[i].x;
            float curY =   StartPos.y + cellSize * cordinates[i].y;

            Image img = GameObject.Instantiate<Image>(this._Templete,this.transform);
            img.rectTransform.localScale = Vector3.one;
            img.rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
            img.rectTransform.anchoredPosition = new Vector2(curX, curY);
            img.gameObject.SetActive(true);
            if (curX == StartPos.x && curY == StartPos.y)
                img.color = this._StartPointColor;
            else
                img.color = this._OtherPointColor;

            _saves.Add(img.gameObject);
        }
    }
}


