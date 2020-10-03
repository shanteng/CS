using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityPlayerIdentityUtility = UnityEngine.PlayerIdentity.Utils.Utility;
using UnityEngine.Scripting;

namespace UnityEngine.PlayerIdentity.UnityUserAuth
{
    /// <summary>
    /// UnityUserAuthBackend is an implementation of PlayerIdentityBackendSubsystem using
    /// Unity User Authentication Service
    /// </summary>
    [Preserve]
    public class UnityUserAuthBackend : PlayerIdentityBackendSubsystem
    {
        // Settings is internal for testing purposes
        internal UnityUserAuthLoaderSettings settings;
        
        // Register will add and run the UAS Backend subsystem
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Register()
        {
            PlayerIdentityBackendSubsystemDescriptor.RegisterDescriptor(new PlayerIdentityBackendSubsystemCInfo
            {
                id = UnityUserAuthLoader.k_SubsystemId,
                displayName = "Unity UserAuth Service",
                implementationType = typeof(UnityUserAuthBackend),
                supportsAnonymousLogin = true,
                supportsEmailPasswordLogin = true,
                supportsTextMessageLogin = false,
            });
        }

        public string idDomainId
        {
            get { return m_IdDomainId; }
        }

        private string m_IdDomainId;
        
        private bool m_Running;
        
        // Start initializes the UAS backend
        public override void Start()
        {
            settings = UnityUserAuthLoader.GetSettings();
            m_IdDomainId = settings.IDDomainID;
            m_Client = new IdentityClient(settings.APIBaseUrl, settings.IDDomainID);
            if (persistentService != null)
            {
                if (settings.PersistRefreshToken)
                {
                    m_State.anonymousToken = persistentService.ReadValue(settings.AnonymousTokenPersistKey);
                    m_State.refreshToken = persistentService.ReadValue(settings.RefreshTokenPersistKey);
                }
                else
                {
                    persistentService.DeleteKey(settings.AnonymousTokenPersistKey);
                    persistentService.DeleteKey(settings.RefreshTokenPersistKey);
                    m_State.anonymousToken = "";
                    m_State.refreshToken = "";
                }
            }
            m_Running = true;
        }

        public override void Stop()
        {
            m_Running = false;
        }
        
        protected override void OnDestroy()
        {
            
        }

        public override bool running
        {
            get { return m_Running; }
        }

        private struct State
        {
            public string refreshToken;
            public string anonymousToken;
            public string authnIdToken;
            public string codeVerifier;
            public string accessToken;
            public bool isAnonymouslySignedIn;
        }

        private State m_State;
        
        private IdentityClient m_Client;

        public IPersistentService persistentService = new PlayerPrefsPersistentService();

        // Logout will log users out and delete tokens as needed
        public override void Logout(Callback callback)
        {
            persistentService?.DeleteKey(settings.RefreshTokenPersistKey);

            m_State = new State();

            // Restore persistent anonymous token
            LoadPersistedTokens();

            callback?.Invoke(new IdentityCallbackArgs());
        }

        // RestoreLogin automatically logs users in depending if token exists/settings
        public override void RestoreLogin(Callback callback)
        {
            if (!string.IsNullOrEmpty(m_State.refreshToken))
            {
                RefreshAccessToken(callback);
            }
            else
            {
                if (settings.AutoCreateAnonymousUser)
                {
                    LoginAnonymous(callback);
                }
                else
                {
                    // Otherwise, only log in anonymous users if the token already exists
                    if (!string.IsNullOrEmpty(m_State.anonymousToken))
                    {
                        LoginAnonymous(callback);
                    }
                }
            }
        }

