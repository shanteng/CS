using Newtonsoft.Json.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeLandManager : MonoBehaviour
{
    public static float Degree = 45;

    public static int COL_COUNT = 50;
    public static int ROW_COUNT = 50;
    public List<SpotCube> _spotPrefabs;
    public Building _BuildPrefab;

    private Dictionary<string, SpotCube> _allSpotDic;//key格式x|y,存储当前所有的地块
    private static HomeLandManager instance;

    public static HomeLandManager GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
       // this.InitScene();
    }

    public void Build(int x, int z)
    {
        Building baseSpot = GameObject.Instantiate<Building>(this._BuildPrefab, new Vector3(x, 1, z), Quaternion.identity, this.transform);
        string key = UtilTools.combine("build",x, "|", z);
        baseSpot.name = key;
        baseSpot.Init();
        baseSpot.SetCordinate(x, z);
    }

    public void InitScene()
    {
        this._allSpotDic = new Dictionary<string, SpotCube>();
        this.GenerateAllBaseSpot();
    }

    private void GenerateAllBaseSpot()
    {
        for (int row = 0; row < ROW_COUNT; ++row)
        {
            int corX = row;
            int start = row % 2;
            for (int col = 0; col < COL_COUNT; ++col)
            {
                int curIndex = (start + 1 +col) % 2;
                SpotCube prefab = this._spotPrefabs[curIndex];
                int corZ = col;
                SpotCube baseSpot = GameObject.Instantiate<SpotCube>(prefab, new Vector3(corX, 0, corZ), Quaternion.identity, this.transform);
                baseSpot.SetCordinate(corX, corZ);
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
