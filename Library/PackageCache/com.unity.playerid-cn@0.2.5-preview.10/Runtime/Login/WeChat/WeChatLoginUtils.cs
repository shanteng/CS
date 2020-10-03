using UnityEngine.Networking;
using UnityEngine.PlayerIdentity;
using UnityEngine.Serialization;

namespace UnityEngine.PlayerIdentity.WeChat
{
    public static class WeChatLoginUtils
    {
        
        [System.Serializable]
        class WechatTokenResponse
        {
            [FormerlySerializedAs("access_token")] public string accessToken=default;
            public string openid=default;
            public string errmsg=default;
            public int errcode=default;
        }
        
        public static void LoginByCodeOrAccessToken(string code, string clientId, string redirectUrl, PlayerIdentityLoginSubsystem.Callback weChatLogin)
        {
            
            var args = new PlayerIdentityLoginSubsystem.IdentityLoginCallbackArgs();
            
            args.externalToken = new UnityEngine.PlayerIdentity.ExternalToken
            {
                idProvider = "wechat.com"
            };
            
            // use unity identity service
            if (string.IsNullOrEmpty(redirectUrl))
            {
                args.externalToken.authCode = code;
                weChatLogin(args);
            }
            else
            {
                var webRequest = UnityWebRequest.Get($"{redirectUrl}?code={code}&&appid={clientId}");
                var asyncRequest = webRequest.SendWebRequest();
                asyncRequest.completed += operation =>
                {
                    if (webRequest.isNetworkError)
                    {
                        Debug.LogError("Error Get Access Token:" + webRequest.error);
                        args.error = new Error
                        {
                            message = webRequest.error,
                            errorClass = ErrorClass.NetworkError,
                            type = "WECHAT_CUSTOM_CALLBACK_ENDPOINT_SERVER",
                        };
                        weChatLogin(args);
                    }
                    else
                    {
                        Debug.Log("Get Access Token Response:\n" + webRequest.downloadHandler.text);
                        var response = JsonUtility.FromJson<WechatTokenResponse>(webRequest.downloadHandler.text);
                        if (!string.IsNullOrEmpty(response.errmsg))
                        {
                            args.error = new Error
                            {
                                message = response.errmsg,
                                errorClass = ErrorClass.Unknown,
                                type = "WECHAT_CUSTOM_CALLBACK_ENDPOINT_SERVER",
                            };
                        }
                        else
                        {
                            args.externalToken = new UnityEngine.PlayerIdentity.ExternalToken
                            {
                                accessToken = response.accessToken,
                                openid = response.openid,
                                idProvider = "wechat.com",
                                clientId = clientId,
                                redirectUri = "http://localhost" //todo copy from sign with apple
                            };
                            weChatLogin(args);
                        }
                    }
                };
            }
        }
    }
}