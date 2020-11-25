using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BattleSpot : MonoBehaviour
    , IPointerClickHandler
{
    public List<Color> _Colors;
    private VInt2 Postion;
    private UnityAction<BattleSpot> _fun;
    public VInt2 Pos => this.Postion;

    void Awake()
    {

    }

    public void InitPostion(int x, int z)
    {
        this.Postion = new VInt2(x, z);
    }

    public void AddEvent(UnityAction<BattleSpot> callBack)
    {
        this._fun = callBack;
    }

    public void ChangeColor(BattleSpotStatus spotStatus)
    {
        this.GetComponent<MeshRenderer>().material.SetColor("_Color", this._Colors[(int)spotStatus]);
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
