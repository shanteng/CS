using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ColorFlash : MonoBehaviour
{
    private List<MeshRenderer> _allRenders;
    public List<MeshRenderer> _ignorls;

    public Color _EndColor;
    public float _Speed = 1f;
    private Color _curColor;
    private bool _isStart = false;
    private float _delatTime = 0;
   
    void Awake()
    {
        _allRenders = new List<MeshRenderer>();
        int count = this.transform.childCount;
        for (int i = 0; i < count; ++i)
        {
            MeshRenderer render = this.transform.GetChild(i).GetComponent<MeshRenderer>();
            if (render == null)
                continue;
            if (this._ignorls.Contains(render))
                continue;
            _allRenders.Add(render);
        }
    }

    public void DoFlash(bool start)
    {
        _delatTime = 0;
        this._isStart = start;
        if (start == false)
            this.Stop();
    }

    public void Stop()
    {
        this._isStart = false;
        _curColor = Color.white;
        this.SetColor();
    }

    void Update()
    {
        if (this._isStart)
        {
            this._delatTime += Time.deltaTime * this._Speed;
            _curColor = Color.Lerp(Color.white, this._EndColor, Mathf.PingPong(this._delatTime, 1));
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
