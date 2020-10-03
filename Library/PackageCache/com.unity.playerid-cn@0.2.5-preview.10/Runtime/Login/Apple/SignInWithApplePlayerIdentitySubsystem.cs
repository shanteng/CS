using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.Scripting;

namespace UnityEngine.PlayerIdentity.Apple
{
    [Preserve]
    public class SignInWithApplePlayerIdentitySubsystem : PlayerIdentityLoginSubsystem
    {
        public const string k_ProviderId = "apple.com";
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void RegisterDescriptor()
        {
            PlayerIdentityLoginSubsystemDescriptor.RegisterDescriptor(
                AppleLoader.k_SubsystemId,
                typeof(SignInWithApplePlayerIdentitySubsystem), 
                new RuntimePlatform[]
                {
                    RuntimePlatform.OSXEditor, 
                    RuntimePlatform.IPhonePlayer, 
                    RuntimePlatform.Android, 
                    RuntimePlatform.WindowsEditor
                }, 
                RuntimePlatform.IPhonePlayer,
                "Sign in with Apple",
                k_ProviderId);
        }
        
        private GameObject m_ProviderButton = null;

        public override GameObject GetButton()
        {
            if (m_ProviderButton == null)
            {
                m_ProviderButton = Resources.Load("AppleLoginBtn") as GameObject;
            }

            return m_ProviderButton;
        }

#if UNITY_ANDROID
        internal class SignInWithAppleCallbacks : AndroidJavaProxy
        {
            public SignInWithAppleCallbacks()
                : base("com.unity3d.playeridentity.SignInWithAppleCallbacks")
            {
            }

            void loginCallback(AndroidJavaObject result)
            {
                var appleIdSubsystem = new List<SignInWithApplePlayerIdentitySubsystem>();
                SubsystemManager.GetInstances(appleIdSubsystem);

                if (appleIdSubsystem.Count > 0 && appleIdSubsystem[0].m_LoginCompleted != null)
                {
                    var args = new IdentityLoginCallbackArgs();
                    var code = result.Get<string>("code");
                    var displayName = result.Get<string>("displayName");
                    var email = result.Get<string>("email");
                    var error = result.Get<string>("error");
                    if (error != null)
                    {
                        args.error = new Error();
                        args.error.message = error;
                    }
                    else
                    {
                        args.userInfo = new UserInfo();
                        args.userInfo.displayName = displayName;
                        args.userInfo.email = email;

                        args.externalToken = new ExternalToken();
                        args.externalToken.authCode = code;
                        args.externalToken.idProvider = k_ProviderId;

                        var settings = AppleLoader.GetSettings();
                        args.externalToken.clientId = settings?.m_ServiceId;
                        args.externalToken.redirectUri = settings?.m_CallbackEndpoint;
                    }

                    appleIdSubsystem[0].m_LoginCompleted(args);
                    appleIdSubsystem[0].m_LoginCompleted = null;
                }
            }
        }

        public override void OnApplicationPause(bool pauseStatus)
        {
            if (m_LoginCompleted != null && pauseStatus == false)
            {
                // Application just returned from tab.
                // Notes on the order when there is a successful login.
                // https://developer.android.com/guide/components/activities/activity-lifecycle
                // When a successful login happens, it triggers in onCreate(), and clears out the m_LoginCompleted field.
                
                var args = new IdentityLoginCallbackArgs();
                args.error = new Error();
                args.error.errorClass = ErrorClass.ActionCancelled;

                var appleIdSubsystem = new List<SignInWithApplePlayerIdentitySubsystem>();
                SubsystemManager.GetInstances(appleIdSubsystem);

                appleIdSubsystem[0].m_LoginCompleted(args);
                appleIdSubsystem[0].m_LoginCompleted = null;
                
                m_LoginCompleted = null;
            }
        }

#endif
        bool m_Running = false;
        Callback m_LoginCompleted;
        Callback m_CredentialState;

        public override bool running
        {
            get { return m_Running; }
        }

        public override void Start()
        {
            if (m_Running)
                return;
            m_Running = true;
#if !UNITY_EDITOR
            var settings = AppleLoaderSettings.s_RuntimeInstance;
            if (settings == null)
            {
                Utils.Logger.Error("SignInWithApplePlayerIdentitySubsystem settings not setup");
                return;
            }
#endif
        }

