using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BornSpot : MonoBehaviour
    , IPointerClickHandler
{
    public int _id;
    public GameObject _select;
    public GameObject Light;
    private BattlePlace _place;
    private Vector3 _Postion;
    private UnityAction<BornSpot> _fun;
    public int ID => this._id;
    void Awake()
    {
        
    }

    public void Init(int id,Vector3 pos,BattlePlace place,BattlePlace myPlace)
    {
        this._id = id;
        this._Postion = pos;
        this._place = place;
        if(place == BattlePlace.Attack)
            this.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(0.37f, 0.64f, 1, 1));
        else
            this.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(1, 0.32f, 0.32f, 1));
        this.SetSelect(false);

        this.Light.SetActive(myPlace == place);
    }


    public void SetSelect(bool isse)
    {
        this._select.SetActive(isse);
    }

    public void AddEvent(UnityAction<BornSpot> callBack)
    {
        this._fun = callBack;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (this._fun == null)
            return;

        if (UtilTools.isFingerOverUI() == false)
        {
            float distancex = (eventData.pressPosition.x - eventData.position.x) * (eventData.pressPosition.x - eventData.position.x);
            float distancey = (eventData.pressPosition.y - eventData.position.y) * (eventData.pressPosition.y - eventData.position.y);
            float distrance = Mathf.Sqrt(distancex * distancex + distancey * distancey);

            if (distrance < 1)
            {
                this._fun.Invoke(this);
            }
        }
    }


}
