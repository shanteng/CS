using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.PlayerIdentity.Management;
using UnityEngine.PlayerIdentity.UI;
using UnityPlayerIdentityUtility = UnityEngine.PlayerIdentity.Utils.Utility;

namespace UnityEngine.PlayerIdentity
{
    public enum LoginStatus
    {
        NotLoggedIn,
        LoginInProgress,
        AnonymouslyLoggedIn,
        LoggedIn,
    }

    /// <summary>
    /// Class to chain multiple LoginSubsystem with the Backend together. 
    /// </summary>
    public class PlayerIdentityCore : MonoBehaviour, IPlayerIdentityCore
    {
        private bool m_Started;
        
        private delegate void Callback();
        private ConcurrentQueue<Callback> m_EventBacklog = new ConcurrentQueue<Callback>();

        private PlayerIdentityBackendSubsystem m_Backend;
        private List<PlayerIdentityLoginSubsystem> m_LoginProviders = new List<PlayerIdentityLoginSubsystem>();

        private List<IPlayerIdentityCallbacks> m_Callbacks = new List<IPlayerIdentityCallbacks>();
        
        private List<PlayerIdentityCallback> m_PendingRefreshCallbacks = new List<PlayerIdentityCallback>();
        
        private static DateTime _epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public PlayerIdentityBackendSubsystem loginBackend
        {
            get { return m_Backend; }
        }
        
        public List<PlayerIdentityLoginSubsystem> loginProviders
        {
            get { return m_LoginProviders; }
        }

        public bool started
        {
            get { return m_Started; }
        }

        public PlayerIdentityCore()
        {
            PlayerIdentityManager.Current = this;
        }

        void Start()
        {
            if (PlayerIdentityGeneralSettings.Instance == null)
            {
                Utils.Logger.Warning("No Loader was configured for use.");
                return;
            }

            var backendLoader = PlayerIdentityGeneralSettings.Instance.Manager.backendLoader;
            if (backendLoader == null)
            {
                Utils.Logger.Error("Identity backend is not configured.");
                return;
            }
            
            var backendSystem = backendLoader.GetLoadedSubsystem<PlayerIdentityBackendSubsystem>();
            if (backendSystem != null)
            {
                Utils.Logger.InfoFormat("BackendSystem is {1} for Provider: {0}", backendLoader.name,
                    backendSystem.running ? "running" : "not running");

                if (!backendSystem.running)
                    backendSystem.Start();

                m_Backend = backendSystem;
            }
            
            List<PlayerIdentityLoader> loaders = PlayerIdentityGeneralSettings.Instance.Manager.providerLoaders;
            foreach (var loader in loaders)
            {
                if (loader == null) continue;
                
                PlayerIdentityLoginSubsystem loginSystem = loader.GetLoadedSubsystem<PlayerIdentityLoginSubsystem>();
                if (loginSystem != null)
                {
                    Utils.Logger.InfoFormat("LoginSystem is {1} for Provider: {0}", loader.name,
                        loginSystem.running ? "running" : "not running");
                    
                    var supportedPlatforms = loginSystem.SubsystemDescriptor.supportedPlatforms;
                    if (supportedPlatforms.Length > 0)
                    {
                        if (supportedPlatforms.Any(p => p == Application.platform))
                        {
                            // supported platform
                            AddLoginProvider(loginSystem);
                        }
                        else
                        {
                            Utils.Logger.InfoFormat("LoginSystem is not supported in platform {1} for Provider: {0}", loader.name,
                                Application.platform);
                        }
                    }
                    else
                    {
                        AddLoginProvider(loginSystem);
                    }
                }
            }
            
            RestoreLogin();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (m_LoginProviders != null)
            {
                foreach (var loginProvider in m_LoginProviders)
                {
                    loginProvider.OnApplicationPause(pauseStatus);
                }
            }
        }

        private void AddLoginProvider(PlayerIdentityLoginSubsystem loginProvider)
        {
            if (!loginProvider.running)
            {
                loginProvider.Start();
            }
            
            if (loginProvider.SubsystemDescriptor.defaultInPlatform !=
                PlayerIdentityLoginSubsystemDescriptor.NoDefaultInPlatform &&
                loginProvider.SubsystemDescriptor.defaultInPlatform == Application.platform)
            {
                // add it as the first (default) provider.
                m_LoginProviders.Insert(0, loginProvider);
            }
            else
            {
                m_LoginProviders.Add(loginProvider);
            }
        }
        
