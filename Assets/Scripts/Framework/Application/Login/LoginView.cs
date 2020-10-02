using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginView : MonoBehaviour
{
    public UIButton _BtnStart;
    public Text _ProgressText;
    public Slider _SceneSlider;
    void Start()
    {
        _BtnStart.AddEvent(this.OnClickStart);
        this._SceneSlider.gameObject.SetActive(false);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnClickStart(UIButton btn)
    {
        this._BtnStart.Hide();
        StartCoroutine(LoadScene());
        this._SceneSlider.gameObject.SetActive(true);
    }

    IEnumerator LoadScene()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(SceneDefine.Main);
        //不激活场景
        asyncOperation.allowSceneActivation = false;
        //当加载正在进行时，加载Text和进度条
        while (!asyncOperation.isDone)
        {
            _ProgressText.text = UtilTools.combine(asyncOperation.progress * 100, "%");
            this._SceneSlider.value = asyncOperation.progress;
            if (asyncOperation.progress >= 0.9f)
            {
                // m_Text.text = "Press the space bar to continue";
                // if (Input.GetKeyDown(KeyCode.A))
                //激活场景
                asyncOperation.allowSceneActivation = true;
            }
            yield return null;
        }

    }


    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        this._SceneSlider.value =1f;
        _ProgressText.text = UtilTools.combine(100, "%");
        MediatorUtil.SendNotification(NotiDefine.LOAD_MAIN_SCENE_FINISH);
    }

}
