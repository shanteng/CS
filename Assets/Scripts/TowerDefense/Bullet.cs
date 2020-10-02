using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int _demage = 10;
    public int _speed = 20;
    public GameObject _effectPrafab;
    private Transform _target;

    public void SetTarget(Transform t)
    {
        this._target = t;
    }

    // Update is called once per frame
    void Update()
    {
        if (this._target == null)
        {
            GameObject.Destroy(this.gameObject);
            return;
        }
        this.transform.LookAt(this._target);
        this.transform.Translate(Vector3.forward * this._speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Enemy"))
        {
            this.AttackEnemy(other.gameObject);
        }
    }

    private void AttackEnemy(GameObject enemyObj)
    {
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDemage(this._demage);
            GameObject effobj = GameObject.Instantiate(this._effectPrafab, enemy.transform.position, Quaternion.identity,enemy.transform);
            Destroy(effobj, 0.2f);
        }
        Destroy(this.gameObject);
    }

}
