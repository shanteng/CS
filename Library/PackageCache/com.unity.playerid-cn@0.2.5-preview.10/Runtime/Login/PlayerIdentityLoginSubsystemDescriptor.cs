using System;
using UnityEngine.Scripting;

namespace UnityEngine.PlayerIdentity
{
    [Preserve]
    public class PlayerIdentityLoginSubsystemDescriptor : SubsystemDescriptor<PlayerIdentityLoginSubsystem>
    {
        public static readonly RuntimePlatform[] AllPlatforms = new RuntimePlatform[0];
        
        public const RuntimePlatform NoDefaultInPlatform = RuntimePlatform.OSXEditor;
        
        /// <summary>
        /// All supported platforms. Empty array means all platforms.
        /// </summary>
        public RuntimePlatform[] supportedPlatforms { get; private set; }
        
        /// <summary>
        /// Whether this login subsystem is the default one in the platform.
        /// Leave it to OSXEditor if this is not a preferred one.
        /// </summary>
        public RuntimePlatform defaultInPlatform { get; private set; }
        
        /// <summary>
        /// The name of the ID provider that will show in the button.
        /// </summary>
        public string displayName { get; private set; }
        
        /// <summary>
        /// The login provider ID that will show up in the callback event.
        /// </summary>
        public string loginProviderID { get; private set; }

        private PlayerIdentityLoginSubsystemDescriptor(
            string id, Type implType, RuntimePlatform[] supportedPlatforms, RuntimePlatform defaultInPlatform,
            string displayName, string providerId)
        {
            this.id = id;
            subsystemImplementationType = implType;
            this.supportedPlatforms = supportedPlatforms;
            this.defaultInPlatform = defaultInPlatform;
            this.displayName = displayName;
            this.loginProviderID = providerId;
        }

        public static PlayerIdentityLoginSubsystemDescriptor RegisterDescriptor(
            string id, Type implType, RuntimePlatform[] supportedPlatforms, RuntimePlatform defaultInPlatform,
            string displayName, string providerId)
        {
            var desc = new PlayerIdentityLoginSubsystemDescriptor(
                id, implType, supportedPlatforms, defaultInPlatform, displayName, providerId);
            return SubsystemRegistration.CreateDescriptor(desc) ? desc : null;
        }
    }
}
