using TMPro;
using UnityEngine.Video;

namespace UnityEngine.PlayerIdentity.UI
{
    public class UpdateAccountPanel : AbstractPanel
    {
        [SerializeField]
        protected TMP_InputField password = default;

        [SerializeField]
        protected TMP_InputField newPassword = default;

        [SerializeField]
        protected TMP_InputField newPasswordConfirmation = default;

        [SerializeField]
        protected TMP_InputField displayName = default;

        [SerializeField]
        protected PrimaryActionButton saveUserButton;
        
        [SerializeField]
        protected PrimaryActionButton changePasswordButton;

        [SerializeField]
        protected GameObject changePasswordPanel;

        [SerializeField]
        protected GameObject changePasswordPanelSeparator;

        protected MainController mainController;
        protected IPlayerIdentityCore identityCore;

        private string originalDisplayName;

        private void Start()
        {
            mainController = MainController.Instance;

            identityCore = PlayerIdentityManager.Current;

            displayName.onValueChanged.AddListener(OnDisplayNameChanged);
            password.onValueChanged.AddListener(OnPasswordValueChange);
            newPassword.onValueChanged.AddListener(OnPasswordValueChange);
            newPasswordConfirmation.onValueChanged.AddListener(OnPasswordValueChange);
        }

        public override void OpenPanel()
        {
            base.OpenPanel();

            originalDisplayName = identityCore.displayName;
            displayName.text = originalDisplayName;
            password.text = null;
            newPassword.text = null;
            newPasswordConfirmation.text = null;

            bool isUserConnectedViaEmail = AccountPanel.IsUserConnectedViaEmail();
            changePasswordPanel.SetActive(isUserConnectedViaEmail);
            changePasswordPanelSeparator.SetActive(isUserConnectedViaEmail);
        }

        private void OnDisplayNameChanged(string text)
        {
            if (!string.IsNullOrWhiteSpace(text) && text != originalDisplayName)
            {
                if (!saveUserButton.IsInteractable())
                    saveUserButton.interactable = true;
            }
            else
            {
                if (saveUserButton.IsInteractable())
                    saveUserButton.interactable = false;
            }
        }
        
        private void OnPasswordValueChange(string text)
        {
            if (FormUtils.AreInputFieldsNotEmpty(password.text, newPassword.text, newPasswordConfirmation.text) &&
                newPassword.text == newPasswordConfirmation.text)
            {
                if (!changePasswordButton.IsInteractable())
                    changePasswordButton.interactable = true;
            }
            else
            {
                if (changePasswordButton.IsInteractable())
                    changePasswordButton.interactable = false;
            }
        }

        public void OnSaveUserClicked()
        {
            saveUserButton.interactable = false;
            UpdateDisplayName(displayName.text, result =>
            {
                if (result.error == null)
                {
                    originalDisplayName = result.displayName;
                    mainController.PopupController.ShowInfo("User is updated !");
                }

                OnDisplayNameChanged(displayName.text);
            });
        }

        public void OnChangePasswordClicked()
        {
            changePasswordButton.interactable = false;
            mainController.ShowLoading(true);
            identityCore.ChangePassword(password.text, newPassword.text, result =>
            {
                mainController.ShowLoading(false);

                if (result.error == null)
                {
                    password.text = null;
                    newPassword.text = null;
                    newPasswordConfirmation.text = null;
                    
                    mainController.PopupController.ShowInfo("Password is changed !");
                }
                
                OnPasswordValueChange(null);
            });
        }

        internal static void UpdateDisplayName(string displayName, PlayerIdentityCallback callback = null)
        {
            var mainController = MainController.Instance;
            var identityCore = PlayerIdentityManager.Current;
            
            mainController.ShowLoading(true);
            identityCore.UpdateUser(new UserInfo
            {
                userId = identityCore.userInfo.userId,
                email = identityCore.userInfo.email,
                // only update the display name here
                displayName = displayName,
                emailVerified = identityCore.userInfo.emailVerified,
                signInProviderId = identityCore.userInfo.signInProviderId,
                externalId = identityCore.userInfo.signInProviderId,
                isAnonymous = identityCore.userInfo.isAnonymous,
                photoUrl = identityCore.userInfo.photoUrl,
                disabled = identityCore.userInfo.disabled,
                externalIds = identityCore.userInfo.externalIds
            }, result =>
            {
                mainController.ShowLoading(false);
                callback?.Invoke(result);
            });
        }
    }
}