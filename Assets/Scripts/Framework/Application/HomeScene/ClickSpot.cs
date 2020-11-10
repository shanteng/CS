using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickSpot : MonoBehaviour
{
    public int x=-1;
    public int z=-1;

    public bool JudegeShow(int targetX, int targetZ)
    {
        bool isVisibleSpot = WorldProxy._instance.IsSpotVisible(targetX, targetZ);
        bool isVisible = this.gameObject.activeSelf;
        bool isShow = targetX != x || targetZ != z || isVisible == false;
        this.gameObject.SetActive(isShow && isVisibleSpot ==false);
        this.x = targetX;
        this.z = targetZ;
        this.transform.position = new Vector3(x, 0.51f, z);
        return gameObject.activeSelf;
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
