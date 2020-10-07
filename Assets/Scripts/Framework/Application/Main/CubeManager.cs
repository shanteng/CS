using Newtonsoft.Json.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeManager : MonoBehaviour
{
    public int COL_COUNT = 50;
    public int ROW_COUNT = 50;
    public SpotCube _prefab;

    private Dictionary<string, SpotCube> _allSpotDic;//key格式x|y,存储当前所有的地块
    //private static CubeManager instance;
    // Start is called before the first frame update

 //   public static CubeManager GetInstance()
 //   {
 //       return instance;
 //   }

    void Awake()
    {
        //instance = this;
    }

    public void InitScene()
    {
        this._allSpotDic = new Dictionary<string, SpotCube>();
        this.GenerateAllBaseSpot();
    }

    private void GenerateAllBaseSpot()
    {
        for (int row = 0; row < this.ROW_COUNT; ++row)
        {
            int corX = row;
            for (int col = 0; col < this.COL_COUNT; ++col)
            {
                int corZ = col;
                SpotCube baseSpot = GameObject.Instantiate<SpotCube>(this._prefab, new Vector3(corX, 0, corZ), Quaternion.identity, this.transform);
                baseSpot.transform.localPosition = new Vector3(corX, 0, corZ);
                baseSpot.transform.localRotation = Quaternion.identity;
                string key = UtilTools.combine(corX, "|", corZ);
                baseSpot.name = key;
                this._allSpotDic[key] = baseSpot;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
