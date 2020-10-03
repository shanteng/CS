using System;
using System.IO;
using UnityEngine;
using UnityEngine.PlayerIdentity.Management;
using UnityEngine.Serialization;

namespace UnityEngine.PlayerIdentity.WeChat
{
    /// <summary>
    /// Settings to control the WeChatLoader behavior.
    /// </summary>
    [PlayerIdentityConfigurationData("Providers/WeChat", WeChatLoader.k_SettingsKey)]
    [System.Serializable]
    public class WeChatLoaderSettings : ScriptableObject
    {
        /// <summary>
        /// Static instance that will hold the runtime asset instance we created in our build process.
        /// </summary>
        public static WeChatLoaderSettings s_RuntimeInstance = null;

        [SerializeField, Tooltip("WeChat login callback endpoint for your app(defaults to Unity ID).")]
        public string m_CallbackEndpoint;
        
        [SerializeField, Tooltip("WeChat App Id")]
        public string m_AppID;

        [SerializeField, Tooltip("Use default WeChat Login CallBack Code.")]
        public bool m_GenerateCallbackCode = true;

        public void Awake()
        {
#if (UNITY_ANDROID || UNITY_IOS)
            s_RuntimeInstance = this;
#endif
        }
    }
}