using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.PlayerIdentity.Management
{
    /// <summary>
    /// The general settings of the player identity.
    /// </summary>
    public class PlayerIdentityGeneralSettings : ScriptableObject
    {
        public static string k_SettingsKey = "com.unity.playerid.management.loader_settings";
        internal static PlayerIdentityGeneralSettings s_RuntimeSettingsInstance = null;

        [SerializeField]
        internal PlayerIdentityManagerSettings m_LoaderManagerInstance = null;

        [SerializeField]
        internal bool m_InitManagerOnStart = true;

        public PlayerIdentityManagerSettings Manager
        {
            get { return m_LoaderManagerInstance; }
            set { m_LoaderManagerInstance = value; }
        }

        private PlayerIdentityManagerSettings m_PlayerIdentityManager = null;

        public static PlayerIdentityGeneralSettings Instance
        {
            get
            {
                return s_RuntimeSettingsInstance;
            }
        }
        
        public bool InitManagerOnStart
        {
            get
            {
                return m_InitManagerOnStart;
            }
            #if UNITY_EDITOR
            set
            {
                m_InitManagerOnStart = value;
            }
            #endif
        }

#if !UNITY_EDITOR
        void Awake()
        {
            Utils.Logger.Info("PlayerIdentityGeneral Settings awakening...");
            s_RuntimeSettingsInstance = this;
            Application.quitting += Quit;
            DontDestroyOnLoad(s_RuntimeSettingsInstance);
        }
#endif

#if UNITY_EDITOR
        bool m_IsPlaying = false;

        void EnterPlayMode()
        {
            if (!m_IsPlaying)
            {
                s_RuntimeSettingsInstance = this;

                InitPlayerIdentitySDK();
                StartPlayerIdentitySDK();
                m_IsPlaying = true;
            }
        }

        void ExitPlayMode()
        {
            if (m_IsPlaying)
            {
                m_IsPlaying = false;
                StopPlayerIdentitySDK();
                DeInitPlayerIdentitySDK();
            
                s_RuntimeSettingsInstance = null;
            }
        }

        public void InternalPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    EnterPlayMode();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    ExitPlayMode();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    break;
            }
        }
#else

        static void Quit()
        {
            PlayerIdentityGeneralSettings instance = PlayerIdentityGeneralSettings.Instance;
            if (instance == null)
                return;

            instance.OnDisable();
            instance.OnDestroy();                
        }

        void Start()
        {
            StartPlayerIdentitySDK();
        }

        void OnDisable()
        {
            StopPlayerIdentitySDK();
        }

        void OnDestroy()
        {
            DeInitPlayerIdentitySDK();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void AttemptInitializePlayerIdentitySDKOnLoad()
        {
#if !UNITY_EDITOR
            PlayerIdentityGeneralSettings instance = PlayerIdentityGeneralSettings.Instance;
            if (instance == null || !instance.InitManagerOnStart)
                return;

            instance.InitPlayerIdentitySDK();
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        internal static void AttemptStartPlayerIdentitySDKOnBeforeSplashScreen()
        {
#if !UNITY_EDITOR
            PlayerIdentityGeneralSettings instance = PlayerIdentityGeneralSettings.Instance;
            if (instance == null || !instance.InitManagerOnStart)
                return;

            instance.StartPlayerIdentitySDK();
#endif
        }

        private void InitPlayerIdentitySDK()
        {
            if (PlayerIdentityGeneralSettings.Instance == null || PlayerIdentityGeneralSettings.Instance.m_LoaderManagerInstance == null || PlayerIdentityGeneralSettings.Instance.m_InitManagerOnStart == false)
                return;

            m_PlayerIdentityManager = PlayerIdentityGeneralSettings.Instance.m_LoaderManagerInstance;
            if (m_PlayerIdentityManager == null)
            {
                Utils.Logger.Error("Assigned GameObject for PlayerIdentity Management loading is invalid. PlayerIdentity SDK will not be automatically loaded.");
                return;
            }

            m_PlayerIdentityManager.automaticLoading = false;
            m_PlayerIdentityManager.automaticRunning = false;
            m_PlayerIdentityManager.InitializeLoader();
        }

        private void StartPlayerIdentitySDK()
        {
            if (m_PlayerIdentityManager != null && m_PlayerIdentityManager.isInitializationComplete)
            {
                m_PlayerIdentityManager.StartSubsystems();
            }
        }

        private void StopPlayerIdentitySDK()
        {
            if (m_PlayerIdentityManager != null && m_PlayerIdentityManager.isInitializationComplete)
            {
                m_PlayerIdentityManager.StopSubsystems();
            }
        }

        private void DeInitPlayerIdentitySDK()
        {
            if (m_PlayerIdentityManager != null && m_PlayerIdentityManager.isInitializationComplete)
            {
                m_PlayerIdentityManager.DeinitializeLoader();
                m_PlayerIdentityManager = null;
            }
        }

    }
}
