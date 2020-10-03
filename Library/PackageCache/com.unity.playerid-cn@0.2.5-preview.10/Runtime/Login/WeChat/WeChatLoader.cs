using UnityEngine.PlayerIdentity.Management;

namespace UnityEngine.PlayerIdentity.WeChat
{
    public class WeChatLoader : PlayerIdentityLoaderHelper<PlayerIdentityLoginSubsystem, PlayerIdentityLoginSubsystemDescriptor>
    {
        /// <summary>
        /// Key we use to store and retrieve custom configuration settings from EditorBuildSettings
        /// </summary>
        public const string k_SettingsKey = "com.unity.playerid.wechat_loader";
        
        /// <summary>
        /// The global unique ID of the subsystem.
        /// </summary>
        public const string k_SubsystemId = "WeChat-Identity-Subsystem";

        public override bool Initialize()
        {
            return CreateSubsystem(k_SubsystemId);
        }

        public static WeChatLoaderSettings GetSettings()
        {
            WeChatLoaderSettings settings = null;
            // When running in the Unity Editor, we have to load user's customization of configuration data directly from
            // EditorBuildSettings. At runtime, we need to grab it from the static instance field instead.
#if UNITY_EDITOR
            UnityEditor.EditorBuildSettings.TryGetConfigObject(k_SettingsKey, out settings);
            WeChatLoaderSettings.s_RuntimeInstance = settings;
#else
            settings = WeChatLoaderSettings.s_RuntimeInstance;
#endif
            return settings;
        }
        
        public override void Register()
        {
            WeChatPlayerIdentitySubsystem.RegisterDescriptor();
        }
    }
}