        // LoginAnonymous signs in anonymous/guest users
        public override void LoginAnonymous(Callback callback)
        {
            HttpRequest<AuthenticationResponse> request = null;
            if (!string.IsNullOrEmpty(m_State.anonymousToken))
            {
                request = m_Client.SessionTokenAuth(new SessionTokenAuthRequest
                {
                    sessionToken = m_State.anonymousToken
                });
            }
            else
            {
                request = m_Client.SignUpAnonymous(new AnonymousUserRequest());
            }

            var response = request.Send();
            response.completed += op =>
            {
                var resp = request.HandleResponse();
                if (resp.error != null)
                {
                    // Error handling: If the error is due to invalid token, clear the token from persistent storage.
                    if (IsTokenError(resp.error))
                    {
                        Utils.Logger.Info("Got invalid token error, clear the cached anonymous token.");
                        m_State.anonymousToken = null;
                        PersistTokens();
                    }
                    
                    SendErrorCallback(resp.error, callback);
                    return;
                }
                SaveAnonymousAuthResponse(resp.result);
                Authorize(callback);
            };
        }

        // Login signs in registered email/password users
        public override void Login(string email, string password, Callback callback)
        {
            var request = m_Client.PasswordAuth(new PasswordAuthRequest
            {
                email = email,
                password = password
            });
            DoAuthenticate(request, callback);
        }

        // Register creates a new email/password user
        public override void Register(string email, string password, Callback callback)
        {
            var request = m_Client.SignUp(new SignupRequest
            {
                email = email,
                password = password
            });

            DoAuthenticate(request, callback);
        }

        // ExternalAuth signs in a user with an external token from a social login
        public override void ExternalAuth(ExternalToken externalToken, Callback callback)
        {
            var request = m_Client.ExternalTokenAuth(new ExternalTokenAuthRequest
            {
                accessToken = externalToken.accessToken,
                idToken = externalToken.idToken,
                idProvider = externalToken.idProvider,
                redirectUri = externalToken.redirectUri,
                authCode = externalToken.authCode,
                clientId = externalToken.clientId,
                openid = externalToken.openid,
            });
            DoAuthenticate(request, callback);
        }

        // RefreshAccessToken refreshes the access token for authorization
        public override void RefreshAccessToken(Callback callback)
        {
            // Call token endpoint
            var request = m_Client.GetToken(new TokenRequest
            {
                client_id = settings.OAuthClientId,
                grant_type = "refresh_token",
                refresh_token = m_State.refreshToken
            });

            var resp = request.Send();
            resp.completed += op =>
            {
                HandleOAuthTokenResponse(request, callback);
            };
        }
        
        // Link updates the user with an additional login method
        public override void Link(ExternalToken externalToken, Callback callback)
        {
            var request = m_Client.LinkExternalId(m_State.accessToken, new ExternalTokenAuthRequest
            {
                accessToken = externalToken.accessToken,
                idToken = externalToken.idToken,
                idProvider = externalToken.idProvider,
                redirectUri = externalToken.redirectUri,
                authCode = externalToken.authCode,
                clientId = externalToken.clientId,
            });
            
            DoAuthenticateUpdate(request, true, callback);
        }

        public override void LinkSmsCode(string userId, string code, string verificationId, Callback callback)
        {
            var request = m_Client.LinkSmsCode(m_State.accessToken, new LinkSmsCodeRequest
            {
                userId = userId,
                code = code,
                verificationId = verificationId
            });
            
            DoAuthenticateUpdate(request, true, callback);
        }

        // Unlink updates the user by removing a login method
        public override void Unlink(string[] idProviders, Callback callback)
        {
            if (idProviders.Contains("apple.com"))
            {
                idProviders = idProviders.Append("oidc.apple.com").ToArray();
            }
            
            var request = m_Client.UnlinkExternalId(m_State.accessToken, new UnlinkExternalIdRequest
            {
                idProviders = idProviders,
            });
            DoAuthenticateUpdate(request, false, callback);
        }
        
