using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ColorFlash : MonoBehaviour
{
    public Color _orignColor = Color.white;
    public Color _EndColor;
 
    private List<MeshRenderer> _allRenders;
    public float _Speed = 1f;
    private Color _curColor;
    private bool _isStart = false;
    private float _delatTime = 0;
    

    private void Awake()
    {
        this.Init();
    }

    private void Init()
    {
        if (this._allRenders != null)
            return;
        _curColor = this._orignColor;
        _allRenders = new List<MeshRenderer>();
        int count = this.transform.childCount;
        for (int i = 0; i < count; ++i)
        {
            MeshRenderer render = this.transform.GetChild(i).GetComponent<MeshRenderer>();
            if (render == null)
                continue;
            render.material.color = this._curColor;
            _allRenders.Add(render);
        }
    }

    public void DoFlash(bool start)
    {
        this.Init();
        _delatTime = 0;
        this._isStart = start;
        if (start == false)
            this.SetOrignal();
    }

    public void DoTransparent()
    {
        this._isStart = false;
        _curColor = new Color(this._orignColor.r,this._orignColor.g,this._orignColor.b,0.5f);
        foreach (MeshRenderer render in this._allRenders)
        {
            render.material.color = this._curColor;
        }
    }

    public void SetOrignal()
    {
        this.Init();
        this._isStart = false;
        _curColor = this._orignColor;
        this.SetColor();
    }

    void Update()
    {
        if (this._isStart && this._allRenders != null)
        {
            this._delatTime += Time.deltaTime * this._Speed;
            _curColor = Color.Lerp(this._orignColor, this._EndColor, Mathf.PingPong(this._delatTime, 1));
            this.SetColor();
        }
    }


    private void SetColor()
    {
        foreach (MeshRenderer render in this._allRenders)
        {
            render.material.color = this._curColor;
        }
    }
}