        [Header("Event fired when anonymous login is complete.")]
        public UnityEvent onAnonymousLogin;

        [Header("Event fired when login is complete.")]
        public UnityEvent onLogin;

        [Header("Event fired when logout is complete.")]
        public UnityEvent onLogout;

        [Header("Event fired when access token is changed.")]
        public UnityEvent onAccessTokenChanged;

        [Header("Event fired when there is an error.")]
        public UnityEvent onError;
        
        [Header("Event fired when link is complete.")]
        public UnityEvent onLink;
        
        [Header("Event fired when unlink is complete.")]
        public UnityEvent onUnlink;
        
        [Header("Event fired when change password is complete.")]
        public UnityEvent onChangePassword;

        [Header("Event fired when reset password is complete.")]
        public UnityEvent onResetPassword;
        
        [Header("Event fired when create credential is complete.")]
        public UnityEvent onCreateCredential;
        
        [Header("Event fired when get user is complete.")]
        public UnityEvent onGetUser;

        [Header("Event fired when update user is complete.")]
        public UnityEvent onUpdateUser;

        [Header("Event fired when verify email is complete.")]
        public UnityEvent onVerifyEmail;
        
        public List<IPlayerIdentityCallbacks> Callbacks
        {
            get { return m_Callbacks; }
        }

        private struct State
        {
            public LoginStatus status;
            public UserInfo userInfo;

            public string accessToken;

            public Error error;
        }

        private State m_State;
        

        /// <summary>
        /// ResetState is a function provided for testing purpose.
        /// It resets the cached state and restores login to simulate restart of the app.
        /// </summary>
        public void ResetState()
        {
            m_Started = false;
            m_Backend.Stop();
            
            m_State = new State();
            m_Backend.Start();
            RestoreLogin();
        }
        
        public void Logout(PlayerIdentityCallback callback = null)
        {
            m_Backend.Logout(result =>
            {
                if (HandleLoginResult(result, callback))
                {
                    // Reset the state
                    m_State = new State();
                    
                    ScheduleCallback(SendLogoutEvent, callback);                    
                }
            });
        }

        public void RestoreLogin(PlayerIdentityCallback callback = null)
        {
            m_State.status = LoginStatus.LoginInProgress;
            m_Backend.RestoreLogin(result =>
            {
                if (HandleLoginResult(result, callback))
                {
                    ScheduleLoginEvent(callback);
                }
                m_Started = true;
            });
        }

        public void LoginAnonymous(PlayerIdentityCallback callback = null)
        {
            m_State.status = LoginStatus.LoginInProgress;
            m_Backend.LoginAnonymous(result =>
            {
                if (HandleLoginResult(result, callback))
                {
                    ScheduleCallback(SendAnonymousLoginEvent, callback);
                }
            });
        }

        public void Login(string email, string password, PlayerIdentityCallback callback = null)
        {
            m_State.status = LoginStatus.LoginInProgress;
            m_Backend.Login(email, password, result =>
            {
                if (HandleLoginResult(result, callback))
                {
                    ScheduleCallback(SendLoginEvent, callback);
                }
            });
        }
        
        public void PhoneLogin(string smsCodeText, string verificationIdText, PlayerIdentityCallback callback = null)
        {
            m_State.status = LoginStatus.LoginInProgress;
            m_Backend.PhoneLogin(smsCodeText, verificationIdText, result =>
            {
                if (HandleLoginResult(result, callback))
                {
                    ScheduleCallback(SendLoginEvent, callback);
                }
            });
        }

        public void RequestSmsCode(string phoneNumber, PlayerIdentityBackendSubsystem.CreateSmsCodeCallBack createSmsCodeCallBack)
        {
            m_Backend.RequestSmsCode(phoneNumber, createSmsCodeCallBack);
        }

