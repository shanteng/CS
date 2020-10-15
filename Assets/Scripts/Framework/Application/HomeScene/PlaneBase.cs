using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaneBase : MonoBehaviour
{
    public List<Color> _colors;
    private MeshRenderer _render;



    public void SetSize(int row, int col)
    {
        if(this._render == null)
            this._render = this.GetComponent<MeshRenderer>();
        this.transform.localScale = new Vector3(row, col, 1);
        _render.material.SetVector("_MainTex_ST", new Vector4(row, col, 0, 0));

    }

    public void SetColorIndex(int index)
    {
        this._render.material.color = this._colors[index];
    }
}
