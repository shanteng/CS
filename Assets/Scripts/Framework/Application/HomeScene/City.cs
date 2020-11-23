using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class City : MonoBehaviour
    , IPointerClickHandler
{
    private int _id;
    private CityUI _Ui;
    public int ID => this._id;
    private void Awake()
    {
       
    }

    public void SetCity(int id)
    {
        this._id = id;
        CityConfig config = CityConfig.Instance.GetData(id);
        GameObject prefab = ResourcesManager.Instance.LoadLandRes("CityUI");
        GameObject obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, this.transform);
        obj.transform.localPosition = Vector3.zero;
        this._Ui = obj.GetComponent<CityUI>();
        this._Ui.SetUI(config);
    }

    public void UpdateOwn()
    {
        this._Ui.UpdateOwn();
    }



    public void OnPointerClick(PointerEventData eventData)
    {
        HomeLandManager.GetInstance().OnClickNpcCity(this.ID);
    }
}
