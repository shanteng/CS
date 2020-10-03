//
//  wxUtils.m
//  unityiostest
//
//  Created by playerid on 11/26/19.
//  Copyright © 2019 Unity. All rights reserved.
//

#import <Foundation/Foundation.h>
#include "WXApi.h"
#include "WXAPIUtils.h"

static OnGetCodeFromWeChat _callback = nullptr;
static NSString* WXLoginStateCache = nullptr;


@implementation WXAPIUtils

+ (void)registerApp {
    NSArray * urlTypes = [[[NSBundle mainBundle] infoDictionary] valueForKey:@"CFBundleURLTypes"];
    NSString * appID;
    for (NSDictionary * urlType in urlTypes) {
        if ([@"weixin" isEqualToString:urlType[@"CFBundleURLName"]]) {
            appID = urlType[@"CFBundleURLSchemes"][0];
        }
    }
    if (nullptr == appID) {
        NSLog(@"wx app id not set");
    } else{
        [WXApi registerApp:appID];
    }
}

+ (void)loginWeChat {
    SendAuthReq* request = [[SendAuthReq alloc] init];
    request.scope = @"snsapi_userinfo";
    WXLoginStateCache = [WXAPIUtils generateRandomString:16];
    request.state = WXLoginStateCache;
    [WXApi sendReq:request];
}

+ (NSString *) generateRandomString:(int)num {
    NSMutableString * rstr = [NSMutableString stringWithCapacity:num];
    for (int i = 0; i < num; i++) {
        [rstr appendFormat:@"%C", (unichar)('a' + arc4random_uniform(26))];
    }
    return rstr;
}


+ (void)setCallBack:(OnGetCodeFromWeChat)cb {
    _callback = cb;
}

+ (OnGetCodeFromWeChat)getCallBack {
    return _callback;
}

+ (NSString *)getWxLoginState {
    return WXLoginStateCache;
}

@end

extern "C" {

    void loginByWeChat(OnGetCodeFromWeChat callback) {
        [WXAPIUtils setCallBack:callback];
        [WXAPIUtils loginWeChat];
    }
    
    void registerWxAPI() {
        [WXAPIUtils registerApp];
    }

}