        public void ExternalLogin(string loginProviderId, PlayerIdentityCallback callback = null)
        {
            m_State.status = LoginStatus.LoginInProgress;
            foreach (var loginProvider in m_LoginProviders)
            {
                if (loginProvider.SubsystemDescriptor.loginProviderID == loginProviderId)
                {
                    loginProvider.Login(null, providerResult =>
                    {
                        // Make sure the callbacks from login providers are running in the main thread
                        ScheduleCallback(() =>
                        {
                            if (providerResult.error != null)
                            {
                                m_State.error = providerResult.error;
                                ScheduleCallback(SendErrorEvent, callback);
                                return;
                            }
                        
                            m_Backend.ExternalAuth(providerResult.externalToken, result =>
                            {
                                if (HandleLoginResult(result, callback))
                                {
                                    ScheduleCallback(SendLoginEvent, callback);
                                }
                            });
                        }, null);
                    });
                    return;
                }
            }
            throw new ArgumentException("Invalid login provider ID: " + loginProviderId);
        }

        public void Register(string email, string password, PlayerIdentityCallback callback = null)
        {
            m_State.status = LoginStatus.LoginInProgress;
            m_Backend.Register(email, password, result =>
            {
                if (HandleLoginResult(result, callback))
                {
                    AutomaticallyVerifyEmail();
                    ScheduleCallback(SendLoginEvent, callback);
                }
            });
        }

        public void RefreshAccessToken(PlayerIdentityCallback callback = null)
        {
            if (m_PendingRefreshCallbacks.Count > 0)
            {
                m_PendingRefreshCallbacks.Add(callback);
                return;
            }

            m_PendingRefreshCallbacks.Add(callback);
            m_State.status = LoginStatus.LoginInProgress;
            m_Backend.RefreshAccessToken(result =>
            {
                PlayerIdentityCallback flushCallback = (IPlayerIdentityCore sender) =>
                {
                    var currentPendings = new List<PlayerIdentityCallback>(m_PendingRefreshCallbacks);
                    m_PendingRefreshCallbacks.Clear();
                    foreach (var t in currentPendings)
                    {
                        if (t != null)
                        {
                            t.Invoke(sender);    
                        }
                    }
                };
                
                if (HandleLoginResult(result, flushCallback))
                {
                    ScheduleCallback(SendAccessTokenChangedEvent, flushCallback);
                }
            });
        }
        
        public void Link(string loginProviderId, PlayerIdentityCallback callback = null)
        {
            foreach (var loginProvider in m_LoginProviders)
            {
                if (loginProvider.SubsystemDescriptor.loginProviderID == loginProviderId)
                {
                    loginProvider.Login(null, providerResult =>
                    {
                        // Make sure the callbacks from login providers are running in the main thread
                        ScheduleCallback(() =>
                        {
                            if (providerResult.error != null)
                            {
                                m_State.error = providerResult.error;
                                ScheduleCallback(SendErrorEvent, callback);
                                return;
                            }
                        
                            m_Backend.Link(providerResult.externalToken, result =>
                            {
                                if (HandleLoginResult(result, callback))
                                {
                                    ScheduleCallback(SendLinkEvent, callback);
                                }
                            });
                        }, null);
                    });
                    return;
                }
            }
            throw new ArgumentException("Invalid login provider ID: " + loginProviderId);
        }

        public void LinkSmsCode(string code, string verificationId, PlayerIdentityCallback callback = null)
        {
            m_Backend.LinkSmsCode(userId, code, verificationId, result =>
            {
                if (HandleLoginResult(result, callback))
                {
                    ScheduleCallback(SendLinkEvent, callback);
                }
            });
        }

        public void Unlink(string[] idProviders, PlayerIdentityCallback callback = null)
        {
            m_Backend.Unlink(idProviders, result =>
            {
                if (HandleLoginResult(result, callback))
                {
                    ScheduleCallback(SendUnlinkEvent, callback);
                }
            });
        }
        
        public void ResetPassword(string email, PlayerIdentityCallback callback = null)
        {
            m_Backend.ResetPassword(email, result =>
            {
                if (HandleResult(result, callback))
                {
                    ScheduleCallback(SendResetPasswordEvent, callback);
                }
            });
        }

        public void CreateCredential(string email, string password, PlayerIdentityCallback callback = null)
        {
            m_Backend.CreateCredential(email, password, result =>
            {
                if (HandleLoginResult(result, callback))
                {
                    AutomaticallyVerifyEmail();
                    
                    m_State.status = LoginStatus.LoggedIn;
                    ScheduleCallback(SendCreateCredentialEvent, callback);
                }
            });
        }
        
