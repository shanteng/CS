using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickSpot : MonoBehaviour
{
    public int x = -1;
    public int z = -1;

    public bool JudegeSpotShow(int targetX, int targetZ)
    {
        bool isVisibleSpot = WorldProxy._instance.IsSpotVisible(targetX, targetZ);
        bool isVisible = this.gameObject.activeSelf;
        bool isShow = (targetX != x || targetZ != z || isVisible == false) && isVisibleSpot == false;
        if (isShow)
            this.ShowInPostion(targetX, targetZ);
        else
            this.Hide();
        return isShow;
    }

    public void ShowInPostion(int targetX, int targetZ)
    {
        this.x = targetX;
        this.z = targetZ;
        this.gameObject.SetActive(true);
        this.transform.position = new Vector3(x, 0.51f, z);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
