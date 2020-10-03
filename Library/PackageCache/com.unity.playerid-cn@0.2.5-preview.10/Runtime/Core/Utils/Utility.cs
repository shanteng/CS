using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityEngine.PlayerIdentity.Utils
{
    internal static class Utility
    {
        

        internal static T DecodeJWT<T>(string jwtToken)
        {
            if (jwtToken == null) return default(T);

            var splitted = jwtToken.Split('.');
            if (splitted.Length != 3)
            {
                throw new ArgumentException("jwtToken");
            }
            var payload = Base64Decode(splitted[1]);
            return JsonUtility.FromJson<T>(payload);
        }

        internal static string Base64Decode(string base64)
        {
            int padding = base64.Length % 4;
            if (padding > 0)
            {
                base64 += new string('=', 4 - padding);
            }
            base64 = base64.Replace('-', '+').Replace('_', '/');
            byte[] decodedBytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(decodedBytes);
        }

        private static char[] base64Padding = {'='};

        internal static string Base64EncodeUrlSafe(byte[] byteArray)
        {
            return Base64ToUrlSafe(Convert.ToBase64String(byteArray));
        }

        internal static string UrlEncode(IDictionary<string, string> parameters)
        {
            if (parameters.Count == 0) return string.Empty;

            var result = new StringBuilder();
            foreach (var entry in parameters)
            {
                if (!string.IsNullOrEmpty(entry.Value))
                {
                    result.Append(entry.Key)
                        .Append("=")
                        .Append(UnityWebRequest.EscapeURL(entry.Value))
                        .Append("&");
                }
            }
            return result.ToString(0, result.Length - 1);
        }

        private static RandomNumberGenerator secureRandom = RNGCryptoServiceProvider.Create();

        internal static string RandomBase64String(int bytes)
        {
            var byteArray = new byte[bytes];
            secureRandom.GetBytes(byteArray);
            return Base64EncodeUrlSafe(byteArray);
        }

        private static string Base64ToUrlSafe(string base64Encoded)
        {
            return base64Encoded.TrimEnd(base64Padding).Replace('+', '-').Replace('/', '_');
        }

        // HttpUtility does not work on .Net_4 (neither mono or il2cpp)
        internal static NameValueCollection ParseQueryString (string query)
        {
            return ParseQueryString (query, Encoding.UTF8);
        }

        internal static NameValueCollection ParseQueryString (string query, Encoding encoding)
        {
            if (query == null)
                throw new ArgumentNullException ("query");
            if (encoding == null)
                throw new ArgumentNullException ("encoding");
            if (query.Length == 0 || (query.Length == 1 && query[0] == '?'))
                return new NameValueCollection ();
            if (query[0] == '?')
                query = query.Substring (1);
				
            NameValueCollection result = new NameValueCollection ();
            ParseQueryString (query, encoding, result);
            return result;
        }
        
        internal static void ParseQueryString (string query, Encoding encoding, NameValueCollection result)
        {
            if (query.Length == 0)
                return;

            string decoded = query;
            int decodedLength = decoded.Length;
            int namePos = 0;
            bool first = true;
            while (namePos <= decodedLength) {
                int valuePos = -1, valueEnd = -1;
                for (int q = namePos; q < decodedLength; q++) {
                    if (valuePos == -1 && decoded [q] == '=') {
                        valuePos = q + 1;
                    } else if (decoded [q] == '&') {
                        valueEnd = q;
                        break;
                    }
                }

                if (first) {
                    first = false;
                    if (decoded [namePos] == '?')
                        namePos++;
                }
				
                string name, value;
                if (valuePos == -1) {
                    name = null;
                    valuePos = namePos;
                } else {
                    name = UnityWebRequest.UnEscapeURL((decoded.Substring (namePos, valuePos - namePos - 1)), encoding);
                }
                if (valueEnd < 0) {
                    namePos = -1;
                    valueEnd = decoded.Length;
                } else {
                    namePos = valueEnd + 1;
                }
                value = UnityWebRequest.UnEscapeURL (decoded.Substring (valuePos, valueEnd - valuePos), encoding);

                result.Add (name, value);
                if (namePos == -1)
                    break;
            }
        }

    }
}
