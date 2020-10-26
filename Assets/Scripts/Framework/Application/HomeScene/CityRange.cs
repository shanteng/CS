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
    public float _constY = 0f;


    private void Awake()
    {
       
    }

    public void SetRange(int row, int col)
    {
        float rowOffset = row/2 + 1;
        float colOffset = col/2 + 1;
        float rowZlen = row + 2.5f;
        float colZlen = col + 2.5f;


        this._Left.localScale = new Vector3(0.5f, 1, colZlen);
        this._Right.localScale = new Vector3(0.5f, 1, colZlen);
        this._Top.localScale = new Vector3(0.5f, 1, rowZlen);
        this._Bottom.localScale = new Vector3(0.5f, 1, rowZlen);

        this._Left.localPosition = new Vector3(-colOffset, this._constY, 0);
        this._Right.localPosition = new Vector3(colOffset, this._constY, 0);
        this._Top.localPosition = new Vector3(0, this._constY, rowOffset);
        this._Bottom.localPosition = new Vector3(0, this._constY, -rowOffset);

    }

}
