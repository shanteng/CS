using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine.Networking;
using UnityEngine.PlayerIdentity;
using UnityEngine.PlayerIdentity.UnityUserAuth;

using UnityPlayerIdentityUtility = UnityEngine.PlayerIdentity.Utils.Utility;

namespace UnityEditor.PlayerIdentity.UnityUserAuth 
{
    /// <summary>
    /// UnityDevIdClient calls the Unity Developer ID System
    /// </summary>
    internal class UnityDevIdClient : HttpClientBase {
        private const string k_ClientId = "unity_userauth";
        private const string k_UnityApiEndpoint = "https://api.unity.cn";

        public UnityDevIdClient() : base(k_UnityApiEndpoint)
        {
        }
        
        // GetUnityProjectInfo gets project data
        public HttpRequest<GetProjectResponse> GetUnityProjectInfo(string accessToken, string projectId) {
            var headers = new Dictionary<string, string> { ["Authorization"] = "Bearer " + accessToken };

            var webReq = RequestJson(UnityWebRequest.kHttpVerbGET, "/v1/core/api/projects/" + projectId, headers, null);
            return new HttpRequest<GetProjectResponse>(webReq, ParseJsonResponse<GetProjectResponse>);
        }

        // GetDevAuthCode gets an authorization code
        public HttpRequest<AuthCodeResponse> GetDevAuthCode(string accessToken, string orgId, string codeVerifier) {
            // Allow user JWT complete access to their own organization
            var queryParams = new Dictionary<string, string>
            {
                ["client_id"] = k_ClientId,
                ["response_type"] = "code",
                ["format"] = "json",
                ["access_token"] = accessToken,
                ["prompt"] = "none",
                ["scope"] = "roles/userauth.admin orgs/" + orgId + " upid/none",
                ["code_challenge_method"] = "s256"
            };

            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                var codeChallenge = UnityPlayerIdentityUtility.Base64EncodeUrlSafe(bytes);
                
                queryParams["code_challenge"] = codeChallenge;
            }
            
            var data = UnityPlayerIdentityUtility.UrlEncode(queryParams);
            
            var webReq = RequestForm(UnityWebRequest.kHttpVerbPOST, "/v1/oauth2/authorize", null, data);
            return new HttpRequest<AuthCodeResponse>(webReq, ParseJsonResponse<AuthCodeResponse>);
        }

        // GetDevToken gets an access token
        public HttpRequest<TokenResponse> GetDevToken(string authCode, string codeVerifier) {
            var queryParams = new Dictionary<string, string>
            {
                ["client_id"] = k_ClientId,
                ["grant_type"] = "authorization_code",
                ["code"] = authCode,
                ["code_verifier"] = codeVerifier,
                ["redirect_uri"] = "launcher://playerid"
            };

            var data = UnityPlayerIdentityUtility.UrlEncode(queryParams);
            
            var webReq = RequestForm(UnityWebRequest.kHttpVerbPOST, "/v1/oauth2/token", null, data);
            return new HttpRequest<TokenResponse>(webReq, ParseJsonResponse<TokenResponse>);
        }

        internal class ErrorResponse
        {
            public string code;
            public string message;
            public ErrorDetail[] details;
        }
        
        internal class ErrorDetail
        {
            public string field;
            public string reason;
        }

        internal class ResponseBase : ErrorResponse, IApiResponseBase
        {
            public UnityEngine.PlayerIdentity.Error GetError()
            {
                var longMessage = message;
                if (details != null && details.Length > 0)
                {
                    longMessage = ": " + string.Join(", ", details.Select(x => x.field + " " + x.reason));
                }
                return new UnityEngine.PlayerIdentity.Error
                {
                    errorClass = ErrorClass.UserError,
                    type = code,
                    message = longMessage,
                };
            }
        }

        internal class GetProjectResponse : ResponseBase {
            public string id;
            public string guid;
            public string org_foreign_key;
        }

        internal class AuthCodeResponse : ResponseBase{
            public new string code;
        } 

        internal class TokenResponse : ResponseBase{
            public string access_token;
            public string token_type;
            public string expires_in;
        }
    }
}
