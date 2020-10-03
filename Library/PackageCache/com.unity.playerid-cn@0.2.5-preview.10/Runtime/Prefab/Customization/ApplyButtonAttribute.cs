using System;

namespace UnityEngine.PlayerIdentity.UI.Customizer
{
    public class ApplyButtonAttribute : PropertyAttribute
    {
        public string CallbackName
        {
            get;
            private set;
        }

        public string DelegateName
        {
            get;
            private set;
        }

        public Type CustomizedType
        {
            get;
            private set;
        }

        public ApplyButtonAttribute(string callbackName, string delegateName, Type customizedType)
        {
            CallbackName = callbackName;
            CustomizedType = customizedType;
            DelegateName = delegateName;
        }
    }
}