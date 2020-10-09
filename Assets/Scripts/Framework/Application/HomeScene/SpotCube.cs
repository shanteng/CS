using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpotCube : MonoBehaviour
    , IPointerClickHandler
{
    private Vector3Int _cordinate = Vector3Int.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCordinate(int x, int z)
    {
        this._cordinate.x = x;
        this._cordinate.z = z;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //测试代码
        if( EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId) == false)
        HomeLandManager.GetInstance().OnClickSpotCube(this._cordinate.x,this._cordinate.z);
    }


}
