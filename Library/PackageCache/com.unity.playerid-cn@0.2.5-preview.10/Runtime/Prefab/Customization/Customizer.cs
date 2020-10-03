using System;
using TMPro;
using System.Reflection;

namespace UnityEngine.PlayerIdentity.UI.Customizer
{
    [ExecuteInEditMode]
    public class Customizer : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("General")]
        [ApplyButton("CustomizeAllText", "CustomizeTextFontAssetDelegate", typeof(TMP_FontAsset))]
        public TMP_FontAsset fontAsset;

        [Header("Panel")]
        [ApplyButton("CustomizePanelBgRectTransform", "CustomizeRectTransformSizeDelegate", typeof(Vector2))]
        public Vector2 size = new Vector2(714, 1258);
        [ApplyButton("CustomizeVerticalLayoutGroupSpacing", "CustomizeVerticalLayoutSpacingDelegate", typeof(float))] 
        public float verticalGroupSpacing = 16.0f;
        public ActionBar actionBar;
        public ProvidersButton providersButton;
        public PrimaryActionButton primaryActionButton;
        public InputField inputField;
        public SecondaryActionFooter footer;
        public PanelBackground background;

        [Header("Popup")]
        public PopupOptions general;
        public ErrorPopup error;
        public InfoPopup info;

        private CustomizerManager customizerManager;

        void OnEnable()
        {
            customizerManager = GetComponent<CustomizerManager>();
        }

        public void ApplyProperty<T>(string callbackName, string delegateName, T value)
        {
            object delegateValue = typeof(CustomizerManager).GetField(delegateName).GetValue(customizerManager);

            object[] parameters = new object[] { value, delegateValue };
            Type[] genericTypes = new Type[] { typeof(T) };

            MethodInfo method = typeof(CustomizerManager).GetMethod(callbackName);
            method = method.MakeGenericMethod(genericTypes);
            method.Invoke(customizerManager, parameters);
        }

#endif
    }

