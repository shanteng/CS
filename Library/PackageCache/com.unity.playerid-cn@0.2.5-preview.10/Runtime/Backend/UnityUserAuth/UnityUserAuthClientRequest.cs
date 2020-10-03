using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityPlayerIdentityUtility = UnityEngine.PlayerIdentity.Utils.Utility;

namespace UnityEngine.PlayerIdentity.UnityUserAuth
{
    internal class IdentityClient : HttpClientBase
    {
        private Dictionary<string, string> m_AuthnCommonHeaders = new Dictionary<string, string>();

        public IdentityClient(string baseUrl, string idDomainKey) : base(baseUrl)
        {
            m_AuthnCommonHeaders["Id-Domain-Key"] = idDomainKey;
        }

        public HttpRequest<AuthenticationResponse> SignUp(SignupRequest request)
        {
            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/authentication/sign-up", m_AuthnCommonHeaders, request);
            return new HttpRequest<AuthenticationResponse>(webReq, ParseJsonResponse<AuthenticationResponse>);
        }

        public HttpRequest<AuthenticationResponse> PasswordAuth(PasswordAuthRequest request)
        {
            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/authentication/password", m_AuthnCommonHeaders, request);
            return new HttpRequest<AuthenticationResponse>(webReq, ParseJsonResponse<AuthenticationResponse>);
        }

        public HttpRequest<AuthenticationResponse> SessionTokenAuth(SessionTokenAuthRequest request)
        {
            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/authentication/session-token", m_AuthnCommonHeaders, request);
            return new HttpRequest<AuthenticationResponse>(webReq, ParseJsonResponse<AuthenticationResponse>);
        }

        public HttpRequest<AuthenticationResponse> SignUpAnonymous(AnonymousUserRequest request)
        {
            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/authentication/anonymous", m_AuthnCommonHeaders, request);
            return new HttpRequest<AuthenticationResponse>(webReq, ParseJsonResponse<AuthenticationResponse>);
        }

        public HttpRequest<AuthenticationResponse> CreateCredential(string accessToken, CreateCredentialsRequest request)
        {
            var headers = WithAccessToken(accessToken);

            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/authentication/create-credential", headers, request);
            return new HttpRequest<AuthenticationResponse>(webReq, ParseJsonResponse<AuthenticationResponse>);
        }

        public HttpRequest<AuthenticationResponse> ChangePassword(string accessToken, ChangePasswordRequest request)
        {
            var headers = WithAccessToken(accessToken);

            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/authentication/change-password", headers, request);
            return new HttpRequest<AuthenticationResponse>(webReq, ParseJsonResponse<AuthenticationResponse>);
        }

        public HttpRequest<AuthenticationResponse> ExternalTokenAuth(ExternalTokenAuthRequest request)
        {
            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/authentication/external-token", m_AuthnCommonHeaders, request);
            return new HttpRequest<AuthenticationResponse>(webReq, ParseJsonResponse<AuthenticationResponse>);
        }

        public HttpRequest<AuthenticationResponse> LinkExternalId(string accessToken, ExternalTokenAuthRequest request)
        {
            var headers = WithAccessToken(accessToken);

            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/authentication/link", headers, request);
            return new HttpRequest<AuthenticationResponse>(webReq, ParseJsonResponse<AuthenticationResponse>);
        }

        public HttpRequest<AuthenticationResponse> LinkSmsCode(string accessToken, LinkSmsCodeRequest request)
        {
            var headers = WithAccessToken(accessToken);

            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/authentication/sms-code/link", headers, request);
            return new HttpRequest<AuthenticationResponse>(webReq, ParseJsonResponse<AuthenticationResponse>);
        }
        
        public HttpRequest<AuthenticationResponse> UnlinkExternalId(string accessToken, UnlinkExternalIdRequest request)
        {
            var headers = WithAccessToken(accessToken);

            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/authentication/unlink", headers, request);
            return new HttpRequest<AuthenticationResponse>(webReq, ParseJsonResponse<AuthenticationResponse>);
        }

        public HttpRequest<ResetPasswordResponse> ResetPassword(ResetPasswordRequest request)
        {
            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/authentication/reset-password", m_AuthnCommonHeaders, request);
            return new HttpRequest<ResetPasswordResponse>(webReq, ParseJsonResponse<ResetPasswordResponse>);
        }
        
        public HttpRequest<UserResponse> GetUser(string accessToken, GetUserRequest request)
        {
            var headers = WithAccessToken(accessToken);
            
            var webReq = RequestJson(UnityWebRequest.kHttpVerbGET, "/users/" + request.id, headers, null);
            return new HttpRequest<UserResponse>(webReq, ParseJsonResponse<UserResponse>);
        }
        
        public HttpRequest<UserResponse> UpdateUser(string accessToken, UpdateUserRequest request)
        {
            var headers = WithAccessToken(accessToken);
            
            var webReq = RequestJson(UnityWebRequest.kHttpVerbPUT, "/users/" + request.id, headers, request);
            return new HttpRequest<UserResponse>(webReq, ParseJsonResponse<UserResponse>);
        }

