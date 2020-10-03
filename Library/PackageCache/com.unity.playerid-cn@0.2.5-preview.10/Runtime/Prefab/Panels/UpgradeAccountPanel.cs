namespace UnityEngine.PlayerIdentity.UI
{
    public class UpgradeAccountPanel : AbstractPasswordConfirmationPanel
    {
        public void OnUpgradeAccountClicked()
        {
            if (FormUtils.IsValidEmail(email.text))
            {
                if (FormUtils.IsValidConfirmationPassword(password.text, passwordConfirmation.text))
                {
                    m_MainController.ShowLoading(true);
                    PlayerIdentityManager.Current.CreateCredential(email.text, password.text, result =>
                    {
                        if (result.error == null)
                        {
                            if (!string.IsNullOrWhiteSpace(displayName.text))
                            {
                                UpdateAccountPanel.UpdateDisplayName(displayName.text,
                                    (_) =>
                                    {
                                        m_MainController.OnUpgradeAccountComplete();
                                    });
                            }
                            else
                            {
                                m_MainController.OnUpgradeAccountComplete();
                            }
                        }
                    });
                }
                else
                {
                    m_MainController.PopupController.ShowError(new Error() { message = "The passwords do not match" });
                }
            }
            else
            {
                m_MainController.PopupController.ShowError(new Error() { message = "Email address is in invalid format" });
            }
        }
    }
}