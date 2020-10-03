using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.PlayerIdentity.UI;


namespace UnityEngine.PlayerIdentity
{
    /// <summary>
    /// PlayerIdentityManager provides functions to manage the IPlayerIdentityCore instance.
    /// </summary>
    public sealed class PlayerIdentityManager
    {
        private static IPlayerIdentityCore s_Current;

        public static IPlayerIdentityCore Current
        {
            get { return s_Current; }
            internal set { s_Current = value; }
        }
    }

    /// <summary>
    /// IPlayerIdentityCallbacks provides a way to listen to
    /// Player Identity events without hooking up in UnityEvent.
    /// </summary>
    public interface IPlayerIdentityCallbacks
    {
        /// <summary>
        /// OnAnonymousLogin is triggered when anonymous login is successful.
        /// Use sender.userInfo to get the information about signed in user.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        void OnAnonymousLogin(IPlayerIdentityCore sender);
        
        /// <summary>
        /// OnLogin is triggered when non-anonymous login is successful.
        /// Use sender.userInfo to get the information about signed in user.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        void OnLogin(IPlayerIdentityCore sender);
        
        /// <summary>
        /// OnLogout is triggered when the user is logged out.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        void OnLogout(IPlayerIdentityCore sender);
        
        /// <summary>
        /// OnAccessTokenChanged is triggered when a new access token is get from the server.
        /// Use sender.accessToken to get the new token.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        void OnAccessTokenChanged(IPlayerIdentityCore sender);
        
        /// <summary>
        /// OnError is triggered whenever there is an error in the API calls.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        void OnError(IPlayerIdentityCore sender, Error error);
        
        /// <summary>
        /// OnLink is triggered when a link event occurs.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        void OnLink(IPlayerIdentityCore sender);
        
        /// <summary>
        /// OnUnlink is triggered when a unlink event occurs.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        void OnUnlink(IPlayerIdentityCore sender);
        
        /// <summary>
        /// OnResetPassword is triggered when a reset password event occurs.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        void OnResetPassword(IPlayerIdentityCore sender);
        
        /// <summary>
        /// OnCreateCredential is triggered when a create email credential event occurs.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        void OnCreateCredential(IPlayerIdentityCore sender);
        
        /// <summary>
        /// OnChangePassword is triggered when a change password event occurs.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        void OnChangePassword(IPlayerIdentityCore sender);

        /// <summary>
        /// OnGetUser is triggered when a get user event occurs.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        void OnGetUser(IPlayerIdentityCore sender);
        
        /// <summary>
        /// OnUpdateUser is triggered when an update user event occurs.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        void OnUpdateUser(IPlayerIdentityCore sender);

        /// <summary>
        /// OnVerifyEmail is triggered when a verify email event occurs.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        void OnVerifyEmail(IPlayerIdentityCore sender);
    }

    public delegate void PlayerIdentityCallback(IPlayerIdentityCore sender);

    /// <summary>
    /// IPlayerIdentityCore provides the core functions of player identity package.
    /// It chains LoginSubsystems together with LoginBackend. 
    /// </summary>
    public interface IPlayerIdentityCore
    {
        /// <summary>
        /// Get the current identity backend subsystem.
        /// </summary>
        PlayerIdentityBackendSubsystem loginBackend { get; }
        
        /// <summary>
        /// Get a list of the currently enabled login providers.
        /// </summary>
        List<PlayerIdentityLoginSubsystem> loginProviders { get; }
        
        /// <summary>
        /// Get the current login status.
        /// </summary>
        LoginStatus loginStatus { get; }
        
        /// <summary>
        /// Get the current user ID if the user is logged in.
        /// </summary>
        string userId { get; }
        
        /// <summary>
        /// Get the current user's display name if the user is logged in.
        /// </summary>
        string displayName { get; }
        
        /// <summary>
        /// Get the current user's information.
        /// </summary>
        UserInfo userInfo { get; }
        
        /// <summary>
        /// Get the current access token of the user.
        /// </summary>
        string accessToken { get; }
        
        /// <summary>
        /// Get the last error from async API calls.
        /// </summary>
        Error error { get; }

        /// <summary>
        /// Get the list of player identity callbacks.
        /// The list can be modified by the caller to add more callbacks.
        /// </summary>
        List<IPlayerIdentityCallbacks> Callbacks { get; }

        /// <summary>
        /// Trigger logout action.
        /// The result can be observed from event callback.
        /// </summary>
        void Logout(PlayerIdentityCallback callback = null);
        
