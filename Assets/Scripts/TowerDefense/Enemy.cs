using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float _hp = 100;
    public float _speed = 10f;
    public GameObject _dieEffect;

    private Slider _bloodSlider;


    private Transform[] _positions;
    private int _length = 0;
    private int _index = 0;//移动的下标
    private float _reachGap;//到达某个点的距离判断

    private void Awake()
    {
        this._bloodSlider = this.transform.Find("Canvas/Slider").GetComponent<Slider>();
    }
    void Start()
    {
        this._positions = WayPoints.positions;
        this._length = this._positions.Length;
        this._reachGap = this._speed / 100f;
        
        this._bloodSlider.wholeNumbers = true;
        this._bloodSlider.maxValue = this._hp;
        this._bloodSlider.minValue = 0;
        this._bloodSlider.value = this._hp;
    }

    public void TakeDemage(float demage)
    {
        if (this._hp <= 0)
            return;
        this._hp -= demage;
        this._bloodSlider.value = this._hp;
        if (this._hp <= 0)
        {
            this.Die();
        }
    }

    private void Die()
    {
        GameObject dieeffect =  GameObject.Instantiate(this._dieEffect, this.transform.position, Quaternion.identity);
        GameObject.Destroy(dieeffect, 0.6f);
        GameObject.Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        this.Move();
    }

    private void Move()
    {
        if (this._index >= this._length)
        {
            //失败了
            return;
        }    
        Vector3 dir = (_positions[this._index].position - this.transform.position).normalized;//移动方向的单位向量
        this.transform.Translate(dir * Time.deltaTime * this._speed);
        if (Vector3.Distance(_positions[this._index].position, this.transform.position) < this._reachGap)
        {
            //到达位置
            this._index++;
        }

        if (this._index == this._length)
        {
            //到达目的地了
            this.ReachDestination();
        }
    }

    private void ReachDestination()
    {
        GameObject.Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        EnemySpawner.OneEnemyDestory();
    }
}
