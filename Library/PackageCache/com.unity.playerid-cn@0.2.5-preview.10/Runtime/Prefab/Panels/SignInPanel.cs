using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UI;

namespace UnityEngine.PlayerIdentity.UI
{
    public class SignInPanel : AbstractPanel
    {
        [Header("Providers")]

        [SerializeField]
        Transform loginProvidersContainer = default;

        [SerializeField]
        GameObject emailLogin = default;

        [SerializeField]
        GameObject phoneLogin = default;

        [SerializeField]
        GameObject anonymousLogin = default;

        IPlayerIdentityCore playerIdentityCore;
        bool loginProvidersPopulated;

        MainController mainController;

        // Used by customization tool
        public List<GameObject> genericProviderButtonsList;

        void Start()
        {
            mainController = MainController.Instance;
            playerIdentityCore = PlayerIdentityManager.Current;
        }

        private void FixedUpdate()
        {
            BuildProvidersList();
        }

        /// <summary>
        /// Add all providers button with listener in the panel
        /// </summary>
        private void BuildProvidersList()
        {
            if (loginProvidersPopulated)
            {
                return;
            }
            
            // Delay the initialization until playerIdentityCore.Start() is done
            if (playerIdentityCore.loginBackend == null)
            {
                return;
            }
            
            var loginBackendSubsystemDescriptor = playerIdentityCore.loginBackend.SubsystemDescriptor;
            if (loginBackendSubsystemDescriptor.supportsEmailPasswordLogin)
            {
                Button emailLoginButton = Instantiate(emailLogin, loginProvidersContainer).GetComponent<Button>();

                emailLoginButton.onClick.RemoveAllListeners();
                emailLoginButton.onClick.AddListener(() => OnEmailLoginClicked());
            }

            {
                Button phoneLoginButton = Instantiate(phoneLogin, loginProvidersContainer).GetComponent<Button>();
                phoneLoginButton.onClick.RemoveAllListeners();
                phoneLoginButton.onClick.AddListener(() => OnPhoneLoginClicked());
            }
            
            foreach (var provider in playerIdentityCore.loginProviders)
            {
                string id = provider.SubsystemDescriptor.loginProviderID;

                Button button = Instantiate(provider.GetButton(), loginProvidersContainer).GetComponent<Button>();
                button.GetComponent<Button>().onClick.RemoveAllListeners();
                button.GetComponent<Button>().onClick.AddListener(() => OnExternalLoginClicked(id));
            }

            if (loginBackendSubsystemDescriptor.supportsAnonymousLogin)
            {
                Button anonymousLoginButton = Instantiate(anonymousLogin, loginProvidersContainer).GetComponent<Button>();

                anonymousLoginButton.onClick.RemoveAllListeners();
                anonymousLoginButton.onClick.AddListener(() => OnAnonymousLoginClicked());
            }

            loginProvidersPopulated = true;
        }

        private void OnEmailLoginClicked()
        {
            PanelController panelController = mainController.PanelController;
            panelController.OpenPanel(panelController.GetComponentInChildren<EmailSignInPanel>() as AbstractPanel);
        }
        
        private void OnPhoneLoginClicked()
        {
            PanelController panelController = mainController.PanelController;
            PhoneSignInPanel.LoginAction = PhoneSignInPanel.LoginActions.SignIn;
            panelController.OpenPanel(panelController.GetComponentInChildren<PhoneSignInPanel>());
        }

        private void OnAnonymousLoginClicked()
        {
            mainController.ShowLoading(true);
            playerIdentityCore.LoginAnonymous();
        }

        private void OnExternalLoginClicked(string providerId)
        {
            try
            {
                playerIdentityCore.ExternalLogin(providerId);
                mainController.ShowLoading(true);
            }
            catch (Exception e)
            {
                Utils.Logger.Exception(e);
                mainController.PopupController.ShowError(new Error { message = "An error occurred while trying to connect to this provider." });
            }
        }

        /// <summary>
        /// Destroy all providers button in the panel
        /// </summary>
        public void CleanProvidersList()
        {
            foreach (Transform child in loginProvidersContainer.transform)
                Destroy(child.gameObject);
        }

        public void ReBuildProvidersList()
        {
            CleanProvidersList();
            BuildProvidersList();
        }
    }
}