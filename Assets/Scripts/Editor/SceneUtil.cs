#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneUtil 
{
    [MenuItem("SceneUtil/启动游戏")]
    public static void StartGame()
    {
        if (EditorApplication.isPlaying) return;
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/Scenes/GameIndex.unity");
        EditorApplication.isPlaying = true;
    }

    [MenuItem("SceneUtil/打开GameIndex场景")]
    public static void EnterGameindex()
    {
        if (EditorApplication.isPlaying) return;
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/Scenes/GameIndex.unity");
    }

    [MenuItem("SceneUtil/打开Json场景")]
    public static void EnterJson()
    {
        if (EditorApplication.isPlaying) return;
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/Scenes/Json.unity");
    }

    [MenuItem("SceneUtil/打开Home场景")]
    public static void EnterHome()
    {
        if (EditorApplication.isPlaying) return;
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
    }
}
#endif