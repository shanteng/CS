namespace UnityEngine.PlayerIdentity.UI
{
    public class SignUpPanel : AbstractPasswordConfirmationPanel
    {
        public void OnSignUpClicked()
        {
            if (FormUtils.IsValidEmail(email.text))
            {
                if (FormUtils.IsValidConfirmationPassword(password.text, passwordConfirmation.text))
                {
                    m_MainController.ShowLoading(true);
                    PlayerIdentityManager.Current.Register(email.text, password.text, result =>
                    {
                        if (result.error == null && !string.IsNullOrWhiteSpace(displayName.text))
                        {
                            UpdateAccountPanel.UpdateDisplayName(displayName.text);
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