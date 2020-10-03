using System;
using System.Collections;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UnityEngine.PlayerIdentity.UI
{
    public class FormValidator
    {
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return false;
            }

            return phoneNumber.StartsWith("+") && phoneNumber.Length >= 7; //todo validation
        }
        
        public static bool IsValidSmsCode(string smsCode)
        {
            if (string.IsNullOrEmpty(smsCode))
            {
                return false;
            }
            string pattern = @"\d+";
            System.Text.RegularExpressions.Regex validator = new System.Text.RegularExpressions.Regex(pattern);
            return validator.IsMatch(smsCode.Trim());
        }
    }
}