using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaneBase : MonoBehaviour
{
    private MeshRenderer _render;
    public List<Color> _colors;
  

    void Awake()
    {
        _render = this.transform.GetComponent<MeshRenderer>();
    }

    public void SetSize(int row, int col)
    {
        this.transform.localScale = new Vector3(row, col, 1);
        this._render.material.SetVector("_MainTex_ST", new Vector4(row, col, 0, 0));

    }

    public void SetColorIndex(int index)
    {
        _render.material.color = this._colors[index];
    }
}
