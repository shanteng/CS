using System;
using TMPro;
using UnityEngine.UI;

namespace UnityEngine.PlayerIdentity.UI
{
    [Serializable]
    public class ActionBar
    {
        public RectTransform container;
        public RectTransform content;
        public Transform title;
        public Transform separationLine;
        public Button closeButton;
        public Button backButton;
    }

    [Serializable]
    public class Footer
    {
        public RectTransform rectTransform;
        public Button secondaryActionButton;
        public Transform separationLine;
    }

    /// <summary>
    /// Base class for all panels
    /// </summary>
    public abstract class AbstractPanel : MonoBehaviour
    {
        private Canvas canvas;

        public AbstractPanel ParentPanel;

        public ActionBar actionBar;
        public Footer secondaryActionFooter;

        void Awake()
        {
            canvas = gameObject.GetComponent<Canvas>();
        }

        public virtual void OpenPanel()
        {
            ShowPanel();
        }

        public void ClosePanel()
        {
            HidePanel();
        }

        public void Back()
        {
            HidePanel();
            ParentPanel?.ShowPanel();
        }

        protected virtual void ShowPanel()
        {
            canvas.enabled = true;
        }

        protected virtual void HidePanel()
        {
            // reset all input fields
            foreach (TMP_InputField inputField in GetComponentsInChildren<TMP_InputField>())
                inputField.text = null;
            canvas.enabled = false;
        }
    }
}
