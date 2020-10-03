using UnityEngine.Scripting;

namespace UnityEngine.PlayerIdentity
{

    /// <summary>
    /// The identity login subsystem provides the ability to login with a third party identity system
    /// </summary>
    [Preserve]
    public abstract class PlayerIdentityLoginSubsystem : Subsystem<PlayerIdentityLoginSubsystemDescriptor>
    {
        public struct IdentityLoginCallbackArgs
        {
            /// <summary>
            /// The logged in user info after the call is done.
            /// </summary>
            public UserInfo userInfo;
            
            /// <summary>
            /// The external token after the call is done.
            /// </summary>
            public ExternalToken externalToken;
            
            /// <summary>
            /// Whether the call ends up with an error.
            /// </summary>
            public Error error;

            /// <summary>
            /// The state of the user's authorization.
            /// </summary>
            public UserCredentialState credentialState;

            /// <summary>
            /// The subsystem which triggered the callback.
            /// </summary>
            public PlayerIdentityLoginSubsystem subsystem;
        }

        /// <summary>
        /// Callback is the callback delegate type.
        /// </summary>
        public delegate void Callback(IdentityLoginCallbackArgs args);

        /// <summary>
        /// Logout the user from the id provider.
        /// </summary>
        /// <param name="loginArgs">The optional login args specific to the provider.</param>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void Login(object loginArgs, Callback callback);

        /// <summary>
        /// The function that returns the button game object.
        /// </summary>
        public abstract GameObject GetButton();

        /// <summary>
        /// The function that is called in every frame.
        /// </summary>
        public virtual void Update() {}

        /// <summary>
        /// OnApplicationPause pass the application pause updates from Unity. 
        /// </summary>
        public virtual void OnApplicationPause(bool pauseStatus) {}
        
#if !UNITY_2019_3_OR_NEWER
        /// <summary>
        ///   <para>Destroys this instance of a subsystem.</para>
        /// </summary>
        public override void Destroy()
        {
            this.OnDestroy();
        }
        
        protected abstract void OnDestroy();
#endif
    }

}
