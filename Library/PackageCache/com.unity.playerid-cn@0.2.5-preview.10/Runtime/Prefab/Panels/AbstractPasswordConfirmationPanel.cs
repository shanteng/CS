using TMPro;

namespace UnityEngine.PlayerIdentity.UI
{
    public abstract class AbstractPasswordConfirmationPanel : AbstractPanel
    {
        [SerializeField]
        protected TMP_InputField email = default;

        [SerializeField]
        protected TMP_InputField password = default;

        [SerializeField]
        protected TMP_InputField passwordConfirmation = default;

        [SerializeField]
        protected TMP_InputField displayName = default;

        protected PrimaryActionButton m_Btn;

        protected MainController m_MainController;

        private void Start()
        {
            m_MainController = MainController.Instance;
            m_Btn = GetComponentInChildren<PrimaryActionButton>();

            email.text = null;
            password.text = null;
            passwordConfirmation.text = null;

            email.onValueChanged.AddListener(OnValueChange);
            password.onValueChanged.AddListener(OnValueChange);
            passwordConfirmation.onValueChanged.AddListener(OnValueChange);
        }

        private void OnValueChange(string text)
        {
            if (FormUtils.AreInputFieldsNotEmpty(email.text, password.text, passwordConfirmation.text))
            {
                if (!m_Btn.IsInteractable())
                    m_Btn.interactable = true;
            }
            else
            {
                if (m_Btn.IsInteractable())
                    m_Btn.interactable = false;
            }
        }
    }
}
