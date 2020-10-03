using System;
using TMPro;

namespace UnityEngine.PlayerIdentity.UI
{
    public abstract class AbstractEmailFieldPanel : AbstractPanel
    {
        [SerializeField]
        protected TMP_InputField email = default;

        protected PrimaryActionButton btn;

        private void Start()
        {
            btn = GetComponentInChildren<PrimaryActionButton>();
            email.onValueChanged.AddListener(OnValueChange);
        }

        public override void OpenPanel()
        {
            base.OpenPanel();
            email.text = PlayerIdentityManager.Current?.userInfo?.email;
            email.Select();
        }

        private void OnValueChange(string text)
        {
            if (FormUtils.AreInputFieldsNotEmpty(email.text))
            {
                if (!btn.IsInteractable())
                    btn.interactable = true;
            }
            else
            {
                if (btn.IsInteractable())
                    btn.interactable = false;
            }
        }
    }
}
