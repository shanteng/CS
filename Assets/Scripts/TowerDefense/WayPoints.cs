using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WayPoints : MonoBehaviour
{
    public static Transform[] positions;
    // Start is called before the first frame update
    void Awake()
    {
        positions = new Transform[transform.childCount];
        for (int i = 0; i < positions.Length; ++i)
        {
            positions[i] = this.transform.GetChild(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
