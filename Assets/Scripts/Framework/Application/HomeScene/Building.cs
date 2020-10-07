using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Building : MonoBehaviour
    , IPointerDownHandler
    , IPointerClickHandler
    , IPointerUpHandler
    ,IDragHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        // var curPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        float CosDegreeValue = Mathf.Cos(45 * Mathf.Deg2Rad);
        float xoffset = (eventData.delta.x) * CosDegreeValue;
        float zoffset = eventData.delta.x * CosDegreeValue;

        float xxof = -eventData.delta.y * CosDegreeValue;
        float zzof = eventData.delta.y * CosDegreeValue;


        this.transform.Translate(new Vector3(xoffset+xxof, 0, zoffset+zzof) *Time.deltaTime, Space.Self);
        Debug.LogError("position:" + this.transform.position);
     //   curPosition.y = 1;
        //this.transform.position = curPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ViewControllerLocal.GetInstance().PressSpot(this.gameObject.name);
    }

    //监听抬起
    public void OnPointerUp(PointerEventData eventData)
    {
        ViewControllerLocal.GetInstance().PressSpot("");
    }

    //监听点击
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.LogError("OnPointerClick" + this.gameObject.name);
        ViewControllerLocal.GetInstance().SetSelectSpot(this.gameObject.name);
     //   this.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
    }
}
