using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityEngine.PlayerIdentity.UI
{
    public class AccountPanel : AbstractPanel
    {
        [Header("Containers")]

        [SerializeField]
        Transform emailSettingsContainer ;

        [SerializeField]
        Transform optionsContainer = default;

        [Header("Buttons")]

        [SerializeField]
        GameObject upgradeAccount = default; // upgrade from anonymous to email user

        [SerializeField]
        GameObject linkPhone = default;

        [SerializeField]
        GameObject unlinkAccount = default;

        [SerializeField]
        GameObject verifyEmailButton = default;

        [Header("User")]

        [SerializeField]
        TextMeshProUGUI userName = default;

        private bool m_AnonymousSignOutConfirmationSent;

        private bool m_UpdatingUserInfo;

        List<Button> m_CurrentAccountManagementOptions;
        IPlayerIdentityCore m_PlayerIdentityCore;
        MainController m_MainController;

        private const string PhoneLoginMethodId = "phone";
        private const string EmailLoginMethodId = "password";

        void Start()
        {
            m_CurrentAccountManagementOptions = new List<Button>();
            m_PlayerIdentityCore = PlayerIdentityManager.Current;
            m_MainController = MainController.Instance;
        }

        protected override void ShowPanel()
        {
            UpdatePanel();
            base.ShowPanel();
        }

        public void UpdatePanel()
        {
            m_MainController.ShowLoading(true);
            m_PlayerIdentityCore.GetUser(identityCore =>
            {
                BuildPanel();
                DisplayUserName();
                
                m_MainController.ShowLoading(false);
            });
        }


        private void BuildPanel()
        {
            UpdateEmailSettingsButtonsState();

            CleanAccountManagementOptions();

            if (IsUserAnonymouslyLoggedIn())
            {
                DisplayUpgradeAccountButton();
                DisplayLinkingOptions();
            }
            else
            {
                if (!IsUserConnectedViaEmail())
                {
                    DisplayUpgradeAccountButton();
                }

                DisplayLinkingOptions();
            }

            if (!IsUserConnectedViaPhone())
            {
                DisplayLinkPhoneButton();
            }
        }

        private void DisplayUserName()
        {
            string name = m_PlayerIdentityCore.userInfo?.displayName ?? m_PlayerIdentityCore.userInfo?.email;
            userName.text = string.IsNullOrEmpty(name) ? "游客" : name;

        }

        private void UpdateEmailSettingsButtonsState()
        {
            if (!IsUserConnectedViaEmail() || m_PlayerIdentityCore.userInfo?.emailVerified == true)
            {
                verifyEmailButton.SetActive(false);
            }
            else
            {
                verifyEmailButton.SetActive(true);
            }
        }

        private void DisplayUpgradeAccountButton()
        {
            if (m_PlayerIdentityCore.loginBackend.SubsystemDescriptor.supportsEmailPasswordLogin)
                AddAccountManagementOptionButton(upgradeAccount, () => OnUpgradeAccountClicked());
        }

        private void DisplayLinkPhoneButton()
        {
            // is supported?
            AddAccountManagementOptionButton(linkPhone, OnLinkPhoneClicked);
        }

        private void DisplayLinkingOptions()
        {
            foreach (var provider in m_PlayerIdentityCore.loginProviders)
            {
                string id = provider.SubsystemDescriptor.loginProviderID;

                if (!IsAlreadyLinkedToProvider(id))
                    AddAccountManagementOptionButton(provider.GetButton(), () => LinkProviderToAccount(id));
                else
                {
                    AddAccountManagementOptionButton(unlinkAccount, "解绑" + provider.SubsystemDescriptor.displayName, () => UnlinkProviderFromAccount(id));
                }
            }

            if (IsUnLinkable(EmailLoginMethodId))
            {
                AddAccountManagementOptionButton(unlinkAccount, "解绑邮箱", () => UnlinkProviderFromAccount(EmailLoginMethodId));
            }
        }

        private void AddAccountManagementOptionButton(GameObject buttonObj, UnityAction callbackAction)
        {
            AddAccountManagementOptionButton(buttonObj, null, callbackAction);
        }

        private void AddAccountManagementOptionButton(GameObject buttonObj, string text, UnityAction callbackAction)
        {
            if(buttonObj != null)
            {
                Button button = Instantiate(buttonObj, optionsContainer).GetComponent<Button>();
                button.GetComponent<Button>().onClick.RemoveAllListeners();
                button.GetComponent<Button>().onClick.AddListener(callbackAction);
                if (text != null)
                    button.GetComponent<PrimaryActionButton>().buttonText.text = text;
                m_CurrentAccountManagementOptions.Add(button);
            }
        }

        /// <summary>
        /// Clean buttons list so that when you open the panel again, there are no doubles
        /// </summary>
        private void CleanAccountManagementOptions()
        {
            if (m_CurrentAccountManagementOptions.Count > 0){
                foreach (Button btn in m_CurrentAccountManagementOptions)
                    Destroy(btn.gameObject);

                m_CurrentAccountManagementOptions.Clear();
            }
        }

        private void LinkProviderToAccount(string providerId)
        {
            m_PlayerIdentityCore.Link(providerId);
        }

        private void UnlinkProviderFromAccount(string providerId)
        {
            if (m_PlayerIdentityCore.userInfo.externalIds.Length <= 1)
            {
                MainController.Instance.PopupController.ShowInfo("不能删除唯一关联帐号");
                return;
            }
            
            string[] linkedProvidersId = new string[1];
            linkedProvidersId[0] = providerId;

            MainController.Instance.ShowLoading(true);
            m_PlayerIdentityCore.Unlink(linkedProvidersId);
        }

        private bool IsUnLinkable(string loginMethodId)
        {
            var externalIds = m_PlayerIdentityCore.userInfo?.externalIds;
            return externalIds != null && externalIds.Length > 1 && externalIds.Any(x => x.providerId == loginMethodId);
        }

        private bool IsAlreadyLinkedToProvider(string providerId)
        {
            return m_PlayerIdentityCore.userInfo?.externalIds != null && m_PlayerIdentityCore.userInfo.externalIds.Any(p => providerId.Equals(p.providerId));
        }

        internal static bool IsUserConnectedViaEmail()
        {
            return PlayerIdentityManager.Current.userInfo?.externalIds?.Any(x => x.providerId == EmailLoginMethodId) ?? false;
        }

        internal static bool IsUserConnectedViaPhone()
        {
            return PlayerIdentityManager.Current.userInfo?.externalIds?.Any(x => x.providerId == PhoneLoginMethodId) ?? false;
        }

        private bool IsUserAnonymouslyLoggedIn()
        {
            return m_PlayerIdentityCore.userInfo?.isAnonymous ?? false;
        }

        public void OnVerifyEmailClicked()
        {
            MainController.Instance.ShowLoading(true);
            PlayerIdentityManager.Current.VerifyEmail(m_PlayerIdentityCore.userInfo.email, (playerIdentityCore) =>
            {
                MainController.Instance.ShowLoading(false);
                MainController.Instance.PopupController.ShowInfo("验证邮件已发送，请注意查看邮件");
            });
        }

        public void OnLogoutClicked()
        {
            m_PlayerIdentityCore.Logout();
        }

        public void OnLinkCompleted()
        {
            MainController.Instance.PopupController.ShowInfo("添加成功");
            MainController.Instance.OnLogin();
            UpdatePanel();
        }

        public void OnUnlinkCompleted()
        {
            MainController.Instance.PopupController.ShowInfo("删除关联成功");
            UpdatePanel();
        }

        /// <summary>
        /// From anonymous to email log in
        /// </summary>
        private void OnUpgradeAccountClicked()
        {
            PanelController panelController = MainController.Instance.PanelController;
            panelController.OpenPanel(panelController.GetComponentInChildren<UpgradeAccountPanel>());
        }

        /// <summary>
        /// Add phone to the user
        /// </summary>
        private void OnLinkPhoneClicked()
        {
            PanelController panelController = MainController.Instance.PanelController;
            PhoneSignInPanel.LoginAction = PhoneSignInPanel.LoginActions.Link;
            panelController.OpenPanel(panelController.GetComponentInChildren<PhoneSignInPanel>());
        }
    }
}
