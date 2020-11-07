using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SpotCube : MonoBehaviour
    , IPointerClickHandler
{
    private UnityAction<Vector3> _fun;
    public void AddEvent(UnityAction<Vector3> callBack)
    {
        this._fun = callBack;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (UtilTools.isFingerOverUI() == false)
        {
            float distancex = (eventData.pressPosition.x - eventData.position.x) * (eventData.pressPosition.x - eventData.position.x);
            float distancey = (eventData.pressPosition.y - eventData.position.y) * (eventData.pressPosition.y - eventData.position.y);

            float distrance = Mathf.Sqrt(distancex * distancex + distancey * distancey);

            if (distrance < 1)
            {
                //Debug.LogWarning("OnPointerClick:" + eventData.delta);
                this._fun.Invoke(eventData.pointerCurrentRaycast.worldPosition);
            }
        }
    }


}
