using TMPro;

namespace UnityEngine.PlayerIdentity.UI
{
    public class EmailSignInPanel : AbstractPanel
    {
        [SerializeField]
        TMP_InputField email = default;

        [SerializeField]
        TMP_InputField password = default;

        PrimaryActionButton m_LoginBtn;

        private void Start()
        {
            m_LoginBtn = GetComponentInChildren<PrimaryActionButton>();

            email.onValueChanged.AddListener(OnValueChange);
            password.onValueChanged.AddListener(OnValueChange);
        }

        private void OnValueChange(string text)
        {
            if(FormUtils.AreInputFieldsNotEmpty(email.text, password.text))
            {
                if (!m_LoginBtn.IsInteractable())
                    m_LoginBtn.interactable = true;
            }
            else
            {
                if (m_LoginBtn.IsInteractable())
                    m_LoginBtn.interactable = false;
            }
        }

        public void OnLoginClicked()
        {
            if (FormUtils.IsValidEmail(email.text))
            {
                MainController.Instance.ShowLoading(true);
                PlayerIdentityManager.Current.Login(email.text, password.text);
            }
            else
                MainController.Instance.PopupController.ShowError(new Error() { message = "Email Address in invalid format" });
        }
    }
}
