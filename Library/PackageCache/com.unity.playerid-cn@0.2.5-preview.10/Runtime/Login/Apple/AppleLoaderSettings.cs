using UnityEngine;
using UnityEngine.PlayerIdentity.Management;

namespace UnityEngine.PlayerIdentity.Apple
{
    /// <summary>
    /// Settings to control the AppleLoader behavior.
    /// </summary>
    [PlayerIdentityConfigurationData("Providers/Sign In with Apple", AppleLoader.k_SettingsKey)]
    [System.Serializable]
    public class AppleLoaderSettings : ScriptableObject
    {
        /// <summary>
        /// Static instance that will hold the runtime asset instance we created in our build process.
        /// </summary>
#if !UNITY_EDITOR
        internal static AppleLoaderSettings s_RuntimeInstance = null;
#endif

        [SerializeField, Tooltip("Sign In with Apple login callback endpoint for your app(defaults to Unity ID).")]
        public string m_CallbackEndpoint;

        [SerializeField, Tooltip("The deep link that redirects back to the application.")]
        public string m_ApplicationDeepLink;

        [SerializeField, Tooltip("The service ID that is used by Sign In With Apple on non-iOS devices.")]
        [InspectorName("Non-iOS Service ID")]
        public string m_ServiceId;

        public void Reset()
        {
#if UNITY_EDITOR
            // set default values
            m_ServiceId = Application.identifier + ".service";
            m_ApplicationDeepLink = Application.identifier.ToLowerInvariant() + "://siwacallback";
            m_CallbackEndpoint = "https://identity-api.unity.cn/oauth2/callback";
#endif
        }

        public void Awake()
        {
#if !UNITY_EDITOR
            s_RuntimeInstance = this;
#endif
        }
    }
}