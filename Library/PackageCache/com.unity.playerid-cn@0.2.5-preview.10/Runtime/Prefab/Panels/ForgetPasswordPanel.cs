namespace UnityEngine.PlayerIdentity.UI
{
    public class ForgetPasswordPanel : AbstractEmailFieldPanel
    {
        public override void OpenPanel()
        {
            base.OpenPanel();
            btn.interactable = true;
        }

        public void OnSendClicked()
        {
            if (FormUtils.IsValidEmail(email.text))
            {
                MainController.Instance.ShowLoading(true);
                PlayerIdentityManager.Current.ResetPassword(email.text, sender =>
                {
                    if (sender.error == null)
                    {
                        OnOkResponseReceived();
                    }
                });
                btn.interactable = false;
            }
            else
            {
                MainController.Instance.PopupController.ShowError(new Error() { message = "Email address is in invalid format" });
            }
        }

        public void OnOkResponseReceived()
        {
            MainController.Instance.ShowLoading(false);
            MainController.Instance.PopupController.ShowInfo("If an account is registered with this email, then an email has been sent to reset password. Please check your inbox.");
            MainController.Instance.PanelController.OnBack();
        }
    }
}