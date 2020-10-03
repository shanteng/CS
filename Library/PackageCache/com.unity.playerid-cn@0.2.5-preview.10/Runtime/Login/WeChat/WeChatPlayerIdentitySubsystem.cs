using System;
using UnityEngine.PlayerIdentity;
using UnityEngine.Scripting;

namespace UnityEngine.PlayerIdentity.WeChat
{
    [Preserve]
    public class WeChatPlayerIdentitySubsystem : PlayerIdentityLoginSubsystem
    {
        public static readonly string SubsystemId = "SignInWeChat-Identity-Subsystem";
        public const string k_ProviderId = "wechat.com";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void RegisterDescriptor()
        {
            PlayerIdentityLoginSubsystemDescriptor.RegisterDescriptor(
                WeChatLoader.k_SubsystemId,
                typeof(WeChatPlayerIdentitySubsystem), 
                new RuntimePlatform[]
                {
                    RuntimePlatform.IPhonePlayer, 
                    RuntimePlatform.Android
//                    RuntimePlatform.OSXEditor,
//                    RuntimePlatform.WindowsEditor
                }, 
                RuntimePlatform.IPhonePlayer,
                "微信",
                k_ProviderId);
        }
        
        public override GameObject GetButton()
        {
            if (_providerButton == null)
            {
                _providerButton = Resources.Load("WeChatLoginBtn") as GameObject;
            }

            return _providerButton;
        }
        
        bool _running = false;
        Callback _loginCompleted;

        public override bool running { get { return _running; } }

        public override void Start()
        {
            if (_running)
                return;
            _running = true;
#if !UNITY_EDITOR
            var settings = WeChatLoaderSettings.s_RuntimeInstance;
            if (settings == null)
            {
                Debug.LogError("WeChatPlayerIdentitySubsystem settings not setup");
                return;
            }
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
            WeChatAndroidLoginUtils.InitAndroidWxApi();
#endif
#if UNITY_IOS
            WeChatIosLoginUtils.InitWxAPI();
#endif
        }

        public override void Stop()
        {
            if (!_running)
                return;
            _running = false;
        }
        
        protected override void OnDestroy()
        {
            // silently destroy

        }

        public override void Login(object loginArgs, Callback callback)
        {
            if (_loginCompleted != null)
            {
                throw new InvalidOperationException("Login called while another login is in progress");
            }
            _loginCompleted = callback;
#if UNITY_IOS || UNITY_TVOS // todo
            WeChatIosLoginUtils.login(OnLoginCompleted);
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
            WeChatAndroidLoginUtils.LoginAndroid(OnLoginCompleted);
#endif
        }
        
        private GameObject _providerButton = null;


        
        

        public void OnLoginCompleted(IdentityLoginCallbackArgs args)
        {
            _loginCompleted.Invoke(args);
            _loginCompleted = null;
        }
    }
}