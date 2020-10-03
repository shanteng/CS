namespace UnityEngine.PlayerIdentity
{
    public class Error
    {
        public ErrorClass errorClass;

        public string type;
        public string message;
    }


    public enum ErrorClass
    {
        Unknown,
        NetworkError,
        ApiError,
        UserError,
        ActionCancelled,
    }

    public class UserInfo
    {
        public string userId;
        public string email;
        public string displayName;
        public bool emailVerified;
        public string signInProviderId;
        public string externalId;
        public bool isAnonymous;
        public string photoUrl;
        public bool disabled;
        
        // externalIds is the current set of Identity Providers that this user's account is linked to.
        public ExternalIdInfo[] externalIds;
    }
    
    // ExternalIdInfo describes a user's account with an Identity Provider.
    public class ExternalIdInfo
    {
        public string providerId;
        public string externalId;
        public string displayName;
        public string email;
        public string phoneNumber;
    }

    public class ExternalToken
    {
        public string idProvider;
        public string idToken;
        public string accessToken;
        public string redirectUri;
        public string authCode;
        public string clientId;
        public string openid;
    }

    public enum UserCredentialState
    {
        Revoked,
        Authorized,
        NotFound
    }
}
