package com.unity3d.playerididentity;

import android.content.Context;
import android.content.SharedPreferences;
import android.util.Log;

import com.tencent.mm.opensdk.modelmsg.SendAuth;
import com.tencent.mm.opensdk.openapi.IWXAPI;
import com.tencent.mm.opensdk.openapi.WXAPIFactory;

import java.util.UUID;

public class WxAPIUtils {

    private static final String TAG = "PLAYERID.WX.WxAPIUtils";
    private static Context context;


    public interface GetCodeCallback {
        void onSuccess(String code);
        void onError(int errorCode, String errorString);
    }

    private static GetCodeCallback callback;

    private static IWXAPI iwxapi;

    private static String wxAppId = null;

    public static void setWxAppId(String wxAppId) {
        Log.d(TAG, "setWxAppId: ");
        WxAPIUtils.wxAppId = wxAppId;
    }

    public static String getWxAppId() {
        return wxAppId;
    }

    public static IWXAPI getIwxapi() {
        return iwxapi;
    }

    private static void setCallback(GetCodeCallback callback) {
        WxAPIUtils.callback = callback;
    }

    static void setContext(Context ctx){
        context = ctx;
    }

    public static void createAndRegister() {
        iwxapi = WXAPIFactory.createWXAPI(context, wxAppId, false);
        iwxapi.registerApp(wxAppId);
    }

    public static void invokeSuccessCallback(String code) {
        Log.d(TAG, "invokeSuccessCallback: get code");
        if (callback != null) {
            callback.onSuccess(code);
        }
    }

    public static void invokeErrorCallback(int errorCode, String errorStr) {
        Log.d(TAG, "invokeErrorCallback: " + errorStr);
        if (callback != null) {
            callback.onError(errorCode, errorStr);
        }
    }

    public static void getCode(String scope, GetCodeCallback callback) {
        setCallback(callback);
        final SendAuth.Req req = new SendAuth.Req();
        req.scope = scope;
        req.state = refreshReqState();
        iwxapi.sendReq(req);
    }

    private static String reqState = "";

    public static String getReqState() {
        return reqState;
    }

    public static String refreshReqState() {
        reqState = UUID.randomUUID().toString();
        return reqState;
    }
}