        public override void Stop()
        {
            if (!m_Running)
                return;
            m_Running = false;
        }

        protected override void OnDestroy()
        {
            
        }

        public override void Login(object loginArgs, Callback callback)
        {
            if (m_LoginCompleted != null)
                throw new InvalidOperationException("Login called while another login is in progress");
            m_LoginCompleted = callback;
#if UNITY_EDITOR
            
            var args = new IdentityLoginCallbackArgs();
            args.error = new Error();
            args.error.errorClass = ErrorClass.UserError;
            args.error.message = "Sign in with Apple is not supported in Editor";

            m_LoginCompleted(args);
            m_LoginCompleted = null;
            
#elif UNITY_IOS || UNITY_TVOS
            IntPtr cback = IntPtr.Zero;
            if (callback != null)
            {
                LoginCompletedSIWA d = LoginCompletedCallback;
                cback = Marshal.GetFunctionPointerForDelegate(d);
            }
            UnityAppleIDPlayerIdentity_Login(cback);
#elif UNITY_ANDROID
            AndroidJavaObject activity;
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                activity = player.GetStatic<AndroidJavaObject>("currentActivity");

            using (var klass = new AndroidJavaClass("com.unity3d.playeridentity.SignInWithApple"))
            {
                var settings = AppleLoaderSettings.s_RuntimeInstance;
                klass.CallStatic("login", activity, settings.m_ServiceId, settings.m_ApplicationDeepLink, settings.m_CallbackEndpoint, new SignInWithAppleCallbacks());
            }
#endif
        }

        private delegate void LoginCompletedSIWA(int result, SignInWithAppleUserInfoiOS s1);

        public struct SignInWithAppleUserInfoiOS
        {
            public string userId;
            public string email;
            public string displayName;

            public string idToken;
            public string error;
        }

        [MonoPInvokeCallback(typeof(LoginCompletedSIWA))]
        private static void LoginCompletedCallback(int result, [MarshalAs(UnmanagedType.Struct)]SignInWithAppleUserInfoiOS info)
        {
            var appleIdSubsystem = new List<SignInWithApplePlayerIdentitySubsystem>();
            SubsystemManager.GetInstances(appleIdSubsystem);
            if (appleIdSubsystem.Count > 0 && appleIdSubsystem[0].m_LoginCompleted != null)
            {
                var args = new IdentityLoginCallbackArgs();
                if (result != 0)
                {
                    args.externalToken = new ExternalToken
                    {
                        idToken = info.idToken,
                        idProvider = k_ProviderId,
                        clientId = Application.identifier,
                        redirectUri = "http://localhost"
                    };

                    args.userInfo = new UserInfo
                    {
                        displayName = info.displayName,
                        email = info.email,
                        userId = info.userId,
                    };
                }
                else
                {
                    args.error = new Error { message = info.error };
                }

                appleIdSubsystem[0].m_LoginCompleted(args);
                appleIdSubsystem[0].m_LoginCompleted = null;
            }
        }

        private delegate void GetCredentialStateCompletedSIWA(UserCredentialState state);

        [MonoPInvokeCallback(typeof(GetCredentialStateCompletedSIWA))]
        private static void GetCredentialStateCallback([MarshalAs(UnmanagedType.SysInt)]UserCredentialState state)
        {
            var appleIdSubsystem = new List<SignInWithApplePlayerIdentitySubsystem>();
            SubsystemManager.GetInstances(appleIdSubsystem);
            if (appleIdSubsystem.Count > 0 && appleIdSubsystem[0].m_LoginCompleted != null)
            {
                var args = new IdentityLoginCallbackArgs
                {
                    credentialState = state
                };

                appleIdSubsystem[0].m_CredentialState(args);
                appleIdSubsystem[0].m_CredentialState = null;
            }
        }

#if UNITY_IOS || UNITY_TVOS
        [DllImport("__Internal")]
        private static extern void UnityAppleIDPlayerIdentity_Login(IntPtr callback);

        [DllImport("__Internal")]
        private static extern void UnityAppleIDPlayerIdentity_GetCredentialState(string userID, IntPtr callback);
#endif
    }
}