        public void ChangePassword(string password, string newPassword, PlayerIdentityCallback callback = null)
        {
            m_Backend.ChangePassword(password, newPassword, result =>
            {
                if (HandleResult(result, callback))
                {
                    ScheduleCallback(SendChangePasswordEvent, callback);
                }
            });
        }

        public void UpdateUser(UserInfo userInfo, PlayerIdentityCallback callback = null)
        {
            m_Backend.UpdateUser(userInfo, result =>
            {
                if (HandleResult(result, callback))
                {
                    ScheduleCallback(SendUpdateUserEvent, callback);
                }
            });
        }

        public void GetUser(PlayerIdentityCallback callback = null)
        {
            m_Backend.GetUser(userId, result =>
            {
                if (HandleResult(result, callback))
                {
                    if (result.userInfo != null)
                    {
                        if (result.userInfo.userId != userId || result.userInfo.disabled)
                        {
                            ScheduleCallback(SendErrorEvent, callback);
                        }

                        // Update state user info
                        // Note that signInProviderId and externalId are only available by sign-in
                        // They are not available by GetUser
                        m_State.userInfo.email = result.userInfo.email;
                        m_State.userInfo.displayName = result.userInfo.displayName;
                        m_State.userInfo.emailVerified = result.userInfo.emailVerified;
                        m_State.userInfo.isAnonymous = result.userInfo.isAnonymous;

                        if (result.userInfo.externalIds != null)
                        {
                            m_State.userInfo.externalIds = result.userInfo.externalIds;
                        }

                        if (!string.IsNullOrEmpty(result.userInfo.photoUrl))
                        {
                            m_State.userInfo.photoUrl = result.userInfo.photoUrl;
                        }
                    }

                    ScheduleCallback(SendGetUserEvent, callback);
                }
            });
        }
        
        public void VerifyEmail(string email, PlayerIdentityCallback callback = null)
        {
            m_Backend.VerifyEmail(email, result =>
            {
                if (HandleResult(result, callback))
                {
                    ScheduleCallback(SendVerifyEmailEvent, callback);
                }
            });
        }

        public LoginStatus loginStatus => m_State.status;

        public string userId => m_State.userInfo?.userId;
        public string displayName => m_State.userInfo?.displayName;

        public UserInfo userInfo => m_State.userInfo;

        public string accessToken => m_State.accessToken;

        public Error error => m_State.error;

        /// <summary>
        /// HandleResult handles the result errors and updates user info and access token.
        /// </summary>
        private bool HandleResult(PlayerIdentityBackendSubsystem.IdentityCallbackArgs result, PlayerIdentityCallback callback)
        {
            if (result.error != null)
            {
                m_State.error = result.error;
                ScheduleCallback(SendErrorEvent, callback);
                return false;
            }
            
            m_State.error = null;

            if (result.userInfo != null)
            {
                m_State.userInfo = result.userInfo;
            }
            
            if (result.accessToken != null)
            {
                m_State.accessToken = result.accessToken;
            }

            return true;
        }

        private bool HandleLoginResult(PlayerIdentityBackendSubsystem.IdentityCallbackArgs result, PlayerIdentityCallback callback)
        {
            if (result.error != null)
            {
                m_State.status = LoginStatus.NotLoggedIn;
                m_State.error = result.error;
                ScheduleCallback(SendErrorEvent, callback);
                
                return false;
            }

            m_State.error = null;

            if (result.userInfo != null)
            {
                m_State.userInfo = result.userInfo;
            }
            
            if (result.accessToken != null)
            {
                m_State.accessToken = result.accessToken;
            }

            if (m_State.userInfo == null)
            {
                m_State.status = LoginStatus.NotLoggedIn;
            }
            else
            {
                m_State.status = m_State.userInfo.isAnonymous ? LoginStatus.AnonymouslyLoggedIn : LoginStatus.LoggedIn;
            }

            return true;
        }

        private void AutomaticallyVerifyEmail()
        {
            if (PlayerIdentityGeneralSettings.Instance.Manager.automaticallyVerifyEmail)
            {
                VerifyEmail(this.userInfo.email);
            }
        }

        private void ScheduleLoginEvent(PlayerIdentityCallback immediateCallback)
        {
            if (m_State.userInfo.isAnonymous)
            {
                ScheduleCallback(SendAnonymousLoginEvent, immediateCallback);
            }
            else
            {
                ScheduleCallback(SendLoginEvent, immediateCallback);
            }
        }