        // ResetPassword allows the user to change their password
        public override void ResetPassword(string email, Callback callback)
        {
            var request = m_Client.ResetPassword(new ResetPasswordRequest
            {
                email = email,
            });
            
            var result = request.Send();
            result.completed += op =>
            {
                var resp = request.HandleResponse();
                if (resp.error != null)
                {
                    SendErrorCallback(resp.error, callback);
                    return;
                };
                callback?.Invoke(new IdentityCallbackArgs
                {
                    userInfo = null,
                    accessToken = null,
                    error = null,
                    subsystem = this
                });
            };
        }

        public override void CreateCredential(string email, string password, Callback callback)
        {
            var request = m_Client.CreateCredential(m_State.accessToken, new CreateCredentialsRequest
            {
                email = email,
                password = password,
            });
            DoAuthenticateUpdate(request, true, callback);
        }

        public override void ChangePassword(string password, string newPassword, Callback callback)
        {
            var request = m_Client.ChangePassword(m_State.accessToken, new ChangePasswordRequest
            {
                password = password,
                newPassword = newPassword,
            });
            DoAuthenticateUpdate(request, false, callback);
        }

        public override void UpdateUser(UserInfo userInfo, Callback callback)
        {
            var request = m_Client.UpdateUser(m_State.accessToken, new UpdateUserRequest
            {
                id = userInfo.userId,
                displayName = userInfo.displayName,
            });
            DoUserResponse(request, callback);
        }
        
        // GetUser fetches the information for a given user
        public override void GetUser(string userId, Callback callback)
        {
            var request = m_Client.GetUser(m_State.accessToken, new GetUserRequest
            {
                id = userId,
            });
            DoUserResponse(request, callback);
        }

        private ExternalIdInfo[] convertExternalIdList(ExternalId[] externalIds)
        {
            if (externalIds == null)
            {
                return new ExternalIdInfo[0];
            }
            
            ExternalIdInfo[] externalIdInfos = new ExternalIdInfo[externalIds.Length];
            var index = 0;
            foreach (ExternalId id in externalIds)
            {
                var externalIdInfo = convertExternalIdToExternalIdInfo(id);
                externalIdInfos[index] = externalIdInfo;
                index++;
            }

            return externalIdInfos;
        }

        private ExternalIdInfo convertExternalIdToExternalIdInfo(ExternalId externalId)
        {
            var result = new ExternalIdInfo
            {
                providerId = externalId.providerId,
                externalId = externalId.externalId,
                displayName = externalId.displayName,
                email = externalId.email,
                phoneNumber = externalId.phoneNumber
            };

            return result;
        }
        
        // VerifyEmail initializes the email verification process
        public override void VerifyEmail(string email, Callback callback)
        {
            var request = m_Client.VerifyEmail(m_State.accessToken, new VerifyEmailRequest
            {
                email = email,
            });
            
            var result = request.Send();
            result.completed += op =>
            {
                var resp = request.HandleResponse();
                if (resp.error != null)
                {
                    SendErrorCallback(resp.error, callback);
                    return;
                };
                callback?.Invoke(new IdentityCallbackArgs
                {
                    userInfo = null,
                    accessToken = null,
                    error = null,
                    subsystem = this
                });
            };
        }

        private void LoadPersistedTokens()
        {
            m_Client = new IdentityClient(settings.APIBaseUrl, settings.IDDomainID);
            if (persistentService != null)
            {
                m_State.anonymousToken = persistentService.ReadValue(settings.AnonymousTokenPersistKey);
                m_State.refreshToken = persistentService.ReadValue(settings.RefreshTokenPersistKey);
            }
        }

