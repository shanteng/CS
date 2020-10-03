using UnityEngine.Scripting;

namespace UnityEngine.PlayerIdentity
{
    /// <summary>
    /// Player Identity Backend is responsible for the following use cases:
    /// 1. Link different identity providers together to provide a centralized user ID.
    /// 2. Provide first-party email/password login.
    /// 3. Provide first-party anonymous login.
    /// The backend provides low level abstraction. The login state/flow management is in PlayerIdentityManagement class.
    /// </summary>
    [Preserve]
    public abstract class PlayerIdentityBackendSubsystem : Subsystem<PlayerIdentityBackendSubsystemDescriptor>
    {
        public struct IdentityCallbackArgs
        {
            /// <summary>
            /// The logged in user info after the call is done.
            /// </summary>
            public UserInfo userInfo;
            
            /// <summary>
            /// The access token after the call is done.
            /// </summary>
            public string accessToken;
            
            /// <summary>
            /// Whether the call ends up with an error.
            /// </summary>
            public Error error;

            /// <summary>
            /// The subsystem which triggered the callback.
            /// </summary>
            public PlayerIdentityBackendSubsystem subsystem;
        }

        public delegate void Callback(IdentityCallbackArgs args);

        /// <summary>
        /// Logout all the sessions.
        /// </summary>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void Logout(Callback callback);

        /// <summary>
        /// Restore the login information from persisted token at game start.
        /// </summary>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void RestoreLogin(Callback callback);

        /// <summary>
        /// Trigger the anonymous login.
        /// </summary>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void LoginAnonymous(Callback callback);

        /// <summary>
        /// Login the user using email password.
        /// </summary>
        /// <param name="email">The email used to login the user.</param>
        /// <param name="password">The password used to login the user.</param>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void Login(string email, string password, Callback callback);

        /// <summary>
        /// Register the user using email password.
        /// </summary>
        /// <param name="email">The email used to create the user.</param>
        /// <param name="password">The password used to create the user.</param>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void Register(string email, string password, Callback callback);

        /// <summary>
        /// Login/Create the user using external token.
        /// </summary>
        /// <param name="externalToken"></param>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void ExternalAuth(ExternalToken externalToken, Callback callback);

        /// <summary>
        /// Refresh access token.
        /// </summary>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void RefreshAccessToken(Callback callback);
        
        /// <summary>
        /// Link an user's id from an Identity Provider to the main user.
        /// </summary>
        /// <param name="info">This is the Identity Provider information to link to the main user.</param>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void Link(ExternalToken info, Callback callback);
        
        /// <summary>
        /// Link an user's id to Phone.
        /// </summary>
        /// <param name="userId">This is the user id to get information about.</param>
        /// <param name="code">The sms code.</param>
        /// <param name="verificationId">The verification id of the code.</param>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void LinkSmsCode(string userId, string code, string verificationId, Callback callback);
        
        /// <summary>
        /// Unlink an user's id from a set of Identity Providers.
        /// </summary>
        /// <param name="info">This is the Identity Provider information to link to the main user.</param>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void Unlink(string[] idProviders, Callback callback);
        
        /// <summary>
        /// Change the password for the user.
        /// </summary>
        /// <param name="password">This is the current password of the user.</param>
        /// <param name="newPassword">This is the new password of the user.</param>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void ChangePassword(string password, string newPassword, Callback callback);

        /// <summary>
        /// Reset the password for a particular email.
        /// </summary>
        /// <param name="email">This is the email to reset the passord for.</param>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void ResetPassword(string email, Callback callback);

        /// <summary>
        /// Create email credential to current user.
        /// The current user can either be an anonymous user or created via social login.
        /// </summary>
        /// <param name="email">The email used to upgrade the user.</param>
        /// <param name="password">The password used to login the user after upgrading.</param>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void CreateCredential(string email, string password, Callback callback);
        
        /// <summary>
        /// Get information about the current user.
        /// </summary>
        /// <param name="userId">This is the user id to get information about.</param>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void GetUser(string userId, Callback callback);
        
        /// <summary>
        /// Update information about the current user.
        /// The ID provider needs to define which fields can be updated in user info.
        /// </summary>
        /// <param name="userInfo">The updated user info.</param>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void UpdateUser(UserInfo userInfo, Callback callback);

        /// <summary>
        /// Verify the email passed in by sending an code to the user's email.
        /// </summary>
        /// <param name="email">The email to send the code to.</param>
        /// <param name="callback">The callback after the async operation is done.</param>
        public abstract void VerifyEmail(string email, Callback callback);

        /// <summary>
        /// The function that is called in every frame
        /// </summary>
        public virtual void Update() {}
        
        public struct CreateSmsCodeCallbackArgs
        {
            /// <summary>
            /// Verification id used for sms code login.
            /// </summary>
            public string verificationId;
            
            /// <summary>
            /// Whether the call ends up with an error.
            /// </summary>
            public Error error;
        }

        public delegate void CreateSmsCodeCallBack(CreateSmsCodeCallbackArgs createSmsCodeCallbackArgs);
        
        
        public abstract void PhoneLogin(string smsCodeText, string verificationIdText, PlayerIdentityBackendSubsystem.Callback callback);
        public abstract void RequestSmsCode(string phoneNumber, CreateSmsCodeCallBack createSmsCodeCallBack);

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
