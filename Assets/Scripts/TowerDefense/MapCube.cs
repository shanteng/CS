using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapCube : MonoBehaviour
{
    private GameObject _turretObj;//当前创建的炮台
    public GameObject _effect;
    private Renderer _render;
    private Color _orignColor;
    void Start()
    {
        this._render = this.gameObject.GetComponent<Renderer>();
        this._orignColor = this._render.material.color;
    }

    public void BuildTurret(TurretData data)
    {
        this._turretObj = GameObject.Instantiate(data._prefab, this.transform.position, Quaternion.identity);
        this._turretObj.GetComponent<Turret>()._isMaxLevel = false; 
        GameObject eff =  GameObject.Instantiate(_effect, this.transform.position, Quaternion.identity);
        Destroy(eff, 1f);
    }

    public void UpgradeTurrent(TurretData data)
    {
        GameObject.Destroy(this._turretObj);
        this._turretObj = GameObject.Instantiate(data._prefabUpgrade, this.transform.position, Quaternion.identity);
        this._turretObj.GetComponent<Turret>()._isMaxLevel = true;
        GameObject eff = GameObject.Instantiate(_effect, this.transform.position, Quaternion.identity);
        Destroy(eff, 1f);
    }

    public void DestoryTurret()
    {
        GameObject.Destroy(this._turretObj);
        this._turretObj = null;
    }

    public bool HasTurret()
    {
        return this._turretObj != null;
    }

    public Turret getCurrentTurret()
    {
        return this._turretObj != null ? this._turretObj.GetComponent<Turret>() : null;
    }

    private void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        if (this.HasTurret() == false)
            this._render.material.color = Color.red;
        else
        {
            this._render.material.color = Color.green;
            BuildingManager.GetInstance().ShowRange(this._turretObj.transform, this._turretObj.GetComponent<SphereCollider>().radius);
        }
            
    }

    private void OnMouseExit()
    {
        this._render.material.color = _orignColor;
        BuildingManager.GetInstance().HideRange();
    }

}