        // DoUserResponse executes http requests and handles the returned user info.
        private void DoUserResponse(HttpRequest<UserResponse> request, Callback callback)
        {
            var result = request.Send();
            result.completed += op =>
            {
                var resp = request.HandleResponse();
                if (resp.error != null)
                {
                    SendErrorCallback(resp.error, callback);
                    return;
                };
                
                var callbackArg = new IdentityCallbackArgs
                {
                    userInfo = new UserInfo
                    {
                        userId = resp.result.id,
                        email = resp.result.email,
                        displayName = resp.result.displayName,
                        emailVerified = resp.result.emailVerified,
                        disabled = resp.result.disabled,
                        externalIds = convertExternalIdList(resp.result.externalIds),
                        isAnonymous = (resp.result.externalIds?.Length ?? 0) == 0,
                    },
                    accessToken = m_State.accessToken,
                    subsystem = this,
                };

                callback?.Invoke(callbackArg);
            };
        }

        // DoAuthenticateUpdate executes http requests and update the state of the user.
        // It doesn't assume a response that has idToken.
        private void DoAuthenticateUpdate(HttpRequest<AuthenticationResponse> request, bool becomingFullUser, Callback callback)
        {
            var result = request.Send();
            result.completed += op =>
            {
                var resp = request.HandleResponse();
                if (resp.error != null)
                {
                    SendErrorCallback(resp.error, callback);
                    return;
                }

                if (becomingFullUser && m_State.isAnonymouslySignedIn)
                {
                    // The user has been updated to have a login method. Clear out the anonymous token.
                    m_State.anonymousToken = null;
                    m_State.isAnonymouslySignedIn = false;
                    PersistTokens();
                }
                
                callback(new IdentityCallbackArgs
                {
                    userInfo = new UserInfo
                    {
                        userId = resp.result.user.id,
                        email = resp.result.user.email,
                        displayName = resp.result.user.displayName ?? resp.result.user.email,
                        emailVerified = resp.result.user.emailVerified,
                        externalId = null,
                        isAnonymous = false, 
                    },
                    accessToken = m_State.accessToken,
                    error = null,
                    subsystem = this,
                });
            };
        }

        // DoAuthenticate executes HTTP requests and stores the authentication state of the user
        private void DoAuthenticate(HttpRequest<AuthenticationResponse> request, Callback callback)
        {
            var result = request.Send();
            result.completed += op =>
            {
                var resp = request.HandleResponse();
                if (resp.error != null)
                {
                    SendErrorCallback(resp.error, callback);
                    return;
                }

                m_State.authnIdToken = resp.result.idToken;
                
                if (m_State.authnIdToken != null)
                {
                    Authorize(callback);
                }
                else
                {
                    // Error without token
                    SendErrorCallback(new Error()
                    {
                        errorClass = ErrorClass.ApiError,
                        type = "AUTHN_TOKEN_MISSING",
                        message = "Authentication token is not returned from the server."
                    }, callback);
                }
            };
        }

        public override void PhoneLogin(string smsCodeText, string verificationIdText, Callback callback)
        {
            var request = m_Client.SmsCodeAuth(new SmsCodeAuthRequest
            {
                code = smsCodeText,
                verificationId = verificationIdText
            });
            DoAuthenticate(request, callback);
        }

        public override void RequestSmsCode(string phoneNumber, CreateSmsCodeCallBack callback)
        {
            var request = m_Client.RequestSmsCode(new CreateCodeRequest
            {
                phoneNumber = phoneNumber
            });
            
            var result = request.Send();
            result.completed += op =>
            {
                var resp = request.HandleResponse();
                var callbackArgs = new CreateSmsCodeCallbackArgs
                {
                    error = resp.error,
                    verificationId = resp.result?.verificationId
                };

                callback(callbackArgs);
            };
        }

