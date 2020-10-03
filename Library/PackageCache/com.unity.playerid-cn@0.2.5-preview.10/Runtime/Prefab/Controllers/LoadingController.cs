using UnityEngine;

namespace UnityEngine.PlayerIdentity.UI
{
    public class LoadingController : MonoBehaviour
    {
        private Animator m_LoadingAnimator;
        private Canvas m_Canvas;

        private const string k_LoadingKey = "isLoading";

        void Start()
        {
            m_Canvas = GetComponent<Canvas>();
            m_LoadingAnimator = GetComponentInChildren<Animator>();
        }

        public void ShowLoading(bool status)
        {
            m_Canvas.enabled = status;
            m_LoadingAnimator.SetBool(k_LoadingKey, status);
        }
    }
}
