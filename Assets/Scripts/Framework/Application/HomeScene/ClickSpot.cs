using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickSpot : MonoBehaviour
{
    public int x=-1;
    public int z=-1;

    public void JudegeShow(int targetX, int targetZ)
    {
        bool isVisible = this.gameObject.activeSelf;
        

        this.gameObject.SetActive(targetX != x || targetZ != z || isVisible == false);
        this.x = targetX;
        this.z = targetZ;
        this.transform.position = new Vector3(x, 0.51f, z);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
