namespace UnityEngine.PlayerIdentity.UI
{
    public class PanelController : MonoBehaviour
    {
        private AbstractPanel m_CurrentPanel;

        public void OpenPanel(AbstractPanel panel)
        {
            if (m_CurrentPanel == null || panel.name != m_CurrentPanel?.name)
            {
                panel.OpenPanel();

                m_CurrentPanel?.ClosePanel();
                m_CurrentPanel = panel;
            } 
        }

        public void OnBack()
        {
            m_CurrentPanel.Back();
            m_CurrentPanel = m_CurrentPanel.ParentPanel;
        }

        public void OnClose()
        {
            m_CurrentPanel?.ClosePanel();
            m_CurrentPanel = null;
        }
    }
}
