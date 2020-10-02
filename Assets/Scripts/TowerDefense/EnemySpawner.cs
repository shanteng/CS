using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public float _waveRate = 3f;
    public Transform _start;
    public Wave[] _waves;
    public static int AliveCount = 0;//当前存活的数量
    // Start is called before the first frame update

    private void Start()
    {
        StartCoroutine(this.SpownEnemy());
    }

    IEnumerator SpownEnemy()
    {
        foreach (Wave wave in this._waves)
        {
            int count = wave._count;
            for (int i = 0; i < count; ++i)
            {
                GameObject.Instantiate(wave._enemyPrefab, this._start.position, Quaternion.identity);
                OneEnemyGenerate();
                if(i<count-1)
                    yield return new WaitForSeconds(wave._rate);
            }

            while (EnemySpawner.AliveCount > 0)
            {
                yield return 0;
            }

            yield return new WaitForSeconds(_waveRate);
        }
    }

    public static void OneEnemyGenerate()
    {
        EnemySpawner.AliveCount++;
    }

    public static void OneEnemyDestory()
    {
        EnemySpawner.AliveCount--;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
