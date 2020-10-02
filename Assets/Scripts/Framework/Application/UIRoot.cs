
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
    FullScreen = 0,
    Window,
    Popup,
    Tips,
    Mask

}
public class UIRoot : MonoBehaviour
{
    public Camera _uiCamera;
    public List<Transform> _windowLayerDic;
    private AudioSource _audioSource;
    public static string CurFullWindow = string.Empty;

    public static UIRoot Intance { get; private set; }
    void Awake()
    {
        Intance = this;
        this._audioSource = this.gameObject.AddComponent<AudioSource>();
        foreach (Transform layer in this._windowLayerDic)
        {
            layer.gameObject.SetActive(true);
        }
    }

    public Transform GetLayer(WindowLayer layer)
    {
        return this._windowLayerDic[(int)layer];
    }
}