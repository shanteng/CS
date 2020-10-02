using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewController : MonoBehaviour
{
    public float _speed = 10f;
    public float _mouseSpeed = 60f;
    void Update()
    {
        float h = Input.GetAxis("Horizontal") * _speed;//ad
        float v = Input.GetAxis("Vertical")* _speed;//ws
        float mouse = Input.GetAxis("Mouse ScrollWheel") * _mouseSpeed;

        this.transform.Translate(new Vector3(h, -mouse, v)*Time.deltaTime,Space.World);

    }
}
