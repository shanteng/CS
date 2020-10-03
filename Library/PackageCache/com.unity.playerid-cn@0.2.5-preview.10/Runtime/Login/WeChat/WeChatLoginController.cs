using System;
using UnityEngine.PlayerIdentity;

namespace UnityEngine.PlayerIdentity.WeChat
{
    public class WeChatLoginController : MonoBehaviour
    {
//        private void OnEnable()
//        {
//            Application.logMessageReceived += LogCallback;
//        }
//        
//        //Called when there is an exception
//        void LogCallback(string condition, string stackTrace, LogType type)
//        {
//            Debug.Log("WeChatLoginController");
//            Debug.Log(condition);
//            Debug.Log(stackTrace);
//            Debug.Log(type);
//        }
//
//        private void OnDisable()
//        {
//            Application.logMessageReceived -= LogCallback;
//        }

        public void OnClick()
        {
            Debug.Log(WeChatPlayerIdentitySubsystem.SubsystemId);
            PlayerIdentityManager.Current.ExternalLogin(WeChatPlayerIdentitySubsystem.SubsystemId);
        }
    }
}