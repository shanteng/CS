using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public bool _isMaxLevel = false;
    public float _attackRate = 0.1f;//攻击频率
    public GameObject _misslePrefab;
    public Transform _cannoTran;//枪口
    public Transform _head;//枪口

    private float _timer = 0;
    public List<GameObject> _enemys = new List<GameObject>();

    public LineRenderer _laser;
    public Transform _laserEffect;
    public float _demageOneSecs = 50;

    private void Start()
    {
        this._timer = this._attackRate;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Enemy"))
        {
            _enemys.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _enemys.Remove(other.gameObject);
    }

    private void judgeEmpty()
    {
        int count = this._enemys.Count;
        for (int i = count - 1; i >= 0; --i)
        {
            if (this._enemys[i] == null)
            {
                this._enemys.RemoveAt(i);
            }
        }
    }

    private void Update()
    {
        this._timer += Time.deltaTime;

        if (this._enemys.Count > 0 && this._enemys[0] == null)
        {
            this.judgeEmpty();
        }

        if (this._enemys.Count > 0 && this._enemys[0] != null)
        {
            Vector3 targetPos = _enemys[0].transform.position;
            targetPos.y = this._head.position.y;
            this._head.LookAt(targetPos);
        }


        if (this._laser == null && this._timer >= this._attackRate && _enemys.Count > 0)
        {
            //普通炮台
            this._timer = 0;
            this.Attack();
        }
        else if (this._laser != null && _enemys.Count > 0 && this._enemys[0] != null)
        {
            this.LaserAttack();
        }
        else if (this._laser != null && _enemys.Count == 0 )
        {
            this._laser.enabled = false;
            this._laserEffect.gameObject.SetActive(false);
        }
    }

    private void LaserAttack()
    {
        if (this._laser.enabled == false)
        {
            this._laser.enabled = true;
            this._laserEffect.gameObject.SetActive(true);
        }
            
        this._laser.SetPositions(new Vector3[] { this._cannoTran.position, _enemys[0].transform.position });
        this._enemys[0].GetComponent<Enemy>().TakeDemage(this._demageOneSecs * Time.deltaTime);
        this._laserEffect.position = this._enemys[0].transform.position;
        Vector3 lookatPos = this.transform.position;
        lookatPos.y = this._enemys[0].transform.position.y;
        this._laserEffect.LookAt(lookatPos);
    }

    private void Attack()
    {
        if (this._enemys.Count > 0)
        {
            Bullet bullet = GameObject.Instantiate(this._misslePrefab, this._cannoTran.position, _cannoTran.rotation).GetComponent<Bullet>();
            bullet.SetTarget(_enemys[0].transform);
            GameObject.Destroy(bullet.gameObject, 1f);
        }
        else
        {
            this._timer = this._attackRate;
        }
       
    }
}