#if UNITY_EDITOR
    // Action Bar
    
    [Serializable]
    public class ActionBar
    {
        [ApplyButton("CustomizeActionBarHeight", "CustomizeRectTransformHeightDelegate", typeof(float))]
        public float height = 90.0f;
        public BackgroundImage backgroundImage;
        public PanelTitle text;
        public CloseButton closeButton;
        public BackButton backButton;
        public ActionBarSeparationLine separationLine;

        [Serializable]
        public class PanelTitle
        {
            [ApplyButton("CustomizeActionBarTitleText", "CustomizeTextFontAssetDelegate", typeof(TMP_FontAsset))]
            public TMP_FontAsset fontAsset;
            [ApplyButton("CustomizeActionBarTitleText", "CustomizeTextFontSizeDelegate", typeof(float))]
            public float fontSize = 35.0f;
            [ApplyButton("CustomizeActionBarTitleText", "CustomizeTextFontColorDelegate", typeof(Color))]
            public Color fontColor;
        }

        [Serializable]
        public class BackgroundImage
        {
            [ApplyButton("CustomizeActionBarBackgroundImage", "CustomizeImageSpriteDelegate", typeof(Sprite))]
            public Sprite sprite;
            [ApplyButton("CustomizeActionBarBackgroundImage", "CustomizeImageColorDelegate", typeof(Color))]
            public Color color;
        }

        [Serializable]
        public class CloseButton
        {
            [ApplyButton("CustomizeCloseButtonRectTransform", "CustomizeRectTransformSizeDelegate", typeof(Vector2))]
            public Vector2 size = new Vector2(89, 89);
            [ApplyButton("CustomizeCloseButtonContainerRectTransform", "CustomizeRectTransformPosition", typeof(Vector2))]
            public Vector2 positionOffsetFromCorner = new Vector2(-44.5f, -44.5f);
            public CloseButtonImage image;
            public CloseButtonHitbox hitbox;

            [Serializable]
            public class CloseButtonHitbox
            {
                [ApplyButton("CustomizeCloseButtonContainerRectTransform", "CustomizeRectTransformSizeDelegate", typeof(Vector2))]
                public Vector2 size = new Vector2(89, 89);
            }
            
            [Serializable]
            public class CloseButtonImage
            {
                [ApplyButton("CustomizeCloseBtnImage", "CustomizeImageSpriteDelegate", typeof(Sprite))]
                public Sprite sprite;
                [ApplyButton("CustomizeCloseBtnImage", "CustomizeImageColorDelegate", typeof(Color))]
                public Color color;
            }
        }

        [Serializable]
        public class BackButton
        {
            [ApplyButton("CustomizeBackButtonRectTransform", "CustomizeRectTransformSizeDelegate", typeof(Vector2))]
            public Vector2 size = new Vector2(89, 89);
            [ApplyButton("CustomizeBackButtonContainerRectTransform", "CustomizeRectTransformPosition", typeof(Vector2))]
            public Vector2 positionOffsetFromCorner = new Vector2(44.5f, -44.5f);
            public BackButtonImage image;
            public BackButtonHitbox hitbox;

            [Serializable]
            public class BackButtonHitbox
            {
                [ApplyButton("CustomizeBackButtonContainerRectTransform", "CustomizeRectTransformSizeDelegate", typeof(Vector2))]
                public Vector2 size = new Vector2(89, 89);
            }

            [Serializable]
            public class BackButtonImage
            {
                [ApplyButton("CustomizeBackBtnImage", "CustomizeImageSpriteDelegate", typeof(Sprite))]
                public Sprite sprite;
                [ApplyButton("CustomizeBackBtnImage", "CustomizeImageColorDelegate", typeof(Color))]
                public Color color;
            }
        }

        [Serializable]
        public class ActionBarSeparationLine
        {
            [ApplyButton("CustomizeActionBarSeparationLineHeight", "CustomizeRectTransformHeightDelegate", typeof(float))]
            public float height = 5.0f;
            public ActionBarSeparationLineImage image;

            [Serializable]
            public class ActionBarSeparationLineImage
            {
                [ApplyButton("CustomizeActionBarSeparationLineImage", "CustomizeImageSpriteDelegate", typeof(Sprite))]
                public Sprite sprite;
                [ApplyButton("CustomizeActionBarSeparationLineImage", "CustomizeImageColorDelegate", typeof(Color))]
                public Color color;
            }
        }
    }

    // Secondary Action Footer

    [Serializable]
    public class SecondaryActionFooter
    {
        public FooterSeparationLine separationLine;
        public SecondaryActionButton secondaryActionButton;

        [Serializable]
        public class FooterSeparationLine
        {
            [ApplyButton("CustomizeFooterSeparationLineHeight", "CustomizeRectTransformHeightDelegate", typeof(float))]
            public float height = 5.0f;
            public FooterSeparationLineImage image;

            [Serializable]
            public class FooterSeparationLineImage
            {
                [ApplyButton("CustomizeFooterSeparationLineImage", "CustomizeImageSpriteDelegate", typeof(Sprite))]
                public Sprite sprite;
                [ApplyButton("CustomizeFooterSeparationLineImage", "CustomizeImageColorDelegate", typeof(Color))]
                public Color color;
            }
        }

        [Serializable]
        public class SecondaryActionButton
        {
            [ApplyButton("CustomizeSecondaryActionBtnHeight", "CustomizeRectTransformHeightDelegate", typeof(float))]
            public float height = 90.0f;
            public SecondaryActionButtonImage image;
            public SecondaryActionButtonText text;

            [Serializable]
            public class SecondaryActionButtonImage
            {
                [ApplyButton("CustomizeSecondaryActionBtnImage", "CustomizeImageSpriteDelegate", typeof(Sprite))]
                public Sprite sprite;
                [ApplyButton("CustomizeSecondaryActionBtnImage", "CustomizeImageColorDelegate", typeof(Color))]
                public Color color;
            }

            [Serializable]
            public class SecondaryActionButtonText
            {
                [ApplyButton("CustomizeSecondaryActionBtnText", "CustomizeTextFontAssetDelegate", typeof(TMP_FontAsset))]
                public TMP_FontAsset fontAsset;
                [ApplyButton("CustomizeSecondaryActionBtnText", "CustomizeTextFontSizeDelegate", typeof(float))]
                public float fontSize = 35.0f;
                [ApplyButton("CustomizeSecondaryActionBtnText", "CustomizeTextFontColorDelegate", typeof(Color))]
                public Color fontColor;
            }
        }
    }

    // Primary Action Button

    [Serializable]
    public class PrimaryActionButton
    {
        [ApplyButton("CustomizePrimaryActionBtnRectTransform", "CustomizeRectTransformSizeDelegate", typeof(Vector2))]
        public Vector2 size = new Vector2(637, 119);
        public PrimaryActionButtonImage image;
        public PrimaryActionButtonText text;

        [Serializable]
        public class PrimaryActionButtonImage
        {
            [ApplyButton("CustomizePrimaryActionBtnImage", "CustomizeImageSpriteDelegate", typeof(Sprite))]
            public Sprite sprite;
            [ApplyButton("CustomizePrimaryActionBtnImageColor", "CustomizeButtonActiveColorDelegate", typeof(Color))]
            public Color activeColor;
            [ApplyButton("CustomizePrimaryActionBtnImageColor", "CustomizeButtonInactiveColorDelegate", typeof(Color))]
            public Color inactiveColor;
        }

        [Serializable]
        public class PrimaryActionButtonText
        {
            [ApplyButton("CustomizePrimaryActionBtnText", "CustomizeTextFontAssetDelegate", typeof(TMP_FontAsset))]
            public TMP_FontAsset fontAsset;
            [ApplyButton("CustomizePrimaryActionBtnText", "CustomizeTextFontSizeDelegate", typeof(float))]
            public float fontSize = 35.0f;
            [ApplyButton("CustomizePrimaryActionBtnScript", "CustomizePrimaryActionButtonActiveColorDelegate", typeof(Color))]
            public Color activeFontColor;
            [ApplyButton("CustomizePrimaryActionBtnScript", "CustomizePrimaryActionButtonInactiveColorDelegate", typeof(Color))]
            public Color inactiveFontColor;
        }
    }

    // Input Field

    [Serializable]
    public class InputField
    {
        [ApplyButton("CustomizeInputFieldRectTransform", "CustomizeRectTransformSizeDelegate", typeof(Vector2))]
        public Vector2 size = new Vector2(614, 95);
        public InputFieldImage image;
        public InputFieldText text;
        public InputFieldPlaceHolder placeHolder;

        [Serializable]
        public class InputFieldImage
        {
            [ApplyButton("CustomizeInputFieldImage", "CustomizeImageSpriteDelegate", typeof(Sprite))]
            public Sprite sprite;
            [ApplyButton("CustomizeInputFieldImage", "CustomizeImageColorDelegate", typeof(Color))]
            public Color color;
        }

        [Serializable]
        public class InputFieldText
        {
            [ApplyButton("CustomizeInputFieldText", "CustomizeTextFontAssetDelegate", typeof(TMP_FontAsset))]
            public TMP_FontAsset fontAsset;
            [ApplyButton("CustomizeInputFieldText", "CustomizeTextFontSizeDelegate", typeof(float))]
            public float fontSize = 30.0f;
            [ApplyButton("CustomizeInputFieldText", "CustomizeTextFontColorDelegate", typeof(Color))]
            public Color fontColor;
        }

        [Serializable]
        public class InputFieldPlaceHolder
        {
            [ApplyButton("CustomizeInputFieldPlaceHolderText", "CustomizeTextFontAssetDelegate", typeof(TMP_FontAsset))]
            public TMP_FontAsset fontAsset;
            [ApplyButton("CustomizeInputFieldPlaceHolderText", "CustomizeTextFontSizeDelegate", typeof(float))]
            public float fontSize = 30.0f;
            [ApplyButton("CustomizeInputFieldPlaceHolderText", "CustomizeTextFontColorDelegate", typeof(Color))]
            public Color fontColor;
        }
    }

    // Panel Background

    [Serializable]
    public class PanelBackground
    {
        [ApplyButton("CustomizePanelBgImage", "CustomizeImageSpriteDelegate", typeof(Sprite))]
        public Sprite sprite;
        [ApplyButton("CustomizePanelBgImage", "CustomizeImageColorDelegate", typeof(Color))]
        public Color color;
    }

    // Popup

    [Serializable]
    public class PopupOptions
    {
        [ApplyButton("CustomizePopupOptions", "CustomizePopupFadeTimeDelegate", typeof(float))]
        public float fadeTime = 0.3f;
        [ApplyButton("CustomizePopupOptions", "CustomizePopupTimeBeforeHideDelegate", typeof(float))]
        public float timeBeforeHide = 3.0f;
        [ApplyButton("CustomizePopupOptions", "CustomizePopupVerticalTextMarginDelegate", typeof(float))]
        public float verticalTextMargin = 15.0f;
    }

    [Serializable]
    public class InfoPopup
    {
        public InfoPopupImage popupBackground;
        public InfoText text;

        [Serializable]
        public class InfoPopupImage
        {
            [ApplyButton("CustomizeInfoPopupImage", "CustomizeImageSpriteDelegate", typeof(Sprite))]
            public Sprite sprite;
            [ApplyButton("CustomizeInfoPopupImage", "CustomizeImageColorDelegate", typeof(Color))]
            public Color color;
        }

        [Serializable]
        public class InfoText
        {
            [ApplyButton("CustomizeInfoText", "CustomizeTextFontAssetDelegate", typeof(TMP_FontAsset))]
            public TMP_FontAsset fontAsset;
            [ApplyButton("CustomizeInfoText", "CustomizeTextFontSizeDelegate", typeof(float))]
            public float fontSize = 35.0f;
            [ApplyButton("CustomizeInfoText", "CustomizeTextFontColorDelegate", typeof(Color))]
            public Color fontColor;
        }
    }

    [Serializable]
    public class ErrorPopup
    {
        public ErrorPopupImage popupBackground;
        public ErrorText text;

        [Serializable]
        public class ErrorPopupImage
        {
            [ApplyButton("CustomizeErrorPopupImage", "CustomizeImageSpriteDelegate", typeof(Sprite))]
            public Sprite sprite;
            [ApplyButton("CustomizeErrorPopupImage", "CustomizeImageColorDelegate", typeof(Color))]
            public Color color;
        }

        [Serializable]
        public class ErrorText
        {
            [ApplyButton("CustomizeErrorText", "CustomizeTextFontAssetDelegate", typeof(TMP_FontAsset))]
            public TMP_FontAsset fontAsset;
            [ApplyButton("CustomizeErrorText", "CustomizeTextFontSizeDelegate", typeof(float))]
            public float fontSize = 35.0f;
            [ApplyButton("CustomizeErrorText", "CustomizeTextFontColorDelegate", typeof(Color))]
            public Color fontColor;
        }
    }

    // Providers Button

    [Serializable]
    public class ProvidersButton
    {
        [ApplyButton("CustomizeProvidersLoginBtnRectTransform", "CustomizeRectTransformSizeDelegate", typeof(Vector2))]
        public Vector2 size = new Vector2(637, 119);
    }
#endif
}