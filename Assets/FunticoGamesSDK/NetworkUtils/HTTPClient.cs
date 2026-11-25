using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.AuthDataProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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

        /// <summary>
        /// Also log request and response bodies. Off by default: bodies carry the player's token and other
        /// private data. Even when on, values of token, password, key and similar fields are masked and long
        /// bodies are cut. Turn it on only while debugging, never in a build you share.
        /// </summary>
        public static bool LogBodies { get; set; }

        public static void Setup(string publicGameKey, string privateGameKey, string sessionId, IAuthDataProvider authDataProvider, IErrorHandler errorHandler)
        {
            _publicGameKey = publicGameKey;
            _privateGameKey = privateGameKey;
            _authDataProvider = authDataProvider;
            _errorHandler = errorHandler;
            _sessionId = sessionId;
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new AotSafeContractResolver()
            };
        }

        public static async Task<bool> Get_Short(string endPoint, Action<UnityWebRequest> errorHandler = null, string tokenToUse = null)
        {
            using UnityWebRequest getRequest = CreateRequest(endPoint, tokenToUse);
            getRequest.SendWebRequest();

            while (!getRequest.isDone) await DelayAsync(0.01f);

            LogResponse(getRequest);

            if (getRequest.result == UnityWebRequest.Result.Success)
            {
                return true;
            }

            try
            {
                errorHandler?.Invoke(getRequest);
                return false;
            }
            catch (Exception e)
            {
                LogException(getRequest, e);
                return false;
            }
        }

        public static async Task<Tuple<bool, T>> Get<T>(string endPoint, Action<UnityWebRequest> errorHandler = null, string tokenToUse = null)
        {
            using UnityWebRequest getRequest = CreateRequest(endPoint, tokenToUse);
            getRequest.SendWebRequest();

            while (!getRequest.isDone) await DelayAsync(0.01f);

            LogResponse(getRequest);

            if (getRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    return new Tuple<bool, T>(true, JsonConvert.DeserializeObject<T>(getRequest.downloadHandler.text));
                }
                catch (Exception e)
                {
                    LogException(getRequest, e);
                    return new Tuple<bool, T>(false, default);
                }
            }

            try
            {
                errorHandler?.Invoke(getRequest);
                return new Tuple<bool, T>(false, JsonConvert.DeserializeObject<T>(getRequest.downloadHandler.text));
            }
            catch (Exception e)
            {
                LogException(getRequest, e);
                return new Tuple<bool, T>(false, default);
            }
        }

        /// <summary>GET that returns the response body as it is, without parsing it. Null on failure.</summary>
        public static async Task<Tuple<bool, string>> GetText(string endPoint, string tokenToUse = null)
        {
            using UnityWebRequest getRequest = CreateRequest(endPoint, tokenToUse);
            getRequest.SendWebRequest();

            while (!getRequest.isDone) await DelayAsync(0.01f);

            LogResponse(getRequest);

            var success = getRequest.result == UnityWebRequest.Result.Success;
            return new Tuple<bool, string>(success, success ? getRequest.downloadHandler.text : null);
        }

        public static async Task<Tuple<bool, T>> Post<T>(string endPoint, object data, Action<UnityWebRequest> errorHandler = null, string tokenToUse = null)
        {
            using UnityWebRequest getRequest = CreateRequest(endPoint, tokenToUse, RequestType.POST, data);
            getRequest.SendWebRequest();

            while (!getRequest.isDone) await DelayAsync(0.01f);

            LogResponse(getRequest);

            if (getRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    return new Tuple<bool, T>(true, JsonConvert.DeserializeObject<T>(getRequest.downloadHandler.text));
                }
                catch (Exception e)
                {
                    LogException(getRequest, e);
                    return new Tuple<bool, T>(false, default);
                }
            }

            try
            {
                errorHandler?.Invoke(getRequest);
                return new Tuple<bool, T>(false, JsonConvert.DeserializeObject<T>(getRequest.downloadHandler.text));
            }
            catch (Exception e)
            {
                LogException(getRequest, e);
                return new Tuple<bool, T>(false, default);
            }
        }

        public static async Task<bool> Post_Short(string endPoint, object data, string tokenToUse = null)
        {
            using UnityWebRequest getRequest = CreateRequest(endPoint, tokenToUse, RequestType.POST, data);
            getRequest.SendWebRequest();

            while (!getRequest.isDone) await DelayAsync(0.01f);

            LogResponse(getRequest);

            return getRequest.result == UnityWebRequest.Result.Success;
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

            LogResponse(getRequest);

            if (getRequest.responseCode != 200)
            {
                return "Error:" + getRequest.downloadHandler.text;
            }

            return getRequest.downloadHandler.text;
        }

        private static UnityWebRequest CreateRequest(string path, string tokenToUse = null, RequestType type = RequestType.GET, object data = null)
        {
            string json = data != null ? JsonConvert.SerializeObject(data) : null;

            Logger.LogDedicated(LogBodies && json != null
                ? $"{type} {HttpLogFormat.Path(path)} body: {HttpLogFormat.Body(json, HttpLogFormat.MaxBodyLength)}"
                : $"{type} {HttpLogFormat.Path(path)}");

            UnityWebRequest request = new UnityWebRequest(path, type.ToString());

            if (json != null)
            {
                var bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.certificateHandler = new CertificateWhore();

            ConfigureRequestHeaders(request, tokenToUse);


            return request;
        }

        /// <summary>
        /// One line per finished request: method, path, status. A failure also carries the start of its
        /// body (masked) so the reason is visible; a success carries its body only with <see cref="LogBodies"/>.
        /// </summary>
        private static void LogResponse(UnityWebRequest request)
        {
            var line = $"{request.method} {HttpLogFormat.Path(request.url)} {request.responseCode}";
            var body = request.downloadHandler?.text;

            if (request.result == UnityWebRequest.Result.Success)
            {
                Logger.LogDedicated(LogBodies && !string.IsNullOrEmpty(body)
                    ? $"{line} {HttpLogFormat.Body(body, HttpLogFormat.MaxBodyLength)}"
                    : line);
                return;
            }

            var reason = !string.IsNullOrEmpty(body)
                ? HttpLogFormat.Body(body, LogBodies ? HttpLogFormat.MaxBodyLength : HttpLogFormat.MaxErrorLength)
                : request.error;
            Logger.LogErrorDedicated($"{line} {reason}");
        }

        private static void LogException(UnityWebRequest request, Exception e) =>
            Logger.LogErrorDedicated($"{request.method} {HttpLogFormat.Path(request.url)} {request.responseCode} {e.Message}");

        private static void ConfigureRequestHeaders(UnityWebRequest request, string tokenToUse = null)
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-User-Key", _publicGameKey);
#if SERVER || UNITY_SERVER
            // A build initialized with a signer has no key to derive the server key from.
            if (!string.IsNullOrEmpty(_privateGameKey))
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

    /// <summary>
    /// Makes request and response text safe to log: no query strings (they can carry passwords), masked
    /// values of secret-looking JSON fields, masked JWTs and bearer tokens, and a length cap.
    /// </summary>
    internal static class HttpLogFormat
    {
        internal const int MaxBodyLength = 2000;
        internal const int MaxErrorLength = 300;
        internal const string Mask = "***";

        // A JSON string field whose name contains one of these words (any case): "token": "...", "Password": "..." etc.
        private static readonly Regex SecretField = new Regex(
            "(\"[^\"]*(?:token|password|secret|authorization|apikey|api_key|privatekey|private_key|jwt)[^\"]*\"\\s*:\\s*)\"(?:[^\"\\\\]|\\\\.)*\"",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex Jwt = new Regex(
            "eyJ[A-Za-z0-9_-]+\\.[A-Za-z0-9_-]+\\.[A-Za-z0-9_-]*",
            RegexOptions.CultureInvariant);

        private static readonly Regex Bearer = new Regex(
            "(Bearer\\s+)[A-Za-z0-9._~+/=-]+",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>The URL without its query string.</summary>
        internal static string Path(string url)
        {
            if (string.IsNullOrEmpty(url)) return url;
            var query = url.IndexOf('?');
            return query < 0 ? url : url.Substring(0, query) + "?" + Mask;
        }

        /// <summary>The text with secrets masked, cut to <paramref name="maxLength"/> characters.</summary>
        internal static string Body(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var masked = SecretField.Replace(text, "$1\"" + Mask + "\"");
            masked = Jwt.Replace(masked, Mask);
            masked = Bearer.Replace(masked, "$1" + Mask);

            return masked.Length <= maxLength
                ? masked
                : masked.Substring(0, maxLength) + $"… ({masked.Length - maxLength} more chars)";
        }
    }

    internal static class JsonBody
    {
        public static Dictionary<string, object> Of(params (string key, object value)[] fields)
        {
            var dict = new Dictionary<string, object>(fields.Length);
            foreach (var (k, v) in fields)
                dict[k] = v;
            return dict;
        }
    }

    internal sealed class AotSafeContractResolver : DefaultContractResolver
    {
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);
            if (contract.DefaultCreator == null &&
                objectType.GetConstructor(Type.EmptyTypes) != null)
            {
                contract.DefaultCreator = () => Activator.CreateInstance(objectType);
            }
            return contract;
        }
    }
}
