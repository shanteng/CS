using UnityEngine;
using UnityEngine.PlayerIdentity.Management;

namespace UnityEngine.PlayerIdentity.UnityUserAuth
{
    /// <summary>
    /// Settings to control the UnityLoader behavior.
    /// </summary>
    [PlayerIdentityConfigurationData("Backends/Unity UserAuth", UnityUserAuthLoader.k_SettingsKey)]
    [System.Serializable]
    public class UnityUserAuthLoaderSettings : ScriptableObject
    {
        /// <summary>
        /// Static instance that will hold the runtime asset instance we created in our build process.
        /// </summary>
#if !UNITY_EDITOR
        internal static UnityUserAuthLoaderSettings s_RuntimeInstance = null;
#endif

        [SerializeField, Tooltip("ID Domain ID. Identifier for the bounded user pool.")]
        private string m_idDomainID;
        
        public string IDDomainID
        {
            get { return m_idDomainID;  }
            set { m_idDomainID = value; }
        }

        [SerializeField, Tooltip("OAuth2 Client ID. Identifier needed for authorization.")]
        private string m_oauthClientId;
        
        public string OAuthClientId
        {
            get { return m_oauthClientId;  }
            set { m_oauthClientId = value; }
        }

        [SerializeField, Tooltip("OAuth Scopes.  Permissions for OAuth")]
        private string m_oauthScopes = "openid offline identity.user";
        
        public string OAuthScopes
        {
            get { return m_oauthScopes;  }
            set { m_oauthScopes = value; }
        }
        
        [SerializeField, Tooltip("API Base URL. Base URL for API calls.")]
        private string m_apiBaseUrl = "https://identity-api.unity.cn";

        public string APIBaseUrl
        {
            get { return m_apiBaseUrl;  }
            set { m_apiBaseUrl = value; }
        }

        [SerializeField, Tooltip("Toggle if anonymous users should be auto-created.")]
        private bool m_autoCreateAnonymousUser = true;
        
        public bool AutoCreateAnonymousUser
        {
            get { return m_autoCreateAnonymousUser;  }
            set { m_autoCreateAnonymousUser = value; }
        }
        
        [SerializeField, Tooltip("Toggle if refresh token should be auto-persisted.")]
        private bool m_persistRefreshToken = true;
        
        public bool PersistRefreshToken
        {
            get { return m_persistRefreshToken;  }
            set { m_persistRefreshToken = value; }
        }
        
        [SerializeField, Tooltip("Refresh Token Persist Key. Key for refresh token persistence.")]
        private string m_refreshTokenPersistKey = "UnityPlayerIdentity.refreshToken";

        public string RefreshTokenPersistKey
        {
            get { return m_refreshTokenPersistKey;  }
            set { m_refreshTokenPersistKey = value; }
        }
        
        [SerializeField, Tooltip("Anon Token Persist Key. Key for anonymous user persistence.")]
        private string m_anonymousTokenPersistKey = "UnityPlayerIdentity.anonymousToken";

        public string AnonymousTokenPersistKey
        {
            get { return m_anonymousTokenPersistKey;  }
            set { m_anonymousTokenPersistKey = value; }
        }

        public void Awake()
        {
#if !UNITY_EDITOR
            s_RuntimeInstance = this;
#endif
        }
    }
}
