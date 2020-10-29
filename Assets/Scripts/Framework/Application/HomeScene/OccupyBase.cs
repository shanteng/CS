using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OccupyBase : MonoBehaviour
{
    public GameObject _Arrow;


    public void SetArrowVisible(bool vi,bool isDisableDrag = false)
    {
        this._Arrow.SetActive(vi &&  !isDisableDrag);
    }
}
