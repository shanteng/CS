using UnityEngine.PlayerIdentity.Management;

namespace UnityEngine.PlayerIdentity.UnityUserAuth
{
    public class UnityUserAuthLoader 
        : PlayerIdentityLoaderHelper<PlayerIdentityBackendSubsystem, PlayerIdentityBackendSubsystemDescriptor>
    {
        /// <summary>
        /// Key we use to store and retrieve custom configuration settings from EditorBuildSettings
        /// </summary>
        public const string k_SettingsKey = "com.unity.playerid.unity_userauth_loader";

        /// <summary>
        /// The global unique ID of the subsystem.
        /// </summary>
        public const string k_SubsystemId = "Unity-UserAuth-Identity-Backend";
        
        public override bool Initialize()
        {
            return CreateSubsystem(k_SubsystemId);
        }
        
        public static UnityUserAuthLoaderSettings GetSettings()
        {
            UnityUserAuthLoaderSettings settings = null;
            // When running in the Unity Editor, we have to load user's customization of configuration data directly from
            // EditorBuildSettings. At runtime, we need to grab it from the static instance field instead.
#if UNITY_EDITOR
            UnityEditor.EditorBuildSettings.TryGetConfigObject(k_SettingsKey, out settings);
#else
            settings = UnityUserAuthLoaderSettings.s_RuntimeInstance;
#endif
            return settings;
        }

        public override void Register()
        {
            UnityUserAuthBackend.Register();
        }
    }
}
