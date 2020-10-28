using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class CityRange : MonoBehaviour
{
    public Transform _Left;
    public Transform _Right;
    public Transform _Top;
    public Transform _Bottom;
   

    private void Awake()
    {
       
    }

    public void SetRange(int range)
    {
        float Offset = range / 2 + 1;
     //   float colOffset = col/2 + 1;
        float Len = range + 1.5f;
     //   float colZlen = col + 1.5f;

        Vector3 size = this._Left.localScale;
        size.z = Len;

        this._Left.localScale = size;
        this._Right.localScale = size;
        this._Top.localScale = size;
        this._Bottom.localScale = size;

        this._Left.localPosition = new Vector3(-Offset, 0, 0);
        this._Right.localPosition = new Vector3(Offset, 0, 0);
        this._Top.localPosition = new Vector3(0, 0, Offset);
        this._Bottom.localPosition = new Vector3(0, 0, -Offset);

    }

}
