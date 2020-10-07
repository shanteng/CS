using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Building : MonoBehaviour
    , IPointerDownHandler
    , IPointerClickHandler
    , IPointerUpHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
       // ViewControllerLocal.GetInstance().PressSpot(this.gameObject.name);
    }

    //监听抬起
    public void OnPointerUp(PointerEventData eventData)
    {
       // ViewControllerLocal.GetInstance().PressSpot("");
    }

    //监听点击
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.LogError("OnPointerClick" + this.gameObject.name);
      //  ViewControllerLocal.GetInstance().SetSelectSpot(this.gameObject.name);
     //   this.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
    }
}