        /// <summary>
        /// Trigger restore login action.
        /// Restore login will try to recover the login session persisted on the device.
        /// The implementation of how the session is persisted is up to the identity backend.
        /// This function will be automatically called when the game starts up.
        /// The result can be observed from event callback.
        /// </summary>
        void RestoreLogin(PlayerIdentityCallback callback = null);
        
        /// <summary>
        /// Trigger anonymous login action.
        /// Anonymous login will try to login anonymously.
        /// The implementation of how to identify the anonymous session up to the identity backend.
        /// The result can be observed from event callback.
        /// </summary>
        void LoginAnonymous(PlayerIdentityCallback callback = null);
        
        /// <summary>
        /// Trigger user login via email and password.
        /// The result can be observed from event callback.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="password">The password of the user.</param>
        void Login(string email, string password, PlayerIdentityCallback callback = null);
        
        /// <summary>
        /// Trigger user registration via email and password.
        /// The result can be observed from event callback.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="password">The password of the user.</param>
        void Register(string email, string password, PlayerIdentityCallback callback = null);
        
        /// <summary>
        /// Trigger login of the user via external login provider.
        /// This function will call external provider to trigger the login.
        /// The result can be observed from event callback.
        /// </summary>
        void ExternalLogin(string loginProviderId, PlayerIdentityCallback callback = null);
        
        /// <summary>
        /// Refresh the access token of the user.
        /// The result can be observed from event callback.
        /// If there's already a refresh request in progress, no new refresh request will be made.
        /// And the providing callback will be invoked when the current refresh request is done.
        /// </summary>
        void RefreshAccessToken(PlayerIdentityCallback callback = null);
        
        /// <summary>
        /// Trigger an link for an user to a the specified id provider.
        /// The result can be observed from event callback.
        /// </summary>
        /// <param name="loginProviderId">The id provider ID to link the user to.</param>
        void Link(string loginProviderId, PlayerIdentityCallback callback = null);
        
        /// <summary>
        /// Trigger an unlink from a set of id providers.
        /// The result can be observed from event callback.
        /// </summary>
        /// <param name="idProviders">The list of idProviders to unlink from.</param>
        void Unlink(string[] idProviders, PlayerIdentityCallback callback = null);

        /// <summary>
        /// Trigger an event to send a password reset.
        /// The result can be observed from event callback.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        void ResetPassword(string email, PlayerIdentityCallback callback = null);

        /// <summary>
        /// Trigger an upgrade of an anonymous user to an email user.
        /// The result can be observed from event callback.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="password">The password of the user.</param>
        void CreateCredential(string email, string password, PlayerIdentityCallback callback = null);

        /// <summary>
        /// Trigger change password for password user.
        /// The result can be observed from event callback.
        /// </summary>
        /// <param name="password">The current password of the user.</param>
        /// <param name="newPassword">The new password of the user.</param>
        void ChangePassword(string password, string newPassword, PlayerIdentityCallback callback = null);

        /// <summary>
        /// Update information about the current user.
        /// The ID provider needs to define which fields can be updated in user info.
        /// </summary>
        /// <param name="userInfo">The updated user info.</param>
        /// <param name="callback">The callback after the async operation is done.</param>
        void UpdateUser(UserInfo userInfo, PlayerIdentityCallback callback = null);

        /// <summary>
        /// Trigger an event to send a verify email.
        /// The result can be observed from event callback.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        void VerifyEmail(string email, PlayerIdentityCallback callback = null);
        
        /// <summary>
        /// Trigger a get user event.
        /// The result can be observed from event callback.
        /// </summary>
        void GetUser(PlayerIdentityCallback callback = null);
        
        /// <summary>
        /// Trigger use login via sms code
        /// The result can be observed from event callback.
        /// </summary>
        void PhoneLogin(string smsCodeText, string verificationIdText, PlayerIdentityCallback callback = null);
        
        /// <summary>
        /// Link phone to the user
        /// The result can be observed from event callback.
        /// </summary>
        void LinkSmsCode(string smsCodeText, string verificationIdText, PlayerIdentityCallback callback = null);
        
        /// <summary>
        /// Trigger user registration via email and password.
        /// The result can be observed from event callback.
        /// </summary>
        /// <param name="phoneNumber">The phone number of the user.</param>
        /// <param name="createSmsCodeCallBack">The callback on request sms code</param>
        void RequestSmsCode(string phoneNumber, PlayerIdentityBackendSubsystem.CreateSmsCodeCallBack createSmsCodeCallBack);
    }
}
