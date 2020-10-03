using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.EditorCoroutines.Editor;
using UnityEditor.PlayerIdentity.Management;
using UnityEngine.PlayerIdentity;
using UnityEngine.PlayerIdentity.Apple;
using UnityEngine.PlayerIdentity.UnityUserAuth;
using UnityPlayerIdentityUtility = UnityEngine.PlayerIdentity.Utils.Utility;

namespace UnityEditor.PlayerIdentity.UnityUserAuth 
{
    [CustomEditor(typeof(UnityUserAuthLoaderSettings))]
    public class UnityUserAuthLoaderSettingsEditor : Editor
    {
        private string apiUrl
        {
            get { return m_ApiBaseUrlProperty.stringValue; }
        }
        
        private Error m_UserAuthServiceError;
        private int m_Counter;

        private SerializedProperty m_ApiBaseUrlProperty;
        private SerializedProperty m_IdDomainIdProperty;
        private SerializedProperty m_OauthClientIdProperty;
        private SerializedProperty m_OauthScopesProperty;
        private SerializedProperty m_AutoCreateAnonymousUser;
        private SerializedProperty m_PersistRefreshToken;
        
        private class InProgress
        {
            private bool m_Value;

            public bool value
            {
                get { return m_Value; }
                set { m_Value = value; }
            }

            public InProgressScope Enter()
            {
                return new InProgressScope(this);
            }
        }
        
        private class InProgressScope : IDisposable
        {
            private InProgress m_IsInProgress;

            public InProgressScope(InProgress progress)
            {
                m_IsInProgress = progress;
                m_IsInProgress.value = true;
            }

            public void Dispose()
            {
                m_IsInProgress.value = false;
            }
        }

        private InProgress m_LoginInProgress = new InProgress();

        private string m_ProjectId;
        private string m_ProjectOrgId;
        
        private string m_DeveloperToken;
        private DateTime m_DeveloperTokenExpires;
        private string m_CodeVerifier;

        private void ResetLoginState()
        {
            m_LoginInProgress.value = false;
            m_ProjectId = null;
            m_ProjectOrgId = null;
            m_DeveloperToken = null;
            m_CodeVerifier = null;
        }
        
