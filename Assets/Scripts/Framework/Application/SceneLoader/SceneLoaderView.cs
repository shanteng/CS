
using UnityEngine;
using SMVC.Interfaces;
using SMVC.Patterns;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoaderView : MonoBehaviour
{
    public Text _ProgressText;
    public Slider _SceneSlider;
    private string _loadName;
    
    void Awake()
    {
       // SceneManager.UnloadSceneAsync(scene);
    }

    public void LoadScene(string name,bool isAddtive)
    {
        this._loadName = name;
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(LoadScene(isAddtive));
    }

    IEnumerator LoadScene(bool isAddtive)
    {
        yield return null;
        AsyncOperation asyncOperation;
        if (isAddtive == false)
            asyncOperation = SceneManager.LoadSceneAsync(this._loadName);
        else
            asyncOperation = SceneManager.LoadSceneAsync(_loadName, LoadSceneMode.Additive);

        //不激活场景
        asyncOperation.allowSceneActivation = true;
        //当加载正在进行时，加载Text和进度条
        while (!asyncOperation.isDone)
        {
            if (asyncOperation.progress >= 0.9f)
            {
                //激活场景加载完成了
                this.SetProgress(1f);
            }
            else
            {
                this.SetProgress(asyncOperation.progress);
            }
            yield return null;
        }
    }//end func

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        MediatorUtil.SendNotification(NotiDefine.LoadSceneFinish, this._loadName);
    }

    private void SetProgress(float progress)
    {
        this._SceneSlider.value = progress;
        float showValue = this._SceneSlider.value * 100f;
        string strProgress = LanguageConfig.GetLanguage(LanMainDefine.Percent,showValue);
        _ProgressText.text = strProgress;
    }

}//end class