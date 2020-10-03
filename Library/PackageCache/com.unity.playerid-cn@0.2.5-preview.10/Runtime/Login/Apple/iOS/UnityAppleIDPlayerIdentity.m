#import <Foundation/Foundation.h>
#import <AuthenticationServices/AuthenticationServices.h>
#import "UnityAppController.h"

struct SignInWithAppleUserInfo
{
    const char * userId;
    const char * email;
    const char * displayName;

    const char * idToken;
    const char * error;

    ASUserDetectionStatus userDetectionStatus;
};

typedef void (*UnityPlayerIdentityCallback)(int result, struct SignInWithAppleUserInfo s1);
API_AVAILABLE(ios(13.0), tvos(13.0))
typedef void (*CredentialStateCallback)(ASAuthorizationAppleIDProviderCredentialState state);

API_AVAILABLE(ios(13.0), tvos(13.0))
@interface UnityAppleIDPlayerIdentity : NSObject<ASAuthorizationControllerDelegate, ASAuthorizationControllerPresentationContextProviding>

@property (nonatomic) UnityPlayerIdentityCallback loginCallback;
@property (nonatomic) CredentialStateCallback credentialStateCallback;

@end

API_AVAILABLE(ios(13.0), tvos(13.0))
static UnityAppleIDPlayerIdentity* _unityAppleIDPlayerIdentityInstance;

@implementation UnityAppleIDPlayerIdentity
{
    ASAuthorizationAppleIDRequest* request;
}

+(UnityAppleIDPlayerIdentity*)instance
{
    if (_unityAppleIDPlayerIdentityInstance == nil)
        _unityAppleIDPlayerIdentityInstance = [[UnityAppleIDPlayerIdentity alloc] init];
    return _unityAppleIDPlayerIdentityInstance;
}

-(void)startRequest
{
    if (@available(iOS 13.0, tvOS 13.0, *)) {
        ASAuthorizationAppleIDProvider* provider = [[ASAuthorizationAppleIDProvider alloc] init];
        request = [provider createRequest];
        [request setRequestedScopes: @[ASAuthorizationScopeEmail, ASAuthorizationScopeFullName]];

        ASAuthorizationController* controller = [[ASAuthorizationController alloc] initWithAuthorizationRequests:@[request]];
        controller.delegate = self;
        controller.presentationContextProvider = self;
        [controller performRequests];
    } else {
        // Fallback on earlier versions
    }
}

- (void)getCredentialState:(NSString *)userID
{
    ASAuthorizationAppleIDProvider* provider = [[ASAuthorizationAppleIDProvider alloc] init];
    [provider getCredentialStateForUserID:userID
                               completion:^(ASAuthorizationAppleIDProviderCredentialState credentialState, NSError * _Nullable error) {
        self.credentialStateCallback(credentialState);
    }];
}

-(ASPresentationAnchor)presentationAnchorForAuthorizationController:(ASAuthorizationController *)controller
{
    return _UnityAppController.window;
}

-(void)authorizationController:(ASAuthorizationController *)controller didCompleteWithAuthorization:(ASAuthorization *)authorization
{
    if (self.loginCallback)
    {
        struct SignInWithAppleUserInfo data;

        if (@available(iOS 13.0, tvOS 13.0, *)) {
            ASAuthorizationAppleIDCredential* credential = (ASAuthorizationAppleIDCredential*)authorization.credential;
            NSString* idToken = [[NSString alloc] initWithData:credential.identityToken encoding:NSUTF8StringEncoding];
            NSPersonNameComponents* name = credential.fullName;

            data.idToken = [idToken UTF8String];

            data.displayName = [[NSPersonNameComponentsFormatter localizedStringFromPersonNameComponents:name
                                                                                                   style:NSPersonNameComponentsFormatterStyleDefault
                                                                                                 options:0] UTF8String];
            data.email = credential.email != NULL ? [credential.email UTF8String] : "";
            data.userId = credential.user != NULL ? [credential.user UTF8String] : "";
            data.userDetectionStatus = credential.realUserStatus;
            data.error = "";

            self.loginCallback(1, data);
        } else {
            // Fallback on earlier versions
        }
    }
}

-(void)authorizationController:(ASAuthorizationController *)controller didCompleteWithError:(NSError *)error
{
    if (self.loginCallback)
    {
        // All members need to be set to a non-null value.
        struct SignInWithAppleUserInfo data;
        data.idToken = "";
        data.displayName = "";
        data.email = "";
        data.userId = "";
        data.userDetectionStatus = 1;
        data.error = [error.localizedDescription UTF8String];
        self.loginCallback(0, data);
    }
}

@end

void UnityAppleIDPlayerIdentity_Login(UnityPlayerIdentityCallback callback)
{
    if (@available(iOS 13.0, tvOS 13.0, *)) {
        UnityAppleIDPlayerIdentity* login = [UnityAppleIDPlayerIdentity instance];
        login.loginCallback = callback;
        [login startRequest];
    } else {
        // Fallback on earlier versions
    }
}

API_AVAILABLE(ios(13.0), tvos(13.0))
void UnityAppleIDPlayerIdentity_GetCredentialState(const char *userID, CredentialStateCallback callback)
{
    if (@available(iOS 13.0, tvOS 13.0, *)) {
        UnityAppleIDPlayerIdentity* login = [UnityAppleIDPlayerIdentity instance];
        login.credentialStateCallback = callback;
        [login getCredentialState: [NSString stringWithUTF8String: userID]];
    } else {
        // Fallback on earlier versions
    }
}
