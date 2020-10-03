using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.PlayerIdentity.UnityUserAuth;

using UnityPlayerIdentityUtility = UnityEngine.PlayerIdentity.Utils.Utility;

namespace UnityEditor.PlayerIdentity.UnityUserAuth 
{
    /// <summary>
    /// UnityUserAuthAdminClient calls the authorization endpoints needed for admins
    /// </summary>
    internal class UnityUserAuthAdminClient : HttpClientBase
    {
        private string m_AccessToken;
        private Dictionary<string, string> m_Headers;

        public UnityUserAuthAdminClient(string baseUrl, string accessToken) : base(baseUrl)
        {
            m_AccessToken = accessToken;
            m_Headers = new Dictionary<string, string> { ["Authorization"] = "Bearer " + accessToken };
        }

        internal class CreateIDDomainRequest
        {
            public string name;
            public string orgId;
            public string backendType;
        }

        internal class IDDomain : ResponseBase
        {
            public string id;
            public string name;
            public string orgId;
            public string backendType;
        }

        internal class ListIDDomainResponse : ResponseBase
        {
            public int count;
            public IDDomain[] data;
        }

        // CreateIdDomain creates a new ID domain for the organization
        public HttpRequest<IDDomain> CreateIdDomain(CreateIDDomainRequest request)
        {
            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/iddomains", m_Headers, request);
            return new HttpRequest<IDDomain>(webReq, ParseJsonResponse<IDDomain>);
        }

        // ListIDDomains lists all available ID domains owned by the organization
        public HttpRequest<ListIDDomainResponse> ListIDDomains(string orgId)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams["org_id"] = orgId;
            queryParams["desc"] = "true";
            var data = UnityPlayerIdentityUtility.UrlEncode(queryParams);
            
            var webReq = RequestJson(UnityWebRequest.kHttpVerbGET, "/iddomains?" + data, m_Headers, null);
            return new HttpRequest<ListIDDomainResponse>(webReq, ParseJsonResponse<ListIDDomainResponse>);
        }

        internal class IdProvider : ResponseBase
        {
            public string type;
            public string clientId;
            public string clientSecret;

            public AppleIdConfig appleIdConfig;
        }

        internal class AppleIdConfig
        {
            public string authKey;
            public string authKeyId;
            public string teamId;
            public string[] clientIds;
        }

        internal class ListIDProviderResponse : ResponseBase
        {
            public int count;
            public IdProvider[] data;
        }

        // CreateIdProvider creates a new ID provider for the organization's specified ID domain
        public HttpRequest<IdProvider> CreateIdProvider(string iddomain, IdProvider request)
        {
            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/iddomains/" + iddomain + "/idps", m_Headers, request);
            return new HttpRequest<IdProvider>(webReq, ParseJsonResponse<IdProvider>);
        }

        // ListIdProviders lists all ID providers created for the organization's specified ID domain
        public HttpRequest<ListIDProviderResponse> ListIdProviders(string iddomain)
        {
            var webReq = RequestJson(UnityWebRequest.kHttpVerbGET, "/iddomains/" + iddomain + "/idps", m_Headers, null);
            return new HttpRequest<ListIDProviderResponse>(webReq, ParseJsonResponse<ListIDProviderResponse>);
        }

        // GetIdProvider gets a specific ID provider from the organization's specified ID domain
        public HttpRequest<IdProvider> GetIdProvider(string iddomain, string type)
        {
            var webReq = RequestJson(UnityWebRequest.kHttpVerbGET, "/iddomains/" + iddomain + "/idps/" + type, m_Headers, null);
            return new HttpRequest<IdProvider>(webReq, ParseJsonResponse<IdProvider>);
        }

        // DeleteIdProvider removes a specific ID provider from the organization's specified ID domain
        public HttpRequest<IdProvider> DeleteIdProvider(string iddomain, string idpType)
        {
            var webReq = RequestJson(UnityWebRequest.kHttpVerbDELETE, "/iddomains/" + iddomain + "/idps/" + idpType, m_Headers, null);
            return new HttpRequest<IdProvider>(webReq, ParseJsonResponse<IdProvider>);
        }

        internal class CreateOAuthClientRequest
        {
            public string client_name;
            public string id_domain;
            public string project_id;
            public string[] redirect_uris;
            public string[] grant_types;
            public string[] response_types;
            public string owner;
            public string scope;
            public string token_endpoint_auth_method;
            public bool first_party;
        }

        internal class OAuthClient : ResponseBase
        {
            public string client_id;
            public string client_name;
            public string id_domain;
            public string project_id;
            public string[] redirect_uris;
            public string[] grant_types;
            public string[] response_types;
            public string owner;
            public string scope;
            public string token_endpoint_auth_method;
            public bool first_party;
        }

        internal class ListOAuthClientResponse : ResponseBase
        {
            public int count;
            public OAuthClient[] data;
        }

        // CreateOauthClient creates a new OAuth2 client for the organization's specified ID domain
        public HttpRequest<OAuthClient> CreateOAuthClient(string iddomain, CreateOAuthClientRequest request)
        {
            var webReq = RequestJson(UnityWebRequest.kHttpVerbPOST, "/oauth2/clients", GetHeadersWithIDDomain(iddomain), request);
            return new HttpRequest<OAuthClient>(webReq, ParseJsonResponse<OAuthClient>);
        }

        // ListOAuthClients lists all OAuth2 clients for the organization's specified ID domain
        public HttpRequest<ListOAuthClientResponse> ListOAuthClients(string iddomain)
        {
            var webReq = RequestJson(UnityWebRequest.kHttpVerbGET, "/oauth2/clients", GetHeadersWithIDDomain(iddomain), null);
            return new HttpRequest<ListOAuthClientResponse>(webReq, ParseJsonResponse<ListOAuthClientResponse>);
        }

        private Dictionary<string, string> GetHeadersWithIDDomain(string iddomain)
        {
            var headersWithIDDomain = new Dictionary<string, string>(m_Headers) { ["Id-Domain-Key"] = iddomain };
            return headersWithIDDomain;
        }
    }
}
