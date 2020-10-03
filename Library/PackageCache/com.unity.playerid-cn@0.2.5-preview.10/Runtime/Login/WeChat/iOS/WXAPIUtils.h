//
//  wxUtils.h
//  unityiostest
//
//  Created by playerid on 11/26/19.
//  Copyright © 2019 Unity. All rights reserved.
//

#ifndef WXAPIUtils_h
#define WXAPIUtils_h

typedef struct {
    const char * code;
    const char * errMsg;
    int errCode;
} CodeInfo;

typedef void (*OnGetCodeFromWeChat) (CodeInfo);

@interface WXAPIUtils : NSObject
+ (void) registerApp;
+ (void) loginWeChat;
+ (void) setCallBack:(OnGetCodeFromWeChat)cb;
+ (OnGetCodeFromWeChat) getCallBack;
+ (NSString *) getWxLoginState;
@end

#endif /* WXAPIUtils_h */
