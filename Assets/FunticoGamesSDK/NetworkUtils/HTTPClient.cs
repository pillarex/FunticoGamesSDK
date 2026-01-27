using System;
using System.Text;
using System.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.AuthDataProviders;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Logger = FunticoGamesSDK.Logging.Logger;
#if SERVER || UNITY_SERVER
using FunticoGamesSDK.Encryption;
#endif

namespace FunticoGamesSDK.NetworkUtils
{
    public static class HTTPClient
    {
        private static string _sessionId;
        private static string _publicGameKey;
        private static string _privateGameKey;
        private static IErrorHandler _errorHandler;
        private static IAuthDataProvider _authDataProvider;

        public static void Setup(string publicGameKey, string privateGameKey, string sessionId, IAuthDataProvider authDataProvider, IErrorHandler errorHandler)
        {
            _publicGameKey = publicGameKey;
            _privateGameKey = privateGameKey;
            _authDataProvider = authDataProvider;
            _errorHandler = errorHandler;
            _sessionId = sessionId;
        }

        public static async Task<bool> Get_Short(string endPoint, Action<UnityWebRequest> errorHandler = null, string tokenToUse = null)
        {
            using UnityWebRequest getRequest = CreateRequest(endPoint, tokenToUse);
            getRequest.SendWebRequest();

            while (!getRequest.isDone) await DelayAsync(0.01f);

            if (!string.IsNullOrEmpty(getRequest.downloadHandler.text))
            {
                Logger.LogDedicated($"{getRequest.method} {getRequest.responseCode} {getRequest.downloadHandler.text}");
            }

            if (getRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    return true;
                }
                catch (Exception e)
                {
                    Logger.LogErrorDedicated($"{getRequest.method} {getRequest.responseCode} {e.Message}");
                    return false;
                }
            }
            else
            {
                try
                {
                    errorHandler?.Invoke(getRequest);
                    Logger.LogErrorDedicated($"{getRequest.method} {getRequest.responseCode} {getRequest.downloadHandler.text}");
                    return false;
                }
                catch (Exception e)
                {
                    Logger.LogErrorDedicated($"{getRequest.method} {getRequest.responseCode} {e.Message}");
                    return false;
                }
            }
        }
        
