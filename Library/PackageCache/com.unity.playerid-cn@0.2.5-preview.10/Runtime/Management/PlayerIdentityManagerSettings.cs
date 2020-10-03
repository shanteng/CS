using System.Collections.Generic;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif
using UnityEngine.Serialization;
using UnityEngine.PlayerIdentity.Management;

namespace UnityEngine.PlayerIdentity.Management
{
    /// <summary>
    /// Class to handle active loader and subsystem management for PlayerIdentity SDK. This class is to be added as a
    /// component on a GameObject in your scene. Given a list of loaders, it will attempt to load all loaders in
    /// the given order.
    /// 
    /// Depending on configuration the PlayerIdentityManager component will automatically manage the active loader at correct points in the scene lifecycle.
    /// The user can override certain points in the active loader lifecycle and manually manage them by toggling the *Automatic Loading* and *Automatic Running*
    /// properties through the inspector UI. Disabling *Automatic Loading* implies the the user is responsible for the full lifecycle
    /// of the manager. Toggling this to false also toggles automatic running to false.
    ///
    /// Disabling *Automatic Running* implies that the user is responsible for starting and stopping.
    ///
    /// Automatic lifecycle management is executed as follows
    ///
    /// * OnEnable -> <see cref="InitializeLoader"/>. The loader list will be iterated over and the first successful loader will be set as the active loader.
    /// * Start -> <see cref="StartSubsystems"/>. Ask the active loader to start all subsystems.
    /// * OnDisable -> <see cref="StopSubsystems"/>. Ask the active loader to stop all subsystems.
    /// * OnDestroy -> <see cref="DeinitializeLoader"/>. Deinitialize and remove the active loader.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerIdentitySettings", menuName = "PlayerIdentitySettings", order = 1)]
    public sealed class PlayerIdentityManagerSettings : ScriptableObject
    {
        [HideInInspector]
        bool m_InitializationComplete = false;
        
        [SerializeField]
        [Tooltip("Determines if the PlayerIdentity Manager instance is responsible for creating and destroying the appropriate loader instance.")]
        [FormerlySerializedAs("AutomaticLoading")]
        bool m_AutomaticLoading = false;

        /// <summary>
        /// Get and set Automatic Loading state for this manager. When this is true, the manager will automatically call
        /// <see cref="InitializeLoader"/> and <see cref="DeinitializeLoader"/> for you. When false <see cref="automaticRunning"/>
        /// is also set to false and remains that way. This means that disabling automatic loading disables all automatic behavior
        /// for the manager.
        /// </summary>
        public bool automaticLoading
        {
            get { return m_AutomaticLoading; }
            set { m_AutomaticLoading = value; }
        }

        [SerializeField]
        [Tooltip("Determines if Player Identity will send a verification email to users who sign up with email or upgrade their guest account with an email address.")]
        bool m_AutomaticallyVerifyEmail = true;

        /// <summary>
        /// Get and set whether the PlayerIdentity is going to trigger email verification after user sign-up or add email.
        /// </summary>
        public bool automaticallyVerifyEmail
        {
            get { return m_AutomaticallyVerifyEmail; }
            set { m_AutomaticallyVerifyEmail = value; }
        }
        
        [SerializeField]
        [Tooltip("Determines if the PlayerIdentity Manager instance is responsible for starting and stopping subsystems for the active loader instance.")]
        [FormerlySerializedAs("AutomaticRunning")]
        bool m_AutomaticRunning = false;

        /// <summary>
        /// Get and set automatic running state for this manager. When set to true the manager will call <see cref="StartSubsystems"/>
        /// and <see cref="StopSubsystems"/> APIs at appropriate times. When set to false, or when <see cref="automaticLoading"/> is false
        /// then it is up to the user of the manager to handle that same functionality.
        /// </summary>
        public bool automaticRunning
        {
            get { return m_AutomaticRunning; }
            set { m_AutomaticRunning = value; }
        }

        [SerializeField]
        [Tooltip("The selected PlayerIdentity Backend instance.")]
        [FormerlySerializedAs("BackendLoader")]
        PlayerIdentityLoader m_BackendLoader;

        /// <summary>
        /// The selected PlayerIdentity Backend instance.
        /// </summary>
        public PlayerIdentityLoader backendLoader
        {
            get { return m_BackendLoader; }
            internal set { m_BackendLoader = value; }
        }

        [SerializeField]
        [Tooltip("List of PlayerIdentity Login Provider Loader instances arranged in desired load order.")]
        [FormerlySerializedAs("ProviderLoaders")]
        List<PlayerIdentityLoader> m_ProviderLoaders = new List<PlayerIdentityLoader>();

