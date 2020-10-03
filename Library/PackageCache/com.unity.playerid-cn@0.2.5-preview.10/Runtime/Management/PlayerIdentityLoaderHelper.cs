using System;
using System.Collections.Generic;

namespace UnityEngine.PlayerIdentity.Management
{
    /// <summary>
    /// PlayerIdentity Loader abstract subclass used as a base class for specific provider implementations. Class provides some
    /// helper logic that can be used to handle subsystem handling in a type safe manner, reducing potential boilerplate
    /// code.
    /// PlayerIdentityLoaderHelper is good for Loader that only allows a single subsystem type.
    /// </summary>
    public abstract class PlayerIdentityLoaderHelper<T, TDescriptor> : PlayerIdentityLoader
        where T : class, ISubsystem
        where TDescriptor : class, ISubsystemDescriptor
    {
        /// <summary>
        /// Map of loaded subsystems. Used so we don't always have to refer to SubsystemManager and do a manual
        /// search to find the instance we loaded.
        /// </summary>
        protected Dictionary<Type, ISubsystem> m_SubsystemInstanceMap = new Dictionary<Type, ISubsystem>();

        /// <summary>
        /// The specific subsystem descriptor with type TDescriptor.
        /// </summary>
        protected TDescriptor m_SubsystemDescriptor;

        /// <summary>
        /// The specific loaded subsystem with type T.
        /// </summary>
        protected T m_Subsystem;

        /// <summary>
        /// Register the subsystem.
        /// </summary>
        public abstract void Register();

        /// <summary>
        /// LoadedSubsystem returns the specific loaded subsystem with type T.
        /// </summary>
        public T LoadedSubsystem
        {
            get
            {
                if (m_SubsystemDescriptor == null)
                {
                    Initialize();
                }
                return m_Subsystem;                
            }
        }

        /// <summary>
        /// SubsystemDescriptor returns the subsystem descriptor of the loaded subsystem.
        /// </summary>
        public override ISubsystemDescriptor SubsystemDescriptor
        {
            get
            {
                if (m_SubsystemDescriptor == null)
                {
                    Initialize();
                }
                return m_SubsystemDescriptor;
            }
        }

        public override PlayerIdentityLoaderCategory Category
        {
            get
            {
                if (typeof(TDescriptor) == typeof(PlayerIdentityBackendSubsystemDescriptor))
                {
                    return PlayerIdentityLoaderCategory.IdentityBackend;
                }
                else if (typeof(TDescriptor) == typeof(PlayerIdentityLoginSubsystemDescriptor))
                {
                    return PlayerIdentityLoaderCategory.IdentityProvider;
                }
                else
                {
                    throw new InvalidOperationException("Unexpected subsystem descriptor type.");
                }
            }
        }

        /// <summary>
        /// Implement the generic GetLoadedSubsystem inherited from PlayerIdentityLoader.
        /// </summary>
        /// <paramref name="TSubsystem">Type of the subsystem to get.</paramref>
        /// <returns>The loaded subsystem or null if not found.</returns>
        public override TSubsystem GetLoadedSubsystem<TSubsystem>()
        {
            if (m_Subsystem is TSubsystem)
            {
                return m_Subsystem as TSubsystem;
            }
            
            ISubsystem subsystem;
            m_SubsystemInstanceMap.TryGetValue(typeof(TSubsystem), out subsystem);
            return subsystem as TSubsystem;
        }
        
        /// <summary>
        /// Implement Start() from PlayerIdentityLoader
        /// </summary>
        public override bool Start()
        {
            T subsystem = LoadedSubsystem;
            subsystem?.Start();
            return true;
        }
        
        /// <summary>
        /// Implement Stop() from PlayerIdentityLoader
        /// </summary>
        public override bool Stop()
        {
            T subsystem = LoadedSubsystem;
            subsystem?.Stop();
            return true;
        }
        
        /// <summary>
        /// Implement Deinitialize() from PlayerIdentityLoader
        /// </summary>
        public override bool Deinitialize()
        {
            T subsystem = LoadedSubsystem;
            subsystem?.Destroy();
            return true;
        }

        /// <summary>
        /// Creates a subsystem given a specific subsystem id.
        /// </summary>
        /// <param name="id">The identifier key of the particular subsystem implementation being requested.</param>
        protected bool CreateSubsystem(string id)
        {
            Register();
            
            List<TDescriptor> descriptors = new List<TDescriptor>();
            
            // load all available descriptors with type TDescriptor from SubsystemManager. 
            SubsystemManager.GetSubsystemDescriptors(descriptors);

            if (descriptors.Count > 0)
            {
                foreach (var descriptor in descriptors)
                {
                    ISubsystem subsys = null;
                    if (string.Compare(descriptor.id, id, true) == 0)
                    {
                        subsys = descriptor.Create();
                    }
                    if (subsys != null)
                    {
                        m_SubsystemInstanceMap[typeof(T)] = subsys;
                        m_Subsystem = subsys as T;
                        m_SubsystemDescriptor = descriptor;
                        break;
                    }
                }
            }
            
            if (m_Subsystem == null)
            {
                Utils.Logger.Error("Failed to load subsystem: " + id);
            }

            return m_Subsystem != null;
        }
    }
}
