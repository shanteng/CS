using System;

namespace UnityEngine.PlayerIdentity.Management
{
    /// <summary>
    /// PlayerIdentityLoaderCategory enum used to shows the loader category.
    /// 
    /// </summary>
    public enum PlayerIdentityLoaderCategory
    {
        /// <summary>
        /// IdentityBackend is the backend service that unify different IDs from ID providers.
        /// It also manages the users from password based sign-in.
        /// </summary>
        IdentityBackend,
        /// <summary>
        /// IdentityProvider is the third party logins that allows user to sign-in or create account with.
        /// </summary>
        IdentityProvider,
    }
    
    /// <summary>
    /// PlayerIdentity Loader abstract class used as a base class for specific provider implementations. Providers should implement
    /// subclasses of this to provide specific initialization and management implementations that make sense for their supported
    /// scenarios and needs.
    /// </summary>
    public abstract class PlayerIdentityLoader : ScriptableObject
    {
        /// <summary>
        /// The subsystem descriptor handled by the loader.
        /// </summary>
        public abstract ISubsystemDescriptor SubsystemDescriptor { get; }

        /// <summary>
        /// The type of the player identity loader.
        /// </summary>
        public abstract PlayerIdentityLoaderCategory Category { get; }

        /// <summary>
        /// The display name if this is a login provider loader.
        /// </summary>
        public string displayName
        {
            get
            {
                switch (SubsystemDescriptor) {
                    case PlayerIdentityBackendSubsystemDescriptor _:
                        return (SubsystemDescriptor as PlayerIdentityBackendSubsystemDescriptor)?.displayName;
                    case PlayerIdentityLoginSubsystemDescriptor _:
                        return (SubsystemDescriptor as PlayerIdentityLoginSubsystemDescriptor)?.displayName;
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// The login provider ID if this is a login provider loader.
        /// </summary>
        public string LoginProviderID
        {
            get
            {
                return (SubsystemDescriptor as PlayerIdentityLoginSubsystemDescriptor)?.loginProviderID;
            }
        }

        /// <summary>
        /// Initialize the loader. This should initialize all subsystems to support the desired runtime setup this
        /// loader represents.
        /// </summary>
        ///
        /// <returns>Whether or not initialization succeeded.</returns>
        public virtual bool Initialize() { return false; }

        /// <summary>
        /// Ask loader to start all initialized subsystems.
        /// </summary>
        ///
        /// <returns>Whether or not all subsystems were successfully started.</returns>
        public virtual bool Start() { return false; }

        /// <summary>
        /// Ask loader to stop all initialized subsystems.
        /// </summary>
        ///
        /// <returns>Whether or not all subsystems were successfully stopped.</returns>
        public virtual bool Stop() { return false; }

        /// <summary>
        /// Ask loader to deinitialize all initialized subsystems.
        /// </summary>
        ///
        /// <returns>Whether or not deinitialization succeeded.</returns>
        public virtual bool Deinitialize() { return false; }

        /// <summary>
        /// Gets the loaded subsystem of the specified type. Implementation dependent as only implementations
        /// know what they have loaded and how best to get it..
        /// </summary>
        ///
        /// <paramref name="T">Type of the subsystem to get</paramref>
        ///
        /// <returns>The loaded subsystem or null if not found.</returns>
        public abstract T GetLoadedSubsystem<T>()  where T : class, ISubsystem;
    }
}