        /// <summary>
        /// List of provider loaders currently managed by this PlayerIdentity Manager instance.
        /// </summary>
        public List<PlayerIdentityLoader> providerLoaders
        {
            get { return m_ProviderLoaders; }
        }

        /// <summary>
        /// Read only boolean letting us know if initialization is completed. 
        /// </summary>
        public bool isInitializationComplete
        {
            get { return m_InitializationComplete; }
        }

        public void InitializeLoader()
        {
            if (m_InitializationComplete)
            {
                Utils.Logger.Warning(
                    "PlayerIdentity Management has already initialized an active loader in this scene." +
                    "Please make sure to stop all subsystems and deinitialize the active loader before initializing a new one.");
                return;
            }

            if (backendLoader != null)
            {
                if (!backendLoader.Initialize())
                {
                    Utils.Logger.Error("PlayerIdentity Management fails to initialize the backend loader " + backendLoader.name);
                    return;
                }
            }

            foreach (var loader in providerLoaders)
            {
                if (loader != null)
                {
                    if (!loader.Initialize())
                    {
                        Utils.Logger.Error("PlayerIdentity Management fails to initialize the provider loader " + loader.name);
                        return;
                    }
                }
            }

            m_InitializationComplete = true;
        }

        /// <summary>
        /// If there is an active loader, this will request the loader to start all the subsystems that it
        /// is managing.
        ///
        /// You must wait for <see cref="isInitializationComplete"/> to be set to true prior to calling this API.
        /// </summary>
        public void StartSubsystems()
        {
            if (!m_InitializationComplete)
            {
                Utils.Logger.Warning(
                    "Call to StartSubsystems without an initialized manager." +
                    "Please make sure wait for initialization to complete before calling this API.");
                return;
            }

            if (backendLoader != null)
            {
                if (!backendLoader.Start())
                {
                    Utils.Logger.Error("PlayerIdentity Management fails to start the backend loader " + backendLoader.name);
                    return;
                }
            }

            foreach (var loader in providerLoaders)
            {
                if (loader != null)
                {
                    if (!loader.Start())
                    {
                        Utils.Logger.Error("PlayerIdentity Management fails to start the provider loader " + loader.name);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// If there is an active loader, this will request the loader to stop all the subsystems that it
        /// is managing.
        ///
        /// You must wait for <see cref="isInitializationComplete"/> to be set to tru prior to calling this API.
        /// </summary>
        public void StopSubsystems()
        {
            if (!m_InitializationComplete)
            {
                Utils.Logger.Warning(
                    "Call to StopSubsystems without an initialized manager." +
                    "Please make sure wait for initialization to complete before calling this API.");
                return;
            }

            foreach (var loader in providerLoaders)
            {
                if (loader != null)
                {
                    if (!loader.Stop())
                    {
                        Utils.Logger.Error("PlayerIdentity Management fails to stop the provider loader " + loader.name);
                        return;
                    }
                }
            }
            
            if (backendLoader != null)
            {
                if (!backendLoader.Stop())
                {
                    Utils.Logger.Error("PlayerIdentity Management fails to stop the backend loader " + backendLoader.name);
                    return;
                }
            }
        }

        /// <summary>
        /// If there is an active loader, this function will deinitialize it and remove the active loader instance from
        /// management. We will automatically call <see cref="StopSubsystems"/> prior to deinitialization to make sure
        /// that things are cleaned up appropriately.
        ///
        /// You must wait for <see cref="isInitializationComplete"/> to be set to tru prior to calling this API.
        ///
        /// Upon return <see cref="isInitializationComplete"/> will be rest to false;
        /// </summary>
        public void DeinitializeLoader()
        {
            if (!m_InitializationComplete)
            {
                Utils.Logger.Warning(
                    "Call to DeinitializeLoader without an initialized manager." +
                    "Please make sure wait for initialization to complete before calling this API.");
                return;
            }

            StopSubsystems();
            
            foreach (var loader in providerLoaders)
            {
                if (loader != null)
                {
                    if (!loader.Stop())
                    {
                        Utils.Logger.Error("PlayerIdentity Management fails to stop the provider loader " + loader.name);
                        return;
                    }
                }
            }
            
            if (backendLoader != null)
            {
                if (!backendLoader.Stop())
                {
                    Utils.Logger.Error("PlayerIdentity Management fails to stop the backend loader " + backendLoader.name);
                    return;
                }
            }

            m_InitializationComplete = false;
        }

        // Use this for initialization
        void Start()
        {
            if (automaticLoading && automaticRunning)
            {
                StartSubsystems();
            }
        }

        void OnDisable()
        {
            if (automaticLoading && automaticRunning)
            {
                StopSubsystems();
            }
        }

        void OnDestroy()
        {
            if (automaticLoading)
            {
                DeinitializeLoader();
            }
        }
    }
}
