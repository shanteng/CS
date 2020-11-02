using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ColorFlash : MonoBehaviour
{
    public Color _orignlColor = new Color(0.38f, 0.52f, 0.75f);
    private Color _EndColor = new Color(0.48f, 0.48f, 0.48f, 0.6f);
 
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
        _curColor = this._orignlColor;
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

    public void DoTransparent(bool isTrans)
    {
        this._isStart = false;
        float alpha = 1f;
        if (isTrans)
            alpha = 0.5f;
        else
            alpha = 1f;
        foreach (MeshRenderer render in this._allRenders)
        {
            render.material.SetFloat("_AlphaScale", alpha);
        }
    }

    public void SetOrignal()
    {
        this.Init();
        this._isStart = false;
        _curColor = this._orignlColor;
        this.SetColor();
    }

    void Update()
    {
        if (this._isStart && this._allRenders != null)
        {
            this._delatTime += Time.deltaTime * this._Speed;
            _curColor = Color.Lerp(this._orignlColor, this._EndColor, Mathf.PingPong(this._delatTime, 1));
            this.SetColor();
        }
    }


    private void SetColor()
    {
        foreach (MeshRenderer render in this._allRenders)
        {
            render.material.color = this._curColor;
            render.material.SetFloat("_AlphaScale", 1f);
        }
    }
}
