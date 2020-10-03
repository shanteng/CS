using UnityEngine;
using System.Collections.Generic;
using System;

namespace UnityEngine.PlayerIdentity.UnityUserAuth
{
    internal interface IApiResponseBase
    {
        Error GetError();
    }

    internal class ResponseBase : ErrorResponse, IApiResponseBase
    {
        public Error GetError()
        {
            return new Error
            {
                errorClass = ErrorClass.UserError,
                type = error,
                message = message,
            };
        }
    }

    internal class ErrorResponse
    {
        public string error;
        public string message;
        public object[] details;
    }

    internal class PasswordAuthRequest
    {
        public string email;
        public string password;
    }

    internal class ExternalTokenAuthRequest
    {
        public string idProvider;
        public string idToken;
        public string accessToken;
        public string redirectUri;
        public string authCode;
        public string clientId;
        public string openid;
    }

    internal class SessionTokenAuthRequest
    {
        public string sessionToken;
    }

    internal class SignupRequest
    {
        public string email;
        public string password;
        public string displayName;
    }

    internal class CreateCredentialsRequest
    {
        public string email;
        public string password;
    }

    internal class ChangePasswordRequest
    {
        public string password;
        public string newPassword;
    }

    internal class UpdateUserRequest
    {
        public string id;
        public string displayName;
        public string photoUrl;
    }

    internal class AnonymousUserRequest
    {
    }
    
    internal class CreateSmsCodeResponse : ResponseBase
    {
        public string verificationId;
    }
    
        
    internal class SmsCodeAuthRequest
    {
        public string code;
        public string verificationId;
    }

    internal class CreateCodeRequest
    {
        public string phoneNumber;
    }

    internal class LinkExternalIdRequest
    {
        public string idProvider;
        public string idToken;
        public string accessToken;
        public string redirectUri;
    }

    internal class UnlinkExternalIdRequest
    {
        public string[] idProviders;
    }

    internal class LinkSmsCodeRequest
    {
        public string userId;
        public string code;
        public string verificationId;
    }
    
    internal class CreateCredentialRequest
    {
        public string email;
        public string password;
    }

    internal class ResetPasswordRequest
    {
        public string email;
    }

    internal class ResetPasswordResponse : ResponseBase
    {
    }
    
    internal class GetUserRequest
    {
        public string id;
    }
    
    internal class VerifyEmailRequest
    {
        public string email;
    }
    
    internal class VerifyEmailResponse : ResponseBase
    {}

    internal class ExternalIdResponse
    {
        public string providerId;
        public string externalId;
        public string displayName;
        public string email;
        public string phoneNumber;
    }

    internal class AuthenticationResponse : ResponseBase
    {
        public string userId;
        public string email;
        public string idToken;
        public string sessionToken;
        public int expiresIn;
        public bool needConfirmation;
        public User user;
        public string rawUserInfo;
    }

    internal class UserResponse : ResponseBase
    {
        public string id;
        public string idDomain;
        public bool disabled;
        public string displayName;
        public string email;
        public bool emailVerified;
        public Dictionary<string, string> metadata;
        public string photoUrl;
        public Dictionary<string, string> customClaims;
        public ExternalId[] externalIds;
    }
    
    internal class User
    {
        public string id;
        public string idDomain;
        public bool disabled;
        public string displayName;
        public string email;
        public bool emailVerified;
        public Dictionary<string, string> metadata;
        public string photoUrl;
        public Dictionary<string, string> customClaims;
        public ExternalId[] externalIds;
    }

    internal class ExternalId
    {
        public string providerId;
        public string externalId;
        public string displayName;
        public string email;
        public string phoneNumber;
    }

    internal class TokenRequest
    {
        public string grant_type;
        public string code;
        public string redirect_uri;
        public string scope;
        public string client_id;
        public string client_secret;
        public string refresh_token;
        public string code_verifier;
    }

    internal class JWTStandardClaims
    {
        public string iss;
        public string sub;
        public string[] aud;
        public long exp;
        public long nbf;
        public long iat;
        public string jti;
    }

    internal class IDToken : JWTStandardClaims
    {
        public string user_id;
        public string email;
        public bool email_verified;
        public string name;
        public string picture;
        public string sign_in_provider;
    }

    internal class AuthorizeRequest
    {
        public string response_type;
        public string client_id;
        public string redirect_uri;
        public string scope;
        public string state;
        public string nonce;
        public string prompt;
        public string id_token;
        public string code_challenge;
        public string code_challenge_method;
    }

    internal class OAuthResponseBase : IApiResponseBase
    {
        public string error;
        public string error_description;

        public Error GetError()
        {
            return new Error
            {
                errorClass = ErrorClass.UserError,
                type = error,
                message = error_description,
            };
        }
    }

    internal class AuthorizeResponse : OAuthResponseBase
    {
        public string state;
        public string code;
        public string scope;
    }

    internal class TokenResponse : OAuthResponseBase
    {
        public string access_token;
        public string id_token;
        public string refresh_token;
        public string scope;
        public string token_type;
        public int expires_in;
    }
}