        private void Authorize(Callback callback)
        {
            var authRequest = m_Client.Authorize(new AuthorizeRequest
            {
                client_id = settings.OAuthClientId,
                scope = SanitizeScope(settings.OAuthScopes + " platform/" + Application.platform),
                id_token = m_State.authnIdToken,
                response_type = "code",
                state = UnityPlayerIdentityUtility.RandomBase64String(8),
                nonce = UnityPlayerIdentityUtility.RandomBase64String(8),
                code_challenge = GenerateCodeChallenge(),
                code_challenge_method = "S256",
            });

            var result = authRequest.Send();
            result.completed += authOp =>
            {
                var authResp = authRequest.HandleResponse();
                if (authResp.error != null)
                {
                    SendErrorCallback(authResp.error, callback);
                    return;
                }

                // Call token endpoint
                var tokenRequest = m_Client.GetToken(new TokenRequest
                {
                    client_id = settings.OAuthClientId,
                    grant_type = "authorization_code",
                    code = authResp.result.code,
                    code_verifier = m_State.codeVerifier
                });

                var tokenResult = tokenRequest.Send();
                tokenResult.completed += tokenOp => {
                    HandleOAuthTokenResponse(tokenRequest, callback);
                }; 
            };
        }

        private void HandleOAuthTokenResponse(HttpRequest<TokenResponse> tokenRequest, Callback callback)
        {
            var resp = tokenRequest.HandleResponse();
            if (resp.error != null)
            {
                // Error handling: If the error is due to invalid token, clear the token from persistent storage
                if (IsTokenError(resp.error))
                {
                    Utils.Logger.Info("Got invalid token error, clear the cached refresh token.");
                    m_State.refreshToken = null;
                    PersistTokens();
                }

                SendErrorCallback(resp.error, callback);
                return;
            }
            SaveOAuthTokenResponse(resp.result, args =>
            {
                m_State.isAnonymouslySignedIn = args.userInfo?.externalIds?.Length == 0;
                callback?.Invoke(args);
            });
        }

        private void SendErrorCallback(Error error, Callback callback)
        {
            var result = new IdentityCallbackArgs
            {
                error = error,
                subsystem = this,
            };
            callback?.Invoke(result);
        }

        private void SaveOAuthTokenResponse(TokenResponse response, Callback callback)
        {
            var decodedToken = UnityPlayerIdentityUtility.DecodeJWT<IDToken>(response.id_token);
         
            var result = new IdentityCallbackArgs
            {
                userInfo = ExtractUserInfo(decodedToken),
                accessToken = response.access_token,
                subsystem = this,
            };
            
            m_State.refreshToken = response.refresh_token;
            m_State.accessToken = response.access_token;

            PersistTokens();
            
            // Force update user info by calling GetUser
            GetUser(result.userInfo.userId, callback);
        }

        private void SaveAnonymousAuthResponse(AuthenticationResponse response)
        {
            m_State.anonymousToken = response.sessionToken;
            m_State.authnIdToken = response.idToken;
            m_State.isAnonymouslySignedIn = true;
            PersistTokens();
        }

        private static UserInfo ExtractUserInfo(IDToken idToken)
        {
            return new UserInfo
            {
                userId = idToken.sub,
                email = idToken.email,
                emailVerified = idToken.email_verified,
                displayName = idToken.name ?? idToken.email,
                signInProviderId = idToken.sign_in_provider,
                isAnonymous = idToken.sign_in_provider == "anonymous"
            };
        }
        
        private string GenerateCodeChallenge()
        {
            m_State.codeVerifier = UnityPlayerIdentityUtility.RandomBase64String(36);
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(m_State.codeVerifier));
                return UnityPlayerIdentityUtility.Base64EncodeUrlSafe(bytes);
            }
        }

        private bool IsTokenError(Error error)
        {
            // Treat all UserErrors as token errors and retry the login
            return error.errorClass == ErrorClass.UserError;
        }
        
        private void PersistTokens()
        {
            if (persistentService != null)
            {
                persistentService.SetValue(settings.AnonymousTokenPersistKey, m_State.anonymousToken);
                if (settings.PersistRefreshToken)
                {
                    persistentService.SetValue(settings.RefreshTokenPersistKey, m_State.refreshToken);
                }
                persistentService.Save();
            }
        }

        private static string SanitizeScope(string scope)
        {
            string[] scopes = scope.Split(new []{' ', '\t', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", scopes);
        }
    }
    
}