        private void SendAnonymousLoginEvent()
        {

            foreach (var callback in m_Callbacks)
            {
                callback.OnAnonymousLogin(this);
            }

            onAnonymousLogin?.Invoke();
        }
        
        private void SendLoginEvent()
        {
            foreach (var callback in m_Callbacks)
            {
                callback.OnLogin(this);
            }

            onLogin?.Invoke();
        }

        private void SendLogoutEvent()
        {
            foreach (var callback in m_Callbacks)
            {
                callback.OnLogout(this);
            }

            onLogout?.Invoke();
        }

        private void SendAccessTokenChangedEvent()
        {
            foreach (var callback in m_Callbacks)
            {
                callback.OnAccessTokenChanged(this);
            }

            onAccessTokenChanged?.Invoke();
        }

        private void SendErrorEvent()
        {
            foreach (var callback in m_Callbacks)
            {
                callback.OnError(this, this.error);
            }

            onError?.Invoke();
        }
        
        private void SendLinkEvent()
        {
            foreach (var callback in m_Callbacks)
            {
                callback.OnLink(this);
            }

            onLink?.Invoke();
        }
        
        private void SendUnlinkEvent()
        {
            foreach (var callback in m_Callbacks)
            {
                callback.OnUnlink(this);
            }

            onUnlink?.Invoke();
        }
        
        private void SendResetPasswordEvent()
        {
            foreach (var callback in m_Callbacks)
            {
                callback.OnResetPassword(this);
            }

            onResetPassword?.Invoke();
        }

        private void SendCreateCredentialEvent()
        {
            foreach (var callback in m_Callbacks)
            {
                callback.OnCreateCredential(this);
            }
            if (onCreateCredential != null)
            {
                onCreateCredential.Invoke();
            }
        }

        private void SendChangePasswordEvent()
        {
            foreach (var callback in m_Callbacks)
            {
                callback.OnChangePassword(this);
            }
            if (onChangePassword != null)
            {
                onChangePassword.Invoke();
            }
        }

        private void SendGetUserEvent()
        {
            foreach (var callback in m_Callbacks)
            {
                callback.OnGetUser(this);
            }

            onGetUser?.Invoke();
        }
        
        private void SendUpdateUserEvent()
        {
            foreach (var callback in m_Callbacks)
            {
                callback.OnUpdateUser(this);
            }
            if (onUpdateUser != null)
            {
                onUpdateUser.Invoke();
            }
        }

        private void SendVerifyEmailEvent()
        {
            foreach (var callback in m_Callbacks)
            {
                callback.OnVerifyEmail(this);
            }

            onVerifyEmail?.Invoke();
        }

        private void ScheduleCallback(Callback callback, PlayerIdentityCallback immediateCallback)
        {
            if (callback != null)
            {
                m_EventBacklog.Enqueue(callback);
            }

            if (immediateCallback != null)
            {
                m_EventBacklog.Enqueue(() =>
                {
                    immediateCallback.Invoke(this);
                });
            }
        }

        private void Update()
        {
            m_Backend?.Update();

            if (m_LoginProviders != null)
            {
                foreach (var loginProvider in m_LoginProviders)
                {
                    loginProvider.Update();
                }
            }

            Callback callback;
            while (m_EventBacklog.TryDequeue(out callback))
            {
                callback.Invoke();
            }

            if (NeedRefreshAccessTokenAync()&&(m_State.status!=LoginStatus.LoginInProgress))
            {
                RefreshAccessToken();
            }
        }
        
        internal class JWTStandardClaims
        {
            public long exp;
        }
        public bool NeedRefreshAccessTokenAync()
        {
            if (!string.IsNullOrEmpty(this.accessToken))
            {
                JWTStandardClaims claims = UnityPlayerIdentityUtility.DecodeJWT<JWTStandardClaims>(m_State.accessToken);
                DateTime expireDateTime = ConvertFromUnixEpochSeconds((claims.exp * 1000).ToString());
                if (expireDateTime - DateTime.UtcNow < TimeSpan.FromMinutes(5))
                {
                    return true;
                }
            }
            
            return false;

        }
        
        public static DateTime ConvertFromUnixEpochSeconds(string milliseconds)
        {
            return new DateTime(Convert.ToInt64(milliseconds) * 10000L + _epochStart.Ticks, DateTimeKind.Utc);
        }
        
    }
}

