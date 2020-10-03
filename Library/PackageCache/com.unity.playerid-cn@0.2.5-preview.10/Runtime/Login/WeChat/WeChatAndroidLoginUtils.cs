#if UNITY_ANDROID
using System;
using UnityEngine.PlayerIdentity;

namespace UnityEngine.PlayerIdentity.WeChat
{

    public class WeChatAndroidLoginUtils
    {

        private static string _packageName = "com.unity3d.playerididentity";
        private static string _appId;
        private static string _redirectUrl;
        private const string WxUtilsPackageNameSuffix = ".WxAPIUtils";
        private const string PlayerClassName = "com.unity3d.player.UnityPlayer";

        public static void InitAndroidWxApi()
        {
//            _packageName = Application.identifier;
            _appId = WeChatLoaderSettings.s_RuntimeInstance.m_AppID;
            _redirectUrl = WeChatLoaderSettings.s_RuntimeInstance.m_CallbackEndpoint;
            AndroidJavaObject context;
            using (var playerClass = new AndroidJavaClass(PlayerClassName))
            {
                var activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
                context = activity.Call<AndroidJavaObject>("getApplicationContext");
                using (var kclass =  new AndroidJavaClass(_packageName + WxUtilsPackageNameSuffix))
                {
                    kclass.CallStatic("setContext", context);
                    kclass.CallStatic("setWxAppId", _appId);
                    kclass.CallStatic("createAndRegister");
                }
            }
        }
        
        public static void LoginAndroid(PlayerIdentityLoginSubsystem.Callback callback)
        {
            using (var kclass =  new AndroidJavaClass(_packageName + WxUtilsPackageNameSuffix))
            {
                kclass.CallStatic("getCode", "snsapi_userinfo", new LoginCallBack(callback));
            }
        }
        
        
        public class LoginCallBack : AndroidJavaProxy
        {
            private PlayerIdentityLoginSubsystem.Callback callback;
            
            public LoginCallBack(PlayerIdentityLoginSubsystem.Callback callback) : base(_packageName + WxUtilsPackageNameSuffix + "$GetCodeCallback")
            {
                this.callback = callback;
            }

            void onSuccess(string code)
            {
                WeChatLoginUtils.LoginByCodeOrAccessToken(code, _appId,  _redirectUrl, this.onLoginCompleted);
            }

            void onError(int errorCode, string errorString)
            {
                var args = new PlayerIdentityLoginSubsystem.IdentityLoginCallbackArgs();
                args.error = new Error
                {
                    message = errorString,
                    type = "WECHAT_APP_OAUTH_ERROR"
                };
                onLoginCompleted(args);
            }

            void onLoginCompleted(PlayerIdentityLoginSubsystem.IdentityLoginCallbackArgs args)
            {
                callback(args);
            }
        }
    }


}
#endif