        public HttpRequest<VerifyEmailResponse> VerifyEmail(string accessToken, VerifyEmailRequest request)
        {
            var headers = WithAccessToken(accessToken);

            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/authentication/verify-email", headers, request);
            return new HttpRequest<VerifyEmailResponse>(webReq, ParseJsonResponse<VerifyEmailResponse>);
        }
        
        public HttpRequest<AuthenticationResponse> SmsCodeAuth(SmsCodeAuthRequest request)
        {
            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/authentication/sms-code/confirm", m_AuthnCommonHeaders, request);
            return new HttpRequest<AuthenticationResponse>(webReq, ParseJsonResponse<AuthenticationResponse>);
        }    
        
        public HttpRequest<CreateSmsCodeResponse> RequestSmsCode(CreateCodeRequest createCodeRequest)
        {
            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/authentication/sms-code", m_AuthnCommonHeaders, createCodeRequest);
            return new HttpRequest<CreateSmsCodeResponse>(webReq, ParseJsonResponse<CreateSmsCodeResponse>);
        }

        public HttpRequest<AuthorizeResponse> Authorize(AuthorizeRequest request)
        {
            var parameters = UnityPlayerIdentityUtility.UrlEncode(new Dictionary<string, string>
            {
                { "response_type", request.response_type },
                { "client_id", request.client_id },
                { "redirect_uri", request.redirect_uri },
                { "scope", request.scope },
                { "state", request.state },
                { "nonce", request.nonce },
                { "prompt", request.prompt },
                { "id_token", request.id_token },
                { "code_challenge", request.code_challenge },
                { "code_challenge_method", request.code_challenge_method },
            });

            var webReq = RequestForm(UnityWebRequest.kHttpVerbPOST, "/oauth2/auth", null, parameters);
            return new HttpRequest<AuthorizeResponse>(webReq, ParseOAuthRedirectResponse);
        }

        public HttpRequest<TokenResponse> GetToken(TokenRequest request)
        {
            var formBody = UnityPlayerIdentityUtility.UrlEncode(new Dictionary<string, string>
            {
                { "grant_type", request.grant_type },
                { "code", request.code },
                { "redirect_uri", request.redirect_uri },
                { "scope", request.scope },
                { "client_id", request.client_id },
                { "client_secret", request.client_secret },
                { "refresh_token", request.refresh_token },
                { "code_verifier", request.code_verifier },
            });

            var webReq = RequestForm(UnityWebRequest.kHttpVerbPOST, "/oauth2/token", null, formBody);
            return new HttpRequest<TokenResponse>(webReq, ParseJsonResponse<TokenResponse>);
        }

        private HttpRequest<AuthorizeResponse>.ParsedResult ParseOAuthRedirectResponse(UnityWebRequest request, string text)
        {
            if (request.responseCode == 302)
            {
                var headers = request.GetResponseHeaders();
                if (!headers.ContainsKey("location"))
                {
                    return new HttpRequest<AuthorizeResponse>.ParsedResult
                    {
                        error = new Error
                        {
                            errorClass = ErrorClass.ApiError,
                            type = "UNEXPECTED_RESPONSE",
                            message = request.responseCode.ToString() + ": location header is not returned",
                        }
                    };
                }

                var location = headers["location"];
                Utils.Logger.Info("Location: " + location);
                
                int index = location.IndexOf('?');
                if (index == -1)
                {
                    return new HttpRequest<AuthorizeResponse>.ParsedResult
                    {
                        error = new Error
                        {
                            errorClass = ErrorClass.ApiError,
                            type = "UNEXPECTED_RESPONSE",
                            message = request.responseCode.ToString() + ": location header does not contain parameters",
                        }
                    };
                }

                var qs = location.Substring(index + 1);
                var queryParams = UnityPlayerIdentityUtility.ParseQueryString(qs);
                
                var response = new AuthorizeResponse
                {
                    state = queryParams["state"],
                    code = queryParams["code"],
                    scope = queryParams["scope"],
                };
                Error error = null;

                if (queryParams["error"] != null)
                {
                    response.error = queryParams["error"];
                    response.error_description = queryParams["error_description"];

                    error = response.GetError();
                }
                return new HttpRequest<AuthorizeResponse>.ParsedResult
                {
                    error = error,
                    result = response
                };
            }
            else
            {
                return new HttpRequest<AuthorizeResponse>.ParsedResult
                {
                    error = new Error
                    {
                        errorClass = ErrorClass.ApiError,
                        type = "UNEXPECTED_RESPONSE",
                        message = request.responseCode.ToString() + ": unexpected response code for OAuth redirect response",
                    }
                };
            }
        }

        private Dictionary<string, string> WithAccessToken(string accessToken)
        {
            var headers = new Dictionary<string, string>(m_AuthnCommonHeaders) { ["Authorization"] = "Bearer " + accessToken };

            return headers;
        }
    }
}