        private void OnLoginStatusGUI()
        {
            GUILayout.Label("Connection to Unity User Authentication Service", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                if (string.IsNullOrEmpty(CloudProjectSettings.accessToken))
                {
                    Reset();
                    EditorGUILayout.HelpBox(
                        "Developer is not logged in to Unity services. Please open Service window to connect.",
                        MessageType.Error, true);

                    Button("Open Services", () => { EditorApplication.ExecuteMenuItem("Window/General/Services"); });

                    return;
                }

                if (string.IsNullOrEmpty(CloudProjectSettings.projectId))
                {
                    Reset();
                    EditorGUILayout.HelpBox(
                        "Project is not linked to Unity cloud services. Please open Service window to connect.",
                        MessageType.Error, true);

                    Button("Open Services", () => { EditorApplication.ExecuteMenuItem("Window/General/Services"); });

                    return;
                }

                if (m_UserAuthServiceError != null)
                {
                    // If login fails with an error, show the error
                    EditorGUILayout.HelpBox(
                        "Error connecting to Unity User Authentication Service.\n" + m_UserAuthServiceError.message + " : " + m_ProjectOrgId,
                        MessageType.Error, true);

                    Button("Retry", () =>
                    {
                        Reset();
                        // Kick off login
                        EditorCoroutineUtility.StartCoroutine(GetDevTokenCoroutine(), this);
                    });
                }
                else if (!LoginIsValid)
                {
                    // Login in progress
                    EditorGUILayout.HelpBox(
                        "Connecting to User Authentication Service..." + Dots(), MessageType.Info, true);

                    if (!m_LoginInProgress.value)
                    {
                        // Kick off login
                        EditorCoroutineUtility.StartCoroutine(GetDevTokenCoroutine(), this);
                    }
                }
                else
                {
                    // Login successful
                    EditorGUILayout.HelpBox(
                        "Connected to User Authentication Service.", MessageType.Info, true);
                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("Refresh", GUILayout.MaxWidth(100)))
                    {
                        Reset();
                        // Kick off login
                        EditorCoroutineUtility.StartCoroutine(GetDevTokenCoroutine(), this);
                    };
                    if (GUILayout.Button("Copy Token", GUILayout.MaxWidth(100)))
                    {
                        GUIUtility.systemCopyBuffer = m_DeveloperToken;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private SerializedProperty m_SelectedIdDomainProperty;
        
        private string selectedIdDomain
        {
            get { return m_IdDomainIdProperty.stringValue; }
            set { m_IdDomainIdProperty.stringValue = value; }
        }

        private InProgress m_IdDomainOperationInProgress = new InProgress();
        
        private UnityUserAuthAdminClient.ListIDDomainResponse m_ListIdDomainResponse;
        
        private UnityUserAuthAdminClient.CreateIDDomainRequest m_CreateIdDomainRequest;
        
        private bool? m_ShowCreateIdDomain;

        private void ResetIdDomainSelectState()
        {
            m_IdDomainOperationInProgress.value = false;
            m_ListIdDomainResponse = null;
            m_CreateIdDomainRequest = null;
            m_ShowCreateIdDomain = null;
        }

        private string[] GetIdDomainIds()
        {
            if (m_ListIdDomainResponse?.data == null)
            {
                return new string[0];
            }

            return m_ListIdDomainResponse.data.Select(x => x.id + " | " + x.name).ToArray();
        }
        
        private void OnIDDomainSelectGUI()
        {
            if (!LoggedIn)
            {
                return;
            }
            
            GUILayout.Label("ID Domain Settings", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                if (m_ListIdDomainResponse == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Fetching ID Domains..." + Dots(), EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();

                    EditorCoroutineUtility.StartCoroutine(ListIdDomainsCoroutine(), this);

                    return;
                }

                EditorGUI.BeginDisabledGroup(m_IdDomainOperationInProgress.value);

                var idDomainIds = GetIdDomainIds();
                if (idDomainIds.Length > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.PrefixLabel("ID Domain");

                        var currentSelectedIdDomainIndex = 0;
                        if (!string.IsNullOrEmpty(selectedIdDomain))
                        {
                            currentSelectedIdDomainIndex = Math.Max(0,
                                Array.FindIndex(idDomainIds, x => x.StartsWith(selectedIdDomain)));
                        }

                        currentSelectedIdDomainIndex =
                            EditorGUILayout.Popup(currentSelectedIdDomainIndex, idDomainIds);
                        if (currentSelectedIdDomainIndex < idDomainIds.Length)
                        {
                            // The iddomain IDs format is `{id} {name}`
                            selectedIdDomain = idDomainIds[currentSelectedIdDomainIndex].Split(new[] {' '}, 2)
                                .FirstOrDefault();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }

                if (m_ShowCreateIdDomain == null)
                {
                    m_ShowCreateIdDomain = idDomainIds.Length == 0;
                }

                m_ShowCreateIdDomain = EditorGUILayout.ToggleLeft("Create new ID Domain", (bool) m_ShowCreateIdDomain);
                if ((bool) m_ShowCreateIdDomain)
                {
                    EditorGUI.indentLevel++;
                    if (m_CreateIdDomainRequest == null)
                    {
                        m_CreateIdDomainRequest = new UnityUserAuthAdminClient.CreateIDDomainRequest();
                    }

                    m_CreateIdDomainRequest.name =
                        EditorGUILayout.TextField("ID Domain Name", m_CreateIdDomainRequest.name);
                    m_CreateIdDomainRequest.orgId = m_ProjectOrgId;
                    m_CreateIdDomainRequest.backendType = "PLAYER_ID";

                    EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(m_CreateIdDomainRequest.name));
                    Button("Create", () => { EditorCoroutineUtility.StartCoroutine(CreateIdDomainCoroutine(), this); });

                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                }

                EditorGUI.EndDisabledGroup();
            }
        }

        private string selectedOAuthClient
        {
            get { return m_OauthClientIdProperty.stringValue; }
            set { m_OauthClientIdProperty.stringValue = value; }
        }
        
        private InProgress m_OauthClientOperationInProgress = new InProgress();

        private string m_OauthClientIdDomain;
        
        private UnityUserAuthAdminClient.ListOAuthClientResponse m_ListOAuthClientResponse;

        private UnityUserAuthAdminClient.CreateOAuthClientRequest m_CreateOAuthClientRequest;

        private string m_OauthClientCreateDefaultScopes;
        private string m_OauthClientCreateAdditionalScopes;
        
        private bool? m_ShowCreateOAuthClient;

        private void ResetOAuthClientSelectState()
        {
            m_OauthClientOperationInProgress.value = false;
            m_ListOAuthClientResponse = null;
            m_CreateOAuthClientRequest = null;
            m_ShowCreateOAuthClient = null;
        }

        private string[] GetOAuthClientIDs()
        {
            return m_ListOAuthClientResponse?.data == null ? new string[0] : m_ListOAuthClientResponse.data.Select(x => x.client_id + " | " + x.client_name).ToArray();
        }
        
        private void OnOAuthClientSelectGUI()
        {
            if (!LoggedIn || string.IsNullOrEmpty(selectedIdDomain))
            {
                return;
            }
            
            GUILayout.Label("OAuth Client Settings", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                if (m_OauthClientIdDomain != selectedIdDomain || m_ListOAuthClientResponse == null)
                {
                    m_OauthClientIdDomain = selectedIdDomain;
                    ResetOAuthClientSelectState();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Fetching OAuth clients..." + Dots(), EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();

                    EditorCoroutineUtility.StartCoroutine(ListOAuthClientsCoroutine(), this);
                    return;
                }

                EditorGUI.BeginDisabledGroup(m_OauthClientOperationInProgress.value);

                var clientIds = GetOAuthClientIDs();
                if (clientIds.Length > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.PrefixLabel("OAuth Client");

                        var currentSelectedOAuthClientIndex = 0;
                        if (!string.IsNullOrEmpty(selectedOAuthClient))
                        {
                            currentSelectedOAuthClientIndex = Math.Max(0,
                                Array.FindIndex(clientIds, x => x.StartsWith(selectedOAuthClient)));
                        }

                        currentSelectedOAuthClientIndex =
                            EditorGUILayout.Popup(currentSelectedOAuthClientIndex, clientIds);
                        if (currentSelectedOAuthClientIndex < clientIds.Length)
                        {
                            // The client IDs format is `{id} {name}`
                            selectedOAuthClient = clientIds[currentSelectedOAuthClientIndex].Split(new[] {' '}, 2)
                                .FirstOrDefault();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
                
                m_OauthScopesProperty.stringValue =
                    EditorGUILayout.TextField("OAuth Scopes", m_OauthScopesProperty.stringValue);

                if (m_ShowCreateOAuthClient == null)
                {
                    m_ShowCreateOAuthClient = clientIds.Length == 0;
                }

                m_ShowCreateOAuthClient =
                    EditorGUILayout.ToggleLeft("Create new OAuth Client", (bool) m_ShowCreateOAuthClient);
                if ((bool) m_ShowCreateOAuthClient)
                {
                    EditorGUI.indentLevel++;
                    if (m_CreateOAuthClientRequest == null)
                    {
                        m_CreateOAuthClientRequest = new UnityUserAuthAdminClient.CreateOAuthClientRequest();
                    }

                    m_CreateOAuthClientRequest.client_name =
                        EditorGUILayout.TextField("Client Name", m_CreateOAuthClientRequest.client_name);
                    m_CreateOAuthClientRequest.owner = m_ProjectOrgId;
                    m_CreateOAuthClientRequest.id_domain = selectedIdDomain;
                    m_CreateOAuthClientRequest.project_id = m_ProjectId;

                    if (m_CreateOAuthClientRequest.redirect_uris == null)
                    {
                        m_CreateOAuthClientRequest.redirect_uris = new []
                        {
                            Application.identifier.ToLowerInvariant() + "://callback"
                        };
                    }

                    var redirectUris = EditorGUILayout.TextField("Redirect Uris",
                        string.Join(", ", m_CreateOAuthClientRequest.redirect_uris));
                    m_CreateOAuthClientRequest.redirect_uris = redirectUris.Split(',').Select(x => x.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                    m_CreateOAuthClientRequest.grant_types = new[]
                    {
                        "authorization_code",
                        "refresh_token"
                    };

                    m_CreateOAuthClientRequest.response_types = new[]
                    {
                        "code",
                        "token"
                    };
                    m_CreateOAuthClientRequest.token_endpoint_auth_method = "none";
                    m_CreateOAuthClientRequest.first_party = true;

                    // The default scopes for all OAuth clients created by this plugin
                    if (m_OauthClientCreateDefaultScopes == null)
                    {
                        m_OauthClientCreateDefaultScopes = "openid offline identity.user";

                        var buf = new StringBuilder();
                        foreach (var platform in Enum.GetValues(typeof(RuntimePlatform)))
                        {
                            var field = platform.GetType().GetField(platform.ToString());
                            var attributes = field.GetCustomAttributes(typeof(ObsoleteAttribute), false);
                            if (attributes != null && attributes.Length > 0)
                            {
                                continue;
                            }

                            buf.Append(" platform/" + platform);
                        }

                        m_OauthClientCreateDefaultScopes += buf.ToString();
                    }

                    m_OauthClientCreateAdditionalScopes =
                        EditorGUILayout.TextField("Additional Scopes", m_OauthClientCreateAdditionalScopes);

                    m_CreateOAuthClientRequest.scope = m_OauthClientCreateDefaultScopes;
                    if (!string.IsNullOrWhiteSpace(m_OauthClientCreateAdditionalScopes))
                    {
                        m_CreateOAuthClientRequest.scope += m_OauthClientCreateAdditionalScopes.Trim();
                    }

                    EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(m_CreateOAuthClientRequest.client_name));
                    Button("Create",
                        () => { EditorCoroutineUtility.StartCoroutine(CreateOAuthClientCoroutine(), this); });

                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                }

                EditorGUI.EndDisabledGroup();
            }
        }

        const string k_NoIdProvidersAvailable = "";
        private InProgress m_IdpOperationInProgress = new InProgress();

        private string m_IdpIdDomain;
        
        private UnityUserAuthAdminClient.ListIDProviderResponse m_ListIdProviderResponse;

        private List<LoaderInfo> m_AllBackendLoaders;
        private List<LoaderInfo> m_AllProviderLoaders;

        private string m_AppleIdConfigAppId;
        private string m_AppleIdConfigServiceId;

        private UnityUserAuthAdminClient.IdProvider m_CreateIdProviderRequest;
        
        private bool? m_ShowCreateIdProvider;
        private bool? m_ShowAppleAuthKey;
        
        private void ResetIdProviderState()
        {
            m_IdpOperationInProgress.value = false;
            m_ListIdProviderResponse = null;
            m_CreateIdProviderRequest = null;
            m_ShowCreateIdProvider = null;

            m_AllBackendLoaders = null;
            m_AllProviderLoaders = null;
            m_ShowAppleAuthKey = null;
        }

        private string[] GetIdProviderIDs()
        {
            return m_ListIdProviderResponse?.data == null ? new string[0] : m_ListIdProviderResponse.data.Select(x => x.type).ToArray();
        }
        
        private string[] GetAvailableProviderIDs()
        {
            if (m_AllProviderLoaders == null)
            {
                return new string[0];
            }

            return m_AllProviderLoaders.Select(x =>
            {
                return x.instance.LoginProviderID;
            }).Except(m_ListIdProviderResponse.data.Select(x => x.type)).OrderBy(x => x).ToArray();
        }

        private void OnIDProviderSelectGUI()
        {
            if (!LoggedIn || string.IsNullOrEmpty(selectedIdDomain))
            {
                return;
            }
            
            GUILayout.Label("ID Provider Settings", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                if (m_IdpIdDomain != selectedIdDomain || m_ListIdProviderResponse == null)
                {
                    m_IdpIdDomain = selectedIdDomain;
                    ResetIdProviderState();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Fetching ID Provider settings..." + Dots(), EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();

                    EditorCoroutineUtility.StartCoroutine(ListIdProvidersCoroutine(), this);
                    return;
                }
                
                if (m_ListIdProviderResponse.data == null)
                {
                    m_ListIdProviderResponse.data = new UnityUserAuthAdminClient.IdProvider[0];
                }

                EditorGUI.BeginDisabledGroup(m_IdpOperationInProgress.value);

                var clientIDs = GetIdProviderIDs();
                if (clientIDs.Length > 0)
                {
                    GUILayout.Label("ID Providers");
                    foreach (var idp in m_ListIdProviderResponse.data)
                    {
                        if (idp.type == "wechat.com" || idp.type == "apple.com")
                        {                    
                            EditorGUILayout.LabelField(idp.type);
                            OnIdProviderGUI(false, idp);
                            EditorGUILayout.Space();
                        }
                    }
                }

                if (m_ShowCreateIdProvider == null)
                {
                    m_ShowCreateIdProvider = clientIDs.Length == 0;
                }

                m_ShowCreateIdProvider =
                    EditorGUILayout.ToggleLeft("Create new ID provider", (bool) m_ShowCreateIdProvider);
                if ((bool) m_ShowCreateIdProvider)
                {
                    EditorGUI.indentLevel++;
                    if (m_CreateIdProviderRequest == null)
                    {
                        m_CreateIdProviderRequest = new UnityUserAuthAdminClient.IdProvider();
                    }

                    if (m_AllProviderLoaders == null)
                    {
                        m_AllBackendLoaders = new List<LoaderInfo>();
                        m_AllProviderLoaders = new List<LoaderInfo>();
                        PlayerIdentitySettingsManager.PopulateAllKnownLoaderInfos(
                            m_AllBackendLoaders,
                            m_AllProviderLoaders);
                    }
                    
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.PrefixLabel("ID Provider Type");

                        var availableTypes = GetAvailableProviderIDs();
                        var currentSelectedIndex = 0;
                        if (!string.IsNullOrEmpty(m_CreateIdProviderRequest.type))
                        {
                            currentSelectedIndex = Math.Max(0,
                                Array.FindIndex(availableTypes, x => x.Equals(m_CreateIdProviderRequest.type)));
                        }

                        currentSelectedIndex = EditorGUILayout.Popup(currentSelectedIndex, availableTypes);
                        m_CreateIdProviderRequest.type = currentSelectedIndex < availableTypes.Length ? availableTypes[currentSelectedIndex] : k_NoIdProvidersAvailable;
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    OnIdProviderGUI(true, m_CreateIdProviderRequest);
                    EditorGUILayout.Space();
                    
                    EditorGUI.indentLevel--;
                }

                EditorGUI.EndDisabledGroup();
            }
        }

        private void OnIdProviderGUI(bool isNew, UnityUserAuthAdminClient.IdProvider idp)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                var isValid = true;

                switch (idp.type) {
                    // If no Identity Provider is selected, no fields or options should be available
                    case k_NoIdProvidersAvailable:
                        break;
                    case SignInWithApplePlayerIdentitySubsystem.k_ProviderId: {
                        if (idp.appleIdConfig == null)
                        {
                            idp.appleIdConfig = new UnityUserAuthAdminClient.AppleIdConfig();
                        }

                        idp.appleIdConfig.teamId = EditorGUILayout.TextField("Apple Team ID", idp.appleIdConfig.teamId);
                        isValid = isValid && !string.IsNullOrWhiteSpace(idp.appleIdConfig.teamId);

                        if (m_ShowAppleAuthKey == null)
                        {
                            m_ShowAppleAuthKey = string.IsNullOrWhiteSpace(idp.appleIdConfig.authKey);
                        }

                        m_ShowAppleAuthKey = EditorGUILayout.Toggle("Apple Auth Key", (bool) m_ShowAppleAuthKey);
                        if ((bool)m_ShowAppleAuthKey)
                        {
                            idp.appleIdConfig.authKey = EditorGUILayout.TextArea(idp.appleIdConfig.authKey);
                        }
                        isValid = isValid && !string.IsNullOrWhiteSpace(idp.appleIdConfig.authKey);

                        idp.appleIdConfig.authKeyId =
                            EditorGUILayout.TextField("Apple Key ID", idp.appleIdConfig.authKeyId);
                        isValid = isValid && !string.IsNullOrWhiteSpace(idp.appleIdConfig.authKeyId);

                        var clientIds = new List<string>();
                        if (idp.appleIdConfig.clientIds != null)
                        {
                            // Use the first client ID as app ID and second as service ID
                            if (idp.appleIdConfig.clientIds.Length > 0)
                            {
                                m_AppleIdConfigAppId = idp.appleIdConfig.clientIds[0];
                            }

                            if (idp.appleIdConfig.clientIds.Length > 1)
                            {
                                m_AppleIdConfigServiceId = idp.appleIdConfig.clientIds[1];
                            }
                        }
                    
                        if (m_AppleIdConfigAppId == null)
                        {
                            m_AppleIdConfigAppId = Application.identifier;
                        }

                        m_AppleIdConfigAppId = EditorGUILayout.TextField("Apple App ID", m_AppleIdConfigAppId);
                        isValid = isValid && !string.IsNullOrWhiteSpace(m_AppleIdConfigAppId);
                        if (!string.IsNullOrWhiteSpace(m_AppleIdConfigAppId))
                        {
                            clientIds.Add(m_AppleIdConfigAppId);
                        }

                        if (m_AppleIdConfigServiceId == null)
                        {
                            m_AppleIdConfigServiceId = Application.identifier + ".service";
                        }

                        m_AppleIdConfigServiceId = EditorGUILayout.TextField("Apple Service ID", m_AppleIdConfigServiceId);
                        if (!string.IsNullOrWhiteSpace(m_AppleIdConfigServiceId))
                        {
                            clientIds.Add(m_AppleIdConfigServiceId);
                        }

                        idp.appleIdConfig.clientIds = new[]
                        {
                            m_AppleIdConfigAppId,
                            m_AppleIdConfigServiceId,
                        };
                        break;
                    }
                    default:
                        idp.clientId = EditorGUILayout.TextField("Client ID", idp.clientId);
                        idp.clientSecret = EditorGUILayout.PasswordField("Client Secret", idp.clientSecret);

                        isValid = !string.IsNullOrWhiteSpace(idp.clientId);
                        break;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 20);

                EditorGUI.BeginDisabledGroup(!isValid);
                if (GUILayout.Button("Save", GUILayout.MaxWidth(100)))
                {
                    EditorCoroutineUtility.StartCoroutine(UpdateIdProvidersCoroutine(idp), this);

                    m_ShowAppleAuthKey = null;
                }

                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("Discard", GUILayout.MaxWidth(100)))
                {
                    GUI.FocusControl(null);
                    if (!isNew)
                    {
                        EditorCoroutineUtility.StartCoroutine(RefreshIdProvidersCoroutine(idp), this);
                    }
                    else
                    {
                        m_ShowCreateIdProvider = null;
                        m_CreateIdProviderRequest = null;
                    }
                }

                if (!isNew) {
                    if (GUILayout.Button("Delete", GUILayout.MaxWidth(100)))
                    {
                        EditorCoroutineUtility.StartCoroutine(DeleteIdProvidersCoroutine(idp), this);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
        }

        private void OnUserAuthSettingsGUI()
        {
            GUILayout.Label("User Auth Settings", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                m_ApiBaseUrlProperty.stringValue = EditorGUILayout.TextField("API base URL",
                    m_ApiBaseUrlProperty.stringValue);

                m_AutoCreateAnonymousUser.boolValue = EditorGUILayout.ToggleLeft("Automatically create anonymous user.",
                    m_AutoCreateAnonymousUser.boolValue);
                
                m_PersistRefreshToken.boolValue = EditorGUILayout.ToggleLeft("Automatically persist refresh token to enable user auto sign-in.",
                    m_PersistRefreshToken.boolValue);
            }
        }

        private string m_ServerUrl = "https://gai-api.unity.cn";
        private string m_CreateAppResult="";
        private List<string> m_GetAppResult=new List<string>();
        private int m_AntiAddictionAppType = 0;
        private string m_AppName="";
        private Texture2D m_AntiAddictionLogo;
        private bool m_canCreateNewApp = true;
        private bool m_canGetApp = true;
        private bool m_FirstTimeGetApp = true;

        private void OnAntiAddictionAppGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("AntiAddiction App Settings", EditorStyles.boldLabel, GUILayout.Width(180), GUILayout.Height(25));
            if (m_AntiAddictionLogo == null)
                m_AntiAddictionLogo = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.unity.playerid-cn/Sprites/AntiAddiction_Icon.png", typeof(Texture2D));
            GUILayout.Label(m_AntiAddictionLogo, EditorStyles.boldLabel,GUILayout.Width(150),GUILayout.Height(30));
            EditorGUILayout.EndHorizontal();
            using (new EditorGUI.IndentLevelScope())
            {
                GUILayout.Label("Create App&Get App");
                m_AppName = EditorGUILayout.TextField("Name", m_AppName);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 20);
                EditorGUI.BeginDisabledGroup(!m_canCreateNewApp);
                if (GUILayout.Button("CreateApp", GUILayout.MaxWidth(100)))
                {
                    m_CreateAppResult = "";
                    if (m_AppName == "")
                    {
                        m_CreateAppResult = "App名称不能为空，请输入名称";
                    }
                    else
                    {
                        CreateAppInput createAppInput = new CreateAppInput();
                        createAppInput.name = m_AppName;
                        EditorCoroutineUtility.StartCoroutine(CreateApp(createAppInput), this); 
                    }
                    m_AntiAddictionAppType = 1;
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(!m_canGetApp);
                if (GUILayout.Button("GetApp", GUILayout.MaxWidth(100)))
                {
                    EditorCoroutineUtility.StartCoroutine(GetApp(), this);
                    m_AntiAddictionAppType = 2;
                }
                EditorGUI.EndDisabledGroup();
                if (m_FirstTimeGetApp & m_DeveloperToken != null)
                {
                    EditorCoroutineUtility.StartCoroutine(GetApp(), this);
                    m_AntiAddictionAppType = 2;
                    m_FirstTimeGetApp = false;
                }   
                EditorGUILayout.EndHorizontal();

                if (m_AntiAddictionAppType == 1)
                {
                    EditorGUILayout.TextField(m_CreateAppResult);
                }
                else if (m_AntiAddictionAppType == 2)
                {
                    for (int i = 0; i < m_GetAppResult.Count; i++)
                    {
                        EditorGUILayout.TextField(m_GetAppResult[i]);
                    }
                }
            }
        }

        private IEnumerator CreateApp(CreateAppInput createAppInput)
        {
            string result;
            m_canCreateNewApp = false;
            bool flag = true;
            string dataJson = UnityEngine.PlayerIdentity.Utils.DeJson.Serialize.From(createAppInput);
            byte[] body = Encoding.UTF8.GetBytes(dataJson);
            UnityEngine.Networking.UnityWebRequest request = new UnityEngine.Networking.UnityWebRequest(m_ServerUrl + "/admin/" + UnityUserAuthLoader.GetSettings().IDDomainID + "/config", "POST");
            request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(body);
            request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            request.SetRequestHeader("Authorization", "Bearer " + m_DeveloperToken);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            while (flag)
            {
                yield return null;
                if (request.isDone)
                {
                    if (request.error != null)
                    {
                        m_CreateAppResult = request.error;
                    }
                    else
                    {
                        result = request.downloadHandler.text;
                        CreateAppReturn createAppReturn = UnityEngine.PlayerIdentity.Utils.DeJson.Deserialize.To<CreateAppReturn>(result);
                        m_CreateAppResult = "ConfigId:"+createAppReturn.id+";  Name:"+ createAppInput.name;                    
                    }
                    m_canCreateNewApp = true;
                    flag = false;
                }
            }
        }

        private IEnumerator GetApp()
        {
            string result;
            m_canGetApp = false;
            bool flag = true;
            UnityEngine.Networking.UnityWebRequest request = new UnityEngine.Networking.UnityWebRequest(m_ServerUrl + "/admin/" + UnityUserAuthLoader.GetSettings().IDDomainID + "/config","GET");
            request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            request.SetRequestHeader("Authorization", "Bearer " + m_DeveloperToken);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            while (flag)
            {
                yield return null;
                if (request.isDone)
                {
                    if (request.error != null)
                    {
                        result = request.error;
                        List<string> tempResultErrorList = new List<string>();
                        tempResultErrorList.Add(result);
                        m_GetAppResult = tempResultErrorList;
                    }
                    else
                    {
                        result = request.downloadHandler.text;
                        string tempResult = result.Insert(result.Length - 1, "}").Insert(0, "{\"getAppReturns\":");
                        GetAppReturnList getAppReturnList = UnityEngine.PlayerIdentity.Utils.DeJson.Deserialize.To<GetAppReturnList>(tempResult);
                        List<string> tempResultList = new List<string>();
                        for (int i = 0; i < getAppReturnList.getAppReturns.Count; i++)
                        {
                            tempResultList.Add("ConfigId:"+getAppReturnList.getAppReturns[i].id+";  AppName:"+getAppReturnList.getAppReturns[i].name);
                        }
                        m_GetAppResult = tempResultList;
                    }
                    m_canGetApp = true;
                    flag = false;
                } 
            }
        }
        
        private void Reset()
        {
            m_Counter = 0;
            ResetLoginState();
            ResetIdDomainSelectState();
            ResetOAuthClientSelectState();
            ResetIdProviderState();
        }

        public void OnEnable()
        {
            if (m_ApiBaseUrlProperty == null)
            {
                m_ApiBaseUrlProperty = serializedObject.FindProperty("m_apiBaseUrl");
            }
            if (m_IdDomainIdProperty == null)
            {
                m_IdDomainIdProperty = serializedObject.FindProperty("m_idDomainID");
            }
            if (m_OauthClientIdProperty == null)
            {
                m_OauthClientIdProperty = serializedObject.FindProperty("m_oauthClientId");
            }
            if (m_OauthScopesProperty == null)
            {
                m_OauthScopesProperty = serializedObject.FindProperty("m_oauthScopes");
            }
            if (m_AutoCreateAnonymousUser == null)
            {
                m_AutoCreateAnonymousUser = serializedObject.FindProperty("m_autoCreateAnonymousUser");
            }
            if (m_PersistRefreshToken == null)
            {
                m_PersistRefreshToken = serializedObject.FindProperty("m_persistRefreshToken");
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }
        
        public override void OnInspectorGUI()
        {
            ++m_Counter;
            
            serializedObject.Update();
            
            // Create a GUI object that indicates whether or not the logged in user is authenticated or not
            OnLoginStatusGUI();
            EditorGUILayout.Space();

            if (!LoginIsValid)
            {
                return;
            }
            
            OnIDDomainSelectGUI();
            EditorGUILayout.Separator();
            
            OnOAuthClientSelectGUI();
            EditorGUILayout.Separator();

            OnIDProviderSelectGUI();
            EditorGUILayout.Separator();

            OnUserAuthSettingsGUI();
            EditorGUILayout.Separator();

            OnAntiAddictionAppGUI();
            EditorGUILayout.Separator();

            serializedObject.ApplyModifiedProperties();
        }

        private bool LoginMatchesCurrentProject
        {
            get { return m_ProjectId == CloudProjectSettings.projectId; }
        }

        private bool LoginIsValid
        {
            get { return LoggedIn && !LoginExpired; }
        }
        
        private bool LoggedIn
        {
            get { return m_DeveloperToken != null; }
        }

        private bool LoginExpired
        {
            get { return DateTime.Now + TimeSpan.FromMinutes(1) >= m_DeveloperTokenExpires; }
        }
        
        private IEnumerator GetDevTokenCoroutine()
        {
            if (m_LoginInProgress.value)
            {
                yield break;
            }

            using (m_LoginInProgress.Enter())
            {
                var client = new UnityDevIdClient();
                var cloudAccessToken = CloudProjectSettings.accessToken;
                // Reset before start
                m_UserAuthServiceError = null;

                if (!LoginMatchesCurrentProject)
                {
                    var projectInfoReq =
                        client.GetUnityProjectInfo(cloudAccessToken, CloudProjectSettings.projectId);
                    yield return projectInfoReq.SendForEditor();
                    projectInfoReq.HandleResponse();

                    if (projectInfoReq.Error != null)
                    {
                        m_UserAuthServiceError = projectInfoReq.Error;
                        yield break;
                    }

                    var projectInfo = projectInfoReq.Result;

                    // Save results
                    m_ProjectOrgId = projectInfo.org_foreign_key;
                    m_ProjectId = CloudProjectSettings.projectId;

                    // Reset developer token
                    m_DeveloperToken = null;
                }

                // Silently sign in the token
                if (!LoginIsValid)
                {
                    m_CodeVerifier = UnityPlayerIdentityUtility.RandomBase64String(36);

                    // Get the auth code
                    var authCodeReq = client.GetDevAuthCode(cloudAccessToken, m_ProjectOrgId, m_CodeVerifier);
                    yield return authCodeReq.SendForEditor();
                    authCodeReq.HandleResponse();

                    if (authCodeReq.Error != null)
                    {
                        m_UserAuthServiceError = authCodeReq.Error;
                        yield break;
                    }

                    var tokenReq = client.GetDevToken(authCodeReq.Result.code, m_CodeVerifier);
                    yield return tokenReq.SendForEditor();
                    tokenReq.HandleResponse();

                    if (tokenReq.Error != null)
                    {
                        m_UserAuthServiceError = tokenReq.Error;
                        yield break;
                    }

                    m_DeveloperTokenExpires = DateTime.Now + TimeSpan.FromSeconds(int.Parse(tokenReq.Result.expires_in));
                    m_DeveloperToken = tokenReq.Result.access_token;
                }

                m_CodeVerifier = null;
            }
        }

        private IEnumerator ListIdDomainsCoroutine()
        {
            if (m_IdDomainOperationInProgress.value)
            {
                yield break;
            }

            using (m_IdDomainOperationInProgress.Enter())
            {
                var client = new UnityUserAuthAdminClient(apiUrl, m_DeveloperToken);

                var request = client.ListIDDomains(m_ProjectOrgId);
                yield return request.SendForEditor();
                request.HandleResponse();

                if (request.Error != null)
                {
                    m_UserAuthServiceError = request.Error;
                    yield break;
                }

                m_ListIdDomainResponse = request.Result;
                if (request.Result.count == 0)
                {
                    selectedIdDomain = "";
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private IEnumerator CreateIdDomainCoroutine()
        {
            if (m_IdDomainOperationInProgress.value)
            {
                yield break;
            }

            using (m_IdDomainOperationInProgress.Enter())
            {
                var client = new UnityUserAuthAdminClient(apiUrl, m_DeveloperToken);

                var request = client.CreateIdDomain(m_CreateIdDomainRequest);
                yield return request.SendForEditor();
                request.HandleResponse();

                if (request.Error != null)
                {
                    m_UserAuthServiceError = request.Error;
                    m_UserAuthServiceError.message += " : " + m_ProjectOrgId;
                    yield break;
                }

                if (m_ListIdDomainResponse == null)
                {
                    yield break;
                }

                if (m_ListIdDomainResponse.data == null)
                {
                    m_ListIdDomainResponse.data = new UnityUserAuthAdminClient.IDDomain[0];
                }

                m_ListIdDomainResponse.data = m_ListIdDomainResponse.data.Append(request.Result).ToArray();
                selectedIdDomain = request.Result.id;
                serializedObject.ApplyModifiedProperties();

                m_CreateIdDomainRequest = null;
                m_ShowCreateIdDomain = false;
            }
            
            // Update the ID domain list
            yield return CreateIdProvidersCoroutine(new UnityUserAuthAdminClient.IdProvider() {type = "wechat.com"});
            yield return ListIdDomainsCoroutine();
        }

        private IEnumerator ListOAuthClientsCoroutine()
        {
            if (m_OauthClientOperationInProgress.value)
            {
                yield break;
            }

            using (m_OauthClientOperationInProgress.Enter())
            {
                var client = new UnityUserAuthAdminClient(apiUrl, m_DeveloperToken);

                var request = client.ListOAuthClients(selectedIdDomain);
                yield return request.SendForEditor();
                request.HandleResponse();

                if (request.Error != null)
                {
                    m_UserAuthServiceError = request.Error;
                    if (m_UserAuthServiceError.message.Contains("UNAUTHORIZED_REQUEST"))
                    {
                        selectedIdDomain = "";
                        serializedObject.ApplyModifiedProperties();
                        m_UserAuthServiceError = null;
                    }
                    yield break;
                }

                m_ListOAuthClientResponse = request.Result;
            }
        }

        private IEnumerator CreateOAuthClientCoroutine()
        {
            if (m_OauthClientOperationInProgress.value)
            {
                yield break;
            }

            using (m_OauthClientOperationInProgress.Enter())
            {
                var client = new UnityUserAuthAdminClient(apiUrl, m_DeveloperToken);

                var request = client.CreateOAuthClient(selectedIdDomain, m_CreateOAuthClientRequest);
                yield return request.SendForEditor();
                request.HandleResponse();

                if (request.Error != null)
                {
                    m_UserAuthServiceError = request.Error;
                    yield break;
                }
                
                if (m_ListOAuthClientResponse == null)
                {
                    yield break;
                }
                
                if (m_ListOAuthClientResponse.data == null)
                {
                    m_ListOAuthClientResponse.data = new UnityUserAuthAdminClient.OAuthClient[0];
                }

                m_ListOAuthClientResponse.data = m_ListOAuthClientResponse.data.Append(request.Result).ToArray();
                selectedOAuthClient = request.Result.client_id;
                serializedObject.ApplyModifiedProperties();

                m_CreateOAuthClientRequest = null;
                m_ShowCreateOAuthClient = false;
            }

            // Update the OAuth client list
            yield return ListOAuthClientsCoroutine();
        }

        private IEnumerator ListIdProvidersCoroutine()
        {
            if (m_IdpOperationInProgress.value)
            {
                yield break;
            }

            using (m_IdpOperationInProgress.Enter())
            {
                var client = new UnityUserAuthAdminClient(apiUrl, m_DeveloperToken);
                var request = client.ListIdProviders(selectedIdDomain);
                yield return request.SendForEditor();
                request.HandleResponse();

                if (request.Error != null)
                {
                    m_UserAuthServiceError = request.Error;
                    if (m_UserAuthServiceError.message.Contains("UNAUTHORIZED_REQUEST"))
                    {
                        selectedIdDomain = "";
                        serializedObject.ApplyModifiedProperties();
                        m_UserAuthServiceError = null;
                    }
                    yield break;
                }

                m_ListIdProviderResponse = request.Result;
            }
        }
        
        private IEnumerator RefreshIdProvidersCoroutine(UnityUserAuthAdminClient.IdProvider idp)
        {
            if (m_IdpOperationInProgress.value)
            {
                yield break;
            }

            using (m_IdpOperationInProgress.Enter())
            {
                var client = new UnityUserAuthAdminClient(apiUrl, m_DeveloperToken);

                var request = client.GetIdProvider(selectedIdDomain, idp.type);
                yield return request.SendForEditor();
                request.HandleResponse();

                if (request.Error != null)
                {
                    m_UserAuthServiceError = request.Error;
                    yield break;
                }

                m_ListIdProviderResponse.data = m_ListIdProviderResponse.data.
                    Where(x => x.type != idp.type).Append(request.Result).OrderBy(x => x.type).ToArray();
            }
        }

        private IEnumerator UpdateIdProvidersCoroutine(UnityUserAuthAdminClient.IdProvider idp)
        {
            if (m_IdpOperationInProgress.value)
            {
                yield break;
            }

            yield return DeleteIdProvidersCoroutine(idp);
            yield return CreateIdProvidersCoroutine(idp);
        }

        private IEnumerator CreateIdProvidersCoroutine(UnityUserAuthAdminClient.IdProvider idp)
        {
            if (m_IdpOperationInProgress.value)
            {
                yield break;
            }

            using (m_IdpOperationInProgress.Enter())
            {
                var client = new UnityUserAuthAdminClient(apiUrl, m_DeveloperToken);

                var request = client.CreateIdProvider(selectedIdDomain, idp);
                yield return request.SendForEditor();
                request.HandleResponse();

                if (request.Error != null)
                {
                    m_UserAuthServiceError = request.Error;
                    yield break;
                }
                
                if (m_ListIdProviderResponse == null)
                {
                    yield break;
                }
                
                if (m_ListIdProviderResponse.data == null)
                {
                    m_ListIdProviderResponse.data = new UnityUserAuthAdminClient.IdProvider[0];
                }
                
                // Update the list response to save another list call
                m_ListIdProviderResponse.data = m_ListIdProviderResponse.data.Append(request.Result).OrderBy(x => x.type).ToArray();
            }
        }

        private IEnumerator DeleteIdProvidersCoroutine(UnityUserAuthAdminClient.IdProvider idp)
        {
            if (m_IdpOperationInProgress.value)
            {
                yield break;
            }

            using (m_IdpOperationInProgress.Enter())
            {
                var client = new UnityUserAuthAdminClient(apiUrl, m_DeveloperToken);

                var request = client.DeleteIdProvider(selectedIdDomain, idp.type);
                yield return request.SendForEditor();
                request.HandleResponse();

                if (request.Error != null)
                {
                    if (request.Error.type != "RESOURCE_NOT_FOUND")
                    {
                        m_UserAuthServiceError = request.Error;
                        yield break;                        
                    }
                }

                // Update the list response to save another list call
                m_ListIdProviderResponse.data = m_ListIdProviderResponse.data.Where(x => x.type != idp.type).ToArray();
            }
        }

        private void Button(string text, Action buttonAction)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 20);
            if (GUILayout.Button(text, GUILayout.MaxWidth(100)))
            {
                buttonAction();
            }
            EditorGUILayout.EndHorizontal();
        }

        private string Dots()
        {
            return new string('.', (m_Counter / 10) % 10);
        }

        internal class CreateAppInput
        {
            public string clientId;
            public string clientSecret;
            public string name;
        }

        internal class CreateAppReturn
        {
            public string clientId;
            public string id;
        }

        internal class GetAppReturn
        {
            public string id;
            public string clientId;
            public string name;
        }

        internal class GetAppReturnList
        {
            public List<GetAppReturn> getAppReturns;
        }
    }
}