        public static async Task<Tuple<bool, T>> Get<T>(string endPoint, Action<UnityWebRequest> errorHandler = null, string tokenToUse = null)
        {
            using UnityWebRequest getRequest = CreateRequest(endPoint, tokenToUse);
            getRequest.SendWebRequest();

            while (!getRequest.isDone) await DelayAsync(0.01f);
            
            if (!string.IsNullOrEmpty(getRequest.downloadHandler.text) )
            {
                Logger.LogDedicated($"{getRequest.method} {getRequest.responseCode} {getRequest.downloadHandler.text}");
            }

            if (getRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    return new Tuple<bool, T>(true, JsonConvert.DeserializeObject<T>(getRequest.downloadHandler.text));
                }
                catch (Exception e)
                {
                    Logger.LogErrorDedicated($"{getRequest.method} {getRequest.responseCode} {e.Message}");
                    return new Tuple<bool, T>(false, default);
                }
            }
            else
            {
                try
                {
                    errorHandler?.Invoke(getRequest);
                    Logger.LogErrorDedicated($"{getRequest.method} {getRequest.responseCode} {getRequest.downloadHandler.text}");
                    return new Tuple<bool, T>(false, JsonConvert.DeserializeObject<T>(getRequest.downloadHandler.text));
                }
                catch (Exception e)
                {
                    Logger.LogErrorDedicated($"{getRequest.method} {getRequest.responseCode} {e.Message}");
                    return new Tuple<bool, T>(false, default);
                }
            }
        }

        public static async Task<Tuple<bool, T>> Post<T>(string endPoint, object data, Action<UnityWebRequest> errorHandler = null, string tokenToUse = null)
        {
            using UnityWebRequest getRequest = CreateRequest(endPoint, tokenToUse, RequestType.POST, data);
            getRequest.SendWebRequest();

            while (!getRequest.isDone) await DelayAsync(0.01f);
            
            if (!string.IsNullOrEmpty(getRequest.downloadHandler.text))
            {
                Logger.LogDedicated($"{getRequest.method} {getRequest.responseCode} {getRequest.downloadHandler.text}");
            }

            if (getRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    return new Tuple<bool, T>(true, JsonConvert.DeserializeObject<T>(getRequest.downloadHandler.text));
                }
                catch (Exception e)
                {
                    Logger.LogErrorDedicated($"{getRequest.method} {getRequest.responseCode} {e.Message}");
                    return new Tuple<bool, T>(false, default);
                }
            }
            else
            {
                try
                {                    
                    errorHandler?.Invoke(getRequest);
                    Logger.LogErrorDedicated($"{getRequest.method} {getRequest.responseCode} {getRequest.downloadHandler.text}");
                    return new Tuple<bool, T>(false, JsonConvert.DeserializeObject<T>(getRequest.downloadHandler.text));
                }
                catch (Exception e)
                {
                    Logger.LogErrorDedicated($"{getRequest.method} {getRequest.responseCode} {e.Message}");
                    return new Tuple<bool, T>(false, default);
                }
            }
        }
        
        public static async Task<bool> Post_Short(string endPoint, object data, string tokenToUse = null)
        {
            using UnityWebRequest getRequest = CreateRequest(endPoint, tokenToUse, RequestType.POST, data);
            getRequest.SendWebRequest();

            while (!getRequest.isDone) await DelayAsync(0.01f);
            
            if (!string.IsNullOrEmpty(getRequest.downloadHandler.text))
            {
                Logger.LogDedicated($"{getRequest.method} {getRequest.responseCode} {getRequest.downloadHandler.text}");
            }
            
            if(getRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    return true;
                }
                catch (Exception e)
                {
                    Logger.LogErrorDedicated($"{getRequest.method} {getRequest.responseCode} {e.Message}");
                    return false;
                }
            }
            else
            {
                try
                {
                    Logger.LogErrorDedicated($"{getRequest.method} {getRequest.responseCode} {getRequest.downloadHandler.text}");
                    return false;
                }
                catch (Exception e)
                {
                    Logger.LogErrorDedicated($"{getRequest.method} {getRequest.responseCode} {e.Message}");
                    return false;
                }
            }
        }

        private static async Task DelayAsync(float secondsDelay)
        {
            float startTime = Time.time;
            while (Time.time < startTime + secondsDelay) await Task.Yield();
        }

        public static async Task<string> Post(string endPoint, object data, string tokenToUse = null)
        {
            using UnityWebRequest getRequest = CreateRequest(endPoint, tokenToUse, RequestType.POST, data);
            getRequest.SendWebRequest();

            while (!getRequest.isDone) await DelayAsync(0.01f);
            
            Logger.LogDedicated("[HTTP] Response code : " + getRequest.responseCode);

            if (getRequest.responseCode != 200)
            {
                return "Error:" + getRequest.downloadHandler.text;
            }
                
            return getRequest.downloadHandler.text;
        }

        private static UnityWebRequest CreateRequest(string path, string tokenToUse = null, RequestType type = RequestType.GET, object data = null)
        {
            Logger.LogDedicated($"POST_SHORT to {path}");
            if (data != null)
                Logger.LogDedicated($"POST_SHORT with data: {JsonConvert.SerializeObject(data)}");
            
            UnityWebRequest request = new UnityWebRequest(path, type.ToString());
            
            if (data != null)
            {
                var bodyRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.certificateHandler = new CertificateWhore();

            ConfigureRequestHeaders(request, tokenToUse);


            return request;
        }

        private static void ConfigureRequestHeaders(UnityWebRequest request, string tokenToUse = null)
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-User-Key", _publicGameKey);
#if SERVER || UNITY_SERVER
            request.SetRequestHeader("X-Server-Key", HashUtils.HashString(_privateGameKey, _publicGameKey));
            request.SetRequestHeader("X-Server-Session-Id", _sessionId);
#else
            request.SetRequestHeader("X-Client-Session-Id", _sessionId);
#endif

            tokenToUse ??= _authDataProvider.GetUserToken();
            if (tokenToUse != null) request.SetRequestHeader("Authorization", $"Bearer {tokenToUse}");
        }

        public static void DefaultErrorHandler(UnityWebRequest www, string defaultErrorText = null)
        {
            var text = GetErrorText(www) ?? defaultErrorText;
            ShowError(text);
        }

        public static string GetErrorText(UnityWebRequest www)
        {
            string text = null;
            if (www.responseCode < 500)
            {
                try
                {
                    text = JsonConvert.DeserializeObject<ErrorResponse>(www.downloadHandler.text).Error;
                    if (text.IsNullOrWhitespace()) text = JsonConvert.DeserializeObject<Error>(www.downloadHandler.text).Message;
                }
                catch (Exception e)
                {
                    text = www.downloadHandler.text;
                }
            }

            return text;
        }

        private static void ShowError(string text) => _errorHandler.ShowError(text);

        private enum RequestType
        {
            GET = 0,
            POST = 1,
        }
    }
}