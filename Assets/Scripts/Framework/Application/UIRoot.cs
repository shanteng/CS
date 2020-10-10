
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;

public enum WindowLayer
{
    Main = 0,
    FullScreen,
    Window,
    Popup,
    Tips,
    Mask

}
public class UIRoot : MonoBehaviour
{
    [HideInInspector]
    public Camera camera;
    private List<Transform> _windowLayers;
    private AudioSource _audioSource;
    public static string CurFullWindow = string.Empty;

    public static UIRoot Intance { get; private set; }
    void Awake()
    {
        this.camera = this.transform.parent.GetComponent<Camera>();
        Intance = this;
        this._audioSource = this.gameObject.AddComponent<AudioSource>();
        this._windowLayers = new List<Transform>();
        int count = this.transform.childCount;
        for (int i = 0; i < count; ++i) 
        {
            Transform layer = this.transform.GetChild(i);
            layer.gameObject.SetActive(true);
            this._windowLayers.Add(layer);
        }
    }

    public Transform GetLayer(WindowLayer layer)
    {
        return this._windowLayers[(int)layer];
    }
}