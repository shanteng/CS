using UnityEngine.PlayerIdentity.Management;

namespace UnityEngine.PlayerIdentity.Apple
{
    public class AppleLoader
        : PlayerIdentityLoaderHelper<PlayerIdentityLoginSubsystem, PlayerIdentityLoginSubsystemDescriptor>
    {
        /// <summary>
        /// Key we use to store and retrieve custom configuration settings from EditorBuildSettings
        /// </summary>
        public const string k_SettingsKey = "com.unity.playerid.apple_loader";
        
        /// <summary>
        /// The global unique ID of the subsystem.
        /// </summary>
        public const string k_SubsystemId = "Apple-Identity-Subsystem";

        public override bool Initialize()
        {
            return CreateSubsystem(k_SubsystemId);
        }
    
        public static AppleLoaderSettings GetSettings()
        {
            AppleLoaderSettings settings = null;
            // When running in the Unity Editor, we have to load user's customization of configuration data directly from
            // EditorBuildSettings. At runtime, we need to grab it from the static instance field instead.
#if UNITY_EDITOR
            UnityEditor.EditorBuildSettings.TryGetConfigObject(AppleLoader.k_SettingsKey, out settings);
#else
            settings = AppleLoaderSettings.s_RuntimeInstance;
#endif
            return settings;
        }

        public override void Register()
        {
            SignInWithApplePlayerIdentitySubsystem.RegisterDescriptor();
        }
    }
}
