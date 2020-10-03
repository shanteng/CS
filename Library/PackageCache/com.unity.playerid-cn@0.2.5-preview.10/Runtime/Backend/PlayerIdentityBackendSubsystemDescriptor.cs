using System;

namespace UnityEngine.PlayerIdentity
{
    /// <summary>
    /// Encapsulates the parameters for creating a new <see cref="PlayerIdentityBackendSubsystem"/>
    /// </summary>
    public struct PlayerIdentityBackendSubsystemCInfo
    {
        /// <summary>
        /// Specifies an identifier for the backend implementation of the subsystem
        /// </summary>
        /// <value>
        /// The identifier for the backend implementation of the subsystem
        /// </value>
        public string id { get; set; }
        
        /// <summary>
        /// Specifies a display name for the backend implementation of the subsystem
        /// </summary>
        /// <value>
        /// The display name for the backend implementation of the subsystem
        /// </value>
        public string displayName { get; set; }

        /// <summary>
        /// Specifies the provider implementation type to use for instantiation
        /// </summary>
        /// <value>
        /// The provider implementation type to use for instantiation
        /// </value>
        public Type implementationType { get; set; }

        /// <summary>
        /// Whether the backend supports anonymous login
        /// </summary>
        public bool supportsAnonymousLogin { get; set; }

        /// <summary>
        /// Whether the backend supports email/password login
        /// </summary>
        public bool supportsEmailPasswordLogin { get; set; }
        
        /// <summary>
        /// Whether the backend supports phone login.
        /// </summary>
        public bool supportsPhoneLogin { get; set; }
        
        /// <summary>
        /// Whether the backend supports text message login
        /// </summary>
        public bool supportsTextMessageLogin { get; set; }
    }

    public class PlayerIdentityBackendSubsystemDescriptor : SubsystemDescriptor<PlayerIdentityBackendSubsystem>
    {
        /// <summary>
        /// The name of the ID provider that will show in the button
        /// </summary>
        public string displayName { get; private set; }

        /// <summary>
        /// Whether the backend supports anonymous login
        /// </summary>
        public bool supportsAnonymousLogin { get; private set; }

        /// <summary>
        /// Whether the backend supports email/password login
        /// </summary>
        public bool supportsEmailPasswordLogin { get; private set; }
        
        public bool supportPhoneNumberLogin { get; private set; }
        
        /// <summary>
        /// Whether the backend supports text message login
        /// </summary>
        public bool supportsTextMessageLogin { get; set; }

        private PlayerIdentityBackendSubsystemDescriptor(PlayerIdentityBackendSubsystemCInfo param)
        {
            id = param.id;
            displayName = param.displayName;
            subsystemImplementationType = param.implementationType;

            supportPhoneNumberLogin = param.supportsPhoneLogin;
            supportsEmailPasswordLogin = param.supportsEmailPasswordLogin;
            supportsAnonymousLogin = param.supportsAnonymousLogin;
            supportsTextMessageLogin = param.supportsTextMessageLogin;
        }
        
        public static PlayerIdentityBackendSubsystemDescriptor RegisterDescriptor(PlayerIdentityBackendSubsystemCInfo param)
        {
            var desc = new PlayerIdentityBackendSubsystemDescriptor(param);
            return SubsystemRegistration.CreateDescriptor(desc) ? desc : null;
        }
    }
}
