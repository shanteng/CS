using TMPro;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
#endif
using UnityEngine.UI;

namespace UnityEngine.PlayerIdentity.UI.Customizer
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Customizer))]
    public class CustomizerManager : MonoBehaviour
    {
#if UNITY_EDITOR
        MainController m_MainController;

        public delegate void CustomizerDelegate<T, U>(T component, U customizableValue);

        public readonly CustomizerDelegate<Image, Color> CustomizeImageColorDelegate = (imageComponent, color) => { imageComponent.color = color; };
        public readonly CustomizerDelegate<Image, Sprite> CustomizeImageSpriteDelegate = (imageComponent, sprite) => { imageComponent.sprite = sprite; };

        public readonly CustomizerDelegate<TextMeshProUGUI, TMP_FontAsset> CustomizeTextFontAssetDelegate = (textComponent, font) => { textComponent.font = font; };
        public readonly CustomizerDelegate<TextMeshProUGUI, Color> CustomizeTextFontColorDelegate = (textComponent, color) => { textComponent.color = color; };
        public readonly CustomizerDelegate<TextMeshProUGUI, float> CustomizeTextFontSizeDelegate = (textComponent, size) => { textComponent.fontSize = size; };

        public readonly CustomizerDelegate<RectTransform, Vector2> CustomizeRectTransformSizeDelegate = (rectTransform, size) => { rectTransform.sizeDelta = size; };
        public readonly CustomizerDelegate<RectTransform, Vector2> CustomizeRectTransformPosition = (rectTransform, position) => { rectTransform.anchoredPosition = position; };
        public readonly CustomizerDelegate<RectTransform, float> CustomizeRectTransformHeightDelegate = (rectTransform, height) => { rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height); };

        public readonly CustomizerDelegate<VerticalLayoutGroup, float> CustomizeVerticalLayoutSpacingDelegate = (verticalLayoutGroup, newSpacing) => { verticalLayoutGroup.spacing = newSpacing; };

        public readonly CustomizerDelegate<Button, Color> CustomizeButtonActiveColorDelegate = (btn, color) => {
            ColorBlock cb = btn.colors;
            cb.normalColor = cb.selectedColor = color;
            btn.colors = cb;
        };

        public readonly CustomizerDelegate<Button, Color> CustomizeButtonInactiveColorDelegate = (btn, color) => {
            ColorBlock cb = btn.colors;
            cb.disabledColor = color;
            btn.colors = cb;
        };

        public readonly CustomizerDelegate<UI.PrimaryActionButton, Color> CustomizePrimaryActionButtonActiveColorDelegate = (btn, color) => { btn.activeButtonTextColor = color; };
        public readonly CustomizerDelegate<UI.PrimaryActionButton, Color> CustomizePrimaryActionButtonInactiveColorDelegate = (btn, color) => { btn.inactiveButtonTextColor = color; };

        public readonly CustomizerDelegate<PopupController, float> CustomizePopupFadeTimeDelegate = (popupController, value) => { popupController.fadeTime = value; };
        public readonly CustomizerDelegate<PopupController, float> CustomizePopupTimeBeforeHideDelegate = (popupController, value) => { popupController.timeBeforeHide = value; };
        public readonly CustomizerDelegate<PopupController, float> CustomizePopupVerticalTextMarginDelegate = (popupController, value) => { popupController.verticalTextMargin = value; };

        AbstractPanel[] m_Panels;

        private void OnEnable()
        {
            m_MainController = GetComponent<MainController>();
            m_Panels = m_MainController.PanelController.GetComponentsInChildren<AbstractPanel>();
        }

        // General Font

        public void CustomizeAllText<T>(T value, CustomizerDelegate<TextMeshProUGUI, T> customizerDelegate)
        {
            foreach (TextMeshProUGUI textComponent in m_MainController.GetComponentsInChildren<TextMeshProUGUI>())
                CustomizeComponent(textComponent, value, customizerDelegate);
        }

        // Panel Background

        public void CustomizePanelBgImage<T>(T value, CustomizerDelegate<Image, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
                CustomizeComponent(panel.GetComponent<Image>(), value, customizerDelegate);
        }

        public void CustomizePanelBgRectTransform<T>(T value, CustomizerDelegate<RectTransform, T> customizerDelegate)
        {
            CustomizeComponent(m_MainController.PanelController.transform as RectTransform, value, customizerDelegate);
        }

        // Action Bar

        public void CustomizeActionBarHeight<T>(T value, CustomizerDelegate<RectTransform, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
            {
                RectTransform content = panel.actionBar?.content;
                if (content != null)
                {
                    CustomizeComponent(content, value, customizerDelegate);
                    UpdateContainerHeight(value, panel.actionBar?.container, panel.actionBar?.separationLine as RectTransform);
                }
            }
        }

        public void CustomizeActionBarTitleText<T>(T value, CustomizerDelegate<TextMeshProUGUI, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
                CustomizeComponent(panel.actionBar?.title?.GetComponent<TextMeshProUGUI>(), value, customizerDelegate);
        }

        public void CustomizeActionBarBackgroundImage<T>(T value, CustomizerDelegate<Image, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
                CustomizeComponent(panel.actionBar?.content?.GetComponent<Image>(), value, customizerDelegate);
        }

        public void CustomizeCloseBtnImage<T>(T value, CustomizerDelegate<Image, T> customizerDelegate)
        {
            // we want the close button image itself and not the image used for managing the hitbox as a raycast target
            foreach (AbstractPanel panel in m_Panels)
                CustomizeComponent(panel.actionBar.closeButton.GetComponentsInChildren<Image>()[1], value, customizerDelegate);
        }

        public void CustomizeCloseButtonContainerRectTransform<T>(T value, CustomizerDelegate<RectTransform, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
                CustomizeComponent(panel.actionBar.closeButton.GetComponent<RectTransform>(), value, customizerDelegate);
        }

        public void CustomizeCloseButtonRectTransform<T>(T value, CustomizerDelegate<RectTransform, T> customizerDelegate)
        {
            // we want the close button rectTransform and not the rectTransform used for managing the hitbox as a raycast target
            foreach (AbstractPanel panel in m_Panels)
                CustomizeComponent(panel.actionBar.closeButton.GetComponentsInChildren<RectTransform>()[1], value, customizerDelegate);
        }

        public void CustomizeBackBtnImage<T>(T value, CustomizerDelegate<Image, T> customizerDelegate)
        {
            // we want the back button image itself and not the image used for managing the hitbox as a raycast target
            foreach (AbstractPanel panel in m_Panels)
                CustomizeComponent(panel.actionBar.backButton?.GetComponentsInChildren<Image>()[1], value, customizerDelegate);
        }

        public void CustomizeBackButtonContainerRectTransform<T>(T value, CustomizerDelegate<RectTransform, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
                CustomizeComponent(panel.actionBar.backButton?.GetComponent<RectTransform>(), value, customizerDelegate);
        }

        public void CustomizeBackButtonRectTransform<T>(T value, CustomizerDelegate<RectTransform, T> customizerDelegate)
        {
            // we want the close button rectTransform and not the rectTransform used for managing the hitbox as a raycast target
            foreach (AbstractPanel panel in m_Panels)
                CustomizeComponent(panel.actionBar.backButton?.GetComponentsInChildren<RectTransform>()[1], value, customizerDelegate);
        }

        public void CustomizeActionBarSeparationLineImage<T>(T value, CustomizerDelegate<Image, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
                CustomizeComponent(panel.actionBar.separationLine.GetComponent<Image>(), value, customizerDelegate);
        }
        
        public void CustomizeActionBarSeparationLineHeight<T>(T value, CustomizerDelegate<RectTransform, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
            {
                RectTransform content = panel.actionBar.separationLine as RectTransform;
                if (content != null)
                {
                    CustomizeComponent(content, value, customizerDelegate);
                    UpdateContainerHeight(value, panel.actionBar.container, panel.actionBar.content);
                }
            }
        }

        // Input Field

        public void CustomizeInputFieldImage<T>(T value, CustomizerDelegate<Image, T> customizerDelegate)
        {
            foreach (TMP_InputField inputField in m_MainController.GetComponentsInChildren<TMP_InputField>())
                CustomizeComponent(inputField.GetComponent<Image>(), value, customizerDelegate);
        }

        public void CustomizeInputFieldText<T>(T value, CustomizerDelegate<TextMeshProUGUI, T> customizerDelegate)
        {
            foreach (TMP_InputField inputField in m_MainController.GetComponentsInChildren<TMP_InputField>())
                CustomizeComponent(inputField.textComponent as TextMeshProUGUI, value, customizerDelegate);
        }

        public void CustomizeInputFieldPlaceHolderText<T>(T value, CustomizerDelegate<TextMeshProUGUI, T> customizerDelegate)
        {
            foreach (TMP_InputField inputField in m_MainController.GetComponentsInChildren<TMP_InputField>())
                CustomizeComponent(inputField.placeholder as TextMeshProUGUI, value, customizerDelegate);
        }

        public void CustomizeInputFieldRectTransform<T>(T value, CustomizerDelegate<RectTransform, T> customizerDelegate)
        {
            foreach (TMP_InputField inputField in m_MainController.GetComponentsInChildren<TMP_InputField>())
                CustomizeComponent(inputField.GetComponent<RectTransform>(), value, customizerDelegate);
        }

        // Primary Action Button

        public void CustomizePrimaryActionBtnImage<T>(T value, CustomizerDelegate<Image, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
                foreach (Button btn in panel.GetComponentsInChildren<UI.PrimaryActionButton>())
                    CustomizeComponent(btn.GetComponent<Image>(), value, customizerDelegate);
        }

        public void CustomizePrimaryActionBtnText<T>(T value, CustomizerDelegate<TextMeshProUGUI, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
                foreach (Button btn in panel.GetComponentsInChildren<UI.PrimaryActionButton>())
                    CustomizeComponent(btn.GetComponentInChildren<TextMeshProUGUI>(), value, customizerDelegate);
        }
        
        public void CustomizePrimaryActionBtnScript<T>(T value, CustomizerDelegate<UI.PrimaryActionButton, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
                foreach (Button btn in panel.GetComponentsInChildren<UI.PrimaryActionButton>())
                    CustomizeComponent(btn.GetComponent<UI.PrimaryActionButton>(), value, customizerDelegate);
        }

        public void CustomizePrimaryActionBtnImageColor<T>(T value, CustomizerDelegate<Button, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
                foreach (Button btn in panel.GetComponentsInChildren<UI.PrimaryActionButton>())
                    CustomizeComponent(btn, value, customizerDelegate);
        }

        public void CustomizePrimaryActionBtnRectTransform<T>(T value, CustomizerDelegate<RectTransform, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
                foreach (Button btn in panel.GetComponentsInChildren<UI.PrimaryActionButton>())
                    CustomizeComponent(btn.GetComponent<RectTransform>(), value, customizerDelegate);
        }

        // Secondary Action Footer

        public void CustomizeFooterSeparationLineImage<T>(T value, CustomizerDelegate<Image, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
                if(panel.secondaryActionFooter?.separationLine != null)
                    CustomizeComponent(panel.secondaryActionFooter?.separationLine.GetComponent<Image>(), value, customizerDelegate);
        }

        public void CustomizeFooterSeparationLineHeight<T>(T value, CustomizerDelegate<RectTransform, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
            {
                Transform line = panel.secondaryActionFooter?.separationLine;
                if (line != null)
                {
                    CustomizeComponent(line.GetComponent<RectTransform>(), value, customizerDelegate);
                    UpdateContainerHeight(value, panel.secondaryActionFooter.rectTransform, panel.secondaryActionFooter.secondaryActionButton.GetComponent<RectTransform>());
                }
            }
        }

        public void CustomizeSecondaryActionBtnImage<T>(T value, CustomizerDelegate<Image, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
                CustomizeComponent(panel.secondaryActionFooter?.secondaryActionButton?.GetComponent<Image>(), value, customizerDelegate);
        }

        public void CustomizeSecondaryActionBtnText<T>(T value, CustomizerDelegate<TextMeshProUGUI, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
                CustomizeComponent(panel.secondaryActionFooter?.secondaryActionButton?.GetComponentInChildren<TextMeshProUGUI>(), value, customizerDelegate);
        }

        public void CustomizeSecondaryActionBtnHeight<T>(T value, CustomizerDelegate<RectTransform, T> customizerDelegate)
        {
            foreach (AbstractPanel panel in m_Panels)
            {
                Button btn = panel.secondaryActionFooter?.secondaryActionButton;
                if (btn != null)
                {
                    CustomizeComponent(btn.GetComponent<RectTransform>(), value, customizerDelegate);
                    UpdateContainerHeight(value, panel.secondaryActionFooter?.rectTransform, panel.secondaryActionFooter?.separationLine as RectTransform);
                }
            }
        }

        // Popup

        public void CustomizePopupOptions<T>(T value, CustomizerDelegate<PopupController, T> customizerDelegate)
        {
            CustomizeComponent(m_MainController.PopupController, value, customizerDelegate);
        }

        public void CustomizeErrorPopupImage<T>(T value, CustomizerDelegate<Image, T> customizerDelegate)
        {
            CustomizeComponent(m_MainController.PopupController.errorPopup.GetComponent<Image>(), value, customizerDelegate);
        }

        public void CustomizeErrorText<T>(T value, CustomizerDelegate<TextMeshProUGUI, T> customizerDelegate)
        {
            CustomizeComponent(m_MainController.PopupController.errorPopup.GetComponentInChildren<TextMeshProUGUI>(), value, customizerDelegate);
        }

        public void CustomizeInfoPopupImage<T>(T value, CustomizerDelegate<Image, T> customizerDelegate)
        {
            CustomizeComponent(m_MainController.PopupController.infoPopup.GetComponent<Image>(), value, customizerDelegate);
        }

        public void CustomizeInfoText<T>(T value, CustomizerDelegate<TextMeshProUGUI, T> customizerDelegate)
        {
            CustomizeComponent(m_MainController.PopupController.infoPopup.GetComponentInChildren<TextMeshProUGUI>(), value, customizerDelegate);
        }

        // Providers Login Btn

        public void CustomizeProvidersLoginBtnRectTransform<T>(T value, CustomizerDelegate<RectTransform, T> customizerDelegate)
        {
            SignInPanel signinPanel = m_MainController.GetComponentInChildren<SignInPanel>();
            AccountPanel accountPanel = m_MainController.GetComponentInChildren<AccountPanel>();

            // buttons in signin panel (email and anonymous sign in)
            foreach (GameObject obj in signinPanel.genericProviderButtonsList)
                CustomizeComponent(obj.GetComponent<RectTransform>(), value, customizerDelegate);

            // providers button
            foreach (var provider in PlayerIdentityManager.Current.loginProviders)
                CustomizeComponent(provider.GetButton().GetComponent<RectTransform>(), value, customizerDelegate);
            
            // we only customize all the prefabs and not the instantiated providers button
            // we dont need this call only on playmode since the providers button are only instantiated during runtime
            if (EditorApplication.isPlaying)
            {
                signinPanel.ReBuildProvidersList();
                accountPanel.UpdatePanel();
            }
        }

        // Vertical Layout

        public void CustomizeVerticalLayoutGroupSpacing<T>(T value, CustomizerDelegate<VerticalLayoutGroup, T> customizerDelegate)
        {
            foreach (VerticalLayoutGroup verticalLayoutGroup in m_MainController.GetComponentsInChildren<VerticalLayoutGroup>())
                CustomizeComponent(verticalLayoutGroup, value, customizerDelegate);
        }

        private void UpdateContainerHeight<T>(T value, RectTransform container, RectTransform content)
        {
            if (container != null && content != null)
                CustomizeComponent(container, (float)(object)value + content.sizeDelta[1], CustomizeRectTransformHeightDelegate);
        }

        private void CustomizeComponent<T, U>(T component, U customizableValue, CustomizerDelegate<T, U> customizerDelegate)
        {
            if (component == null)
                return;

            Object componentObject = component as Object;
            Undo.RecordObject(componentObject, "Customize Component");

            customizerDelegate(component, customizableValue);

            PrefabUtility.RecordPrefabInstancePropertyModifications(componentObject);

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }
#endif
    }
}
