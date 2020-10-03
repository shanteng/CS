#if UNITY_IOS || UNITY_TVOS
using AOT;
using System.Runtime.InteropServices;

namespace UnityEngine.PlayerIdentity.WeChat
{

    public class WeChatIosLoginUtils
    {
        private static PlayerIdentityLoginSubsystem.Callback _callback;
        private static string _redirectUrl;
        private static string _appId;

        #region API

        public static void login(PlayerIdentityLoginSubsystem.Callback callback)
        {
            _callback = callback;
            loginByWeChat(onGetCode:OnGetCode);
        }
        
        public static void InitWxAPI()
        {
            _redirectUrl = WeChatLoaderSettings.s_RuntimeInstance.m_CallbackEndpoint;
            _appId = WeChatLoaderSettings.s_RuntimeInstance.m_AppID;
            registerWxAPI();
        }

        #endregion
        
        public struct CodeInfo
        {
            public string code;
            public string errMsg;
            public int errCode;
        }
        private delegate void OnGetCodeSIWA(CodeInfo codeInfo);

        [MonoPInvokeCallback(typeof(OnGetCodeSIWA))]
        private static void OnGetCode([MarshalAs(UnmanagedType.Struct)]CodeInfo codeInfo)
        {
            if (_callback == null) {
                Debug.LogError("callback not set for wechat login....");
                return;
            }
            if (string.IsNullOrEmpty(codeInfo.errMsg))
            {
                WeChatLoginUtils.LoginByCodeOrAccessToken(codeInfo.code, _appId, _redirectUrl, args => { _callback(args); });
            }
            else
            {
                var args = new PlayerIdentityLoginSubsystem.IdentityLoginCallbackArgs();
                args.error = new Error
                {
                    message = codeInfo.errMsg,
                    type = "WECHAT_APP_OAUTH_ERROR"
                };
                _callback(args);
            }
        }
        


        [DllImport("__Internal")]
        private static extern void loginByWeChat(OnGetCodeSIWA onGetCode);
        [DllImport("__Internal")]
        private static extern void registerWxAPI();

    }

}
#endif