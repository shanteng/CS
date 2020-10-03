using System;

namespace UnityEngine.PlayerIdentity.UI
{
    public class MainController : MonoBehaviour
    {
        [SerializeField]
        PanelController panelController;

        public PanelController PanelController
        {
            get { return panelController; }
            private set { panelController = value; }
        }

        [SerializeField]
        LoginStatusController loginStatusController;

        public LoginStatusController LoginStatusController
        {
            get { return loginStatusController; }
            private set { loginStatusController = value; }
        }

        [SerializeField]
        PopupController popupController;

        public PopupController PopupController
        {
            get { return popupController; }
            private set { popupController = value; }
        }

        [SerializeField]
        LoadingController loadingController;
        
        public LoadingController LoadingController
        {
            get { return loadingController; }
            private set { loadingController = value; }
        }

        public static MainController Instance { get; private set; }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Instance = this;
            }
        }

        public void OnLogin()
        {
            LoginStatusController.OnLogin();
            ShowLoading(false);
            PanelController.OnClose();
        }

        public void OnLogout()
        {
            LoginStatusController.OnLogout();
            PanelController.OnClose();
        }

        public void OnError()
        {
            ShowLoading(false);
            PopupController.ShowError(PlayerIdentityManager.Current.error);
        }


        public void OnUpgradeAccountComplete()
        {
            PanelController.OnBack();
            ShowLoading(false);
            GetComponentInChildren<AccountPanel>().UpdatePanel();
        }

        public void ShowLoading(bool status)
        {
            LoadingController.ShowLoading(status);
        }
    }
}