using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

using DeJson = UnityEngine.PlayerIdentity.Utils.DeJson;

namespace UnityEngine.PlayerIdentity.UnityUserAuth
{
    internal class HttpRequest<T> where T : IApiResponseBase, new()
    {
        private UnityWebRequest m_WebRequest;
        private ResultParser m_ResultParser;

        private ParsedResult m_Result;

        public struct ParsedResult
        {
            public T result;
            public Error error;
        }

        public delegate ParsedResult ResultParser(UnityWebRequest request, string responseBody);

        public HttpRequest(UnityWebRequest request, ResultParser resultParser)
        {
            m_WebRequest = request;
            m_ResultParser = resultParser;
        }

        public UnityWebRequestAsyncOperation Send()
        {
            return m_WebRequest.SendWebRequest();
        }

        /// <summary>
        /// SendForEditor is needed to work with com.unity.editorcoroutines@0.0.2-preview.1
        /// In this version of com.unity.editor coroutines, UnityWebRequestAsyncOperation is not treated
        /// as an AsyncOperation, so it doesn't really wait until it's completed 
        /// </summary>
        public IEnumerator SendForEditor()
        {
            var asyncOp = m_WebRequest.SendWebRequest();
            while (!asyncOp.isDone)
            {
                yield return null;
            }
        }

        public ParsedResult HandleResponse()
        {
            Utils.Logger.Info($"{m_WebRequest.responseCode} from {m_WebRequest.url}");
            if (m_WebRequest.downloadHandler.text != null)
            {
                Utils.Logger.Info(m_WebRequest.downloadHandler.text);
            }

            m_Result = new ParsedResult();
            if (m_WebRequest.isHttpError && m_WebRequest.responseCode >= 500)
            {
                m_Result.error = new Error
                {
                    errorClass = ErrorClass.NetworkError,
                    type = "INTERNAL_SERVER_ERROR",
                    message = m_WebRequest.responseCode.ToString() + ": " + m_WebRequest.downloadHandler.text,
                };
            }
            else if (m_WebRequest.isNetworkError && m_WebRequest.error != "Redirect limit exceeded")
            {
                m_Result.error = new Error
                {
                    errorClass = ErrorClass.NetworkError,
                    type = "NETWORK_ERROR",
                    message = m_WebRequest.error,
                };
            }
            else
            {
                m_Result = m_ResultParser(m_WebRequest, m_WebRequest.downloadHandler.text);
            }

            return m_Result;
        }

        public Error Error => m_Result.error;
        public T Result => m_Result.result;
    }

    internal abstract class HttpClientBase
    {
        protected string m_BaseUrl;

        public HttpClientBase(string baseUrl)
        {
            m_BaseUrl = baseUrl.TrimEnd('/');
        }

        protected UnityWebRequest RequestJson(string method, string url, IDictionary<string, string> headers, object requestBody)
        {
            var request = new UnityWebRequest(m_BaseUrl + url, method);
            request.redirectLimit = 0;

            Utils.Logger.Info($"{request.method} {request.url}");

            string body = null;

            if (requestBody != null)
            {
                body = DeJson.Serialize.From(requestBody);
                byte[] bytes = Encoding.UTF8.GetBytes(body);
                
                var uploadHandler = new UploadHandlerRaw(bytes);
                uploadHandler.contentType = "application/json";
                request.uploadHandler = uploadHandler;
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    Utils.Logger.Info($"{header.Key}: {header.Value}");
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }
            if (body != null)
            {
                Utils.Logger.Info($"\n{body}");
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            return request;
        }

        protected UnityWebRequest RequestForm(string method, string url, IDictionary<string, string> headers, string requestBody)
        {
            var request = new UnityWebRequest(m_BaseUrl + url, method) { redirectLimit = 0 };

            Utils.Logger.Info($"{request.method} {request.url}");

            byte[] bytes = Encoding.UTF8.GetBytes(requestBody);

            var uploadHandler = new UploadHandlerRaw(bytes) { contentType = "application/x-www-form-urlencoded" };
            request.uploadHandler = uploadHandler;

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    Utils.Logger.Info($"{header.Key}: {header.Value}");
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }
            if (requestBody != null)
            {
                Utils.Logger.Info($"\n{requestBody}");
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            return request;
        }

        protected HttpRequest<T>.ParsedResult ParseJsonResponse<T>(UnityWebRequest request, string text) where T : IApiResponseBase, new()
        {
            Error error = null;
            var response = default(T);

            if (request.responseCode < 400)
            {
                response = DeserializeJson<T>(request, text, ref error);
            }
            else
            {
                // Error response
                if (string.IsNullOrEmpty(text))
                {
                    error = new Error
                    {
                        errorClass = ErrorClass.ApiError,
                        type = "UNEXPECTED_RESPONSE",
                        message = request.responseCode.ToString() + ": empty response",
                    };
                }
                else
                {
                    response = DeserializeJson<T>(request, text, ref error);
                    if (error == null)
                    {
                        error = response.GetError();
                    }
                }
            }

            return new HttpRequest<T>.ParsedResult
            {
                error = error,
                result = response,
            };
        }

        protected static T DeserializeJson<T>(UnityWebRequest request, string text, ref Error error)
        {
            try
            {
                return DeJson.Deserialize.To<T>(text);
            }
            catch (Exception ex)
            {
                Utils.Logger.Error("Failed to parse json.");
                Utils.Logger.Exception(ex);
                error = new Error
                {
                    errorClass = ErrorClass.ApiError,
                    type = "UNEXPECTED_RESPONSE",
                    message = request.responseCode.ToString() + ": " + ex.Message,
                };
                return default(T);
            }
        }

    }
}
