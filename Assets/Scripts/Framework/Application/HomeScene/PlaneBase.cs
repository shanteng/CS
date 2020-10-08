using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaneBase : MonoBehaviour
{
    private List<MeshRenderer> _allRenders;
    public List<Color> _colors;
  

    void Awake()
    {
        _allRenders = new List<MeshRenderer>();
        int count = this.transform.childCount;
        for (int i = 0; i < count; ++i)
        {
            MeshRenderer render = this.transform.GetChild(i).GetComponent<MeshRenderer>();
            if (render == null)
                continue;
            _allRenders.Add(render);
        }
    }


    public void SetColorIndex(int index)
    {
        foreach (MeshRenderer render in this._allRenders)
        {
            render.material.color = this._colors[index];
        }
    }
}
