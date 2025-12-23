using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.TextureResizer;
using UnityEngine;
using UnityEngine.Networking;
using WebP;
using Logger = FunticoGamesSDK.Logging.Logger;
using Object = UnityEngine.Object;

namespace FunticoGamesSDK.AssetsProvider
{
    public class UrlAssetProvider
    {
        public static string Domain = APIConstants.FUNTICO_BASE;
        public const string OriginHeaderKey = "Origin";
        private const int START_DELAY_MS = 1000;
        private const int MAX_DELAY_MS = 60000;
        private const int MAX_CACHE_SIZE = 100;

        private readonly LimitedSizeDictionary<string, Texture2D> _cache = new(MAX_CACHE_SIZE);
        private readonly Dictionary<string, Task> _currentlyLoading = new();

        public UrlAssetProvider()
        {
            _cache.OnKeyEvicted += DestroyTexture;
        }

        public async UniTask<Texture2D> LoadTexture(string address, TextureResizeOptions resizeOptions, LogType logType,
            bool addToCache = true, bool forceLoad = false)
        {
            if (!forceLoad && IsCached(address, out var result))
                return result;

            if (_currentlyLoading.TryGetValue(address, out var cachedLoadTask))
            {
                await cachedLoadTask;
                return _cache.TryGetValue(address, out var cachedTexture) ? cachedTexture : null;
            }

            var loadTask = LoadOriginalTexture(address, logType);
            if (addToCache) _currentlyLoading.Add(address, loadTask);
            var texture = await loadTask;
            texture = TuneTexture(texture, resizeOptions);
            if (addToCache)
            {
                AddToCache(address, texture);
                _currentlyLoading.Remove(address);
            }

            return texture;
        }

        public bool IsCached(string address, out Texture2D texture) => _cache.TryGetValue(address, out texture);

        public void AddToCache(string address, Texture2D texture)
        {
            if (address.IsNullOrWhitespace() || texture == null)
                return;

            texture.name = $"__UrlAssetsProviderCache_{address}";
            _cache[address] = texture;
        }

        public static Texture2D ConvertWebpToTexture2D(byte[] webpData) =>
            Texture2DExt.CreateTexture2DFromWebP(webpData, false, true, out _);

        private Texture2D TuneTexture(Texture2D originalTexture, TextureResizeOptions resizeOptions)
        {
            if (resizeOptions == null || originalTexture == null)
                return originalTexture;

            switch (resizeOptions.mode)
            {
                case TextureResizeMode.None:
                case TextureResizeMode.KeepAspectRatio
                    when resizeOptions.aspectConstraint is AspectRatioConstraint.Height &&
                         originalTexture.height <= resizeOptions.targetHeight:
                case TextureResizeMode.KeepAspectRatio
                    when resizeOptions.aspectConstraint is AspectRatioConstraint.Width &&
                         originalTexture.width <= resizeOptions.targetWidth:
                case TextureResizeMode.KeepAspectRatio
                    when resizeOptions.aspectConstraint is AspectRatioConstraint.Auto &&
                         originalTexture.width <= resizeOptions.targetWidth && originalTexture.height <= resizeOptions.targetHeight:
                    return originalTexture;
            }

            // Resize with utility
            Texture2D resized = TextureResizerUtility.ResizeTexture(originalTexture, resizeOptions);

            // Destroy original if resized is a new instance
            if (resized != originalTexture)
                Object.Destroy(originalTexture);

            return resized;
        }

        private async Task<Texture2D> LoadOriginalTexture(string address, LogType logType)
        {
            var currentDelay = START_DELAY_MS;
            while (currentDelay <= MAX_DELAY_MS)
            {
                using var www = UnityWebRequestTexture.GetTexture(address);
                www.SetRequestHeader(OriginHeaderKey, Domain);
#if DEVELOP
                Logger.Log(address);
#endif
                try
                {
                    await www.SendWebRequest();
                }
                catch
                {
                    Logger.Log($"Error while trying to load texture on {address}");
                    using UnityWebRequest uwr_webp = UnityWebRequest.Get(address);
                    uwr_webp.SetRequestHeader(OriginHeaderKey, Domain);
                    await uwr_webp.SendWebRequest();

                    switch (uwr_webp.result)
                    {
                        case UnityWebRequest.Result.Success:
                            var texture = ConvertWebpToTexture2D(uwr_webp.downloadHandler.data);
                            return texture;
                        case UnityWebRequest.Result.ConnectionError:
#if DEVELOP
                        Logger.Log($"Connection lost while trying to send request to {address}", logType);
#endif
                            await UniTask.Delay(currentDelay);
                            currentDelay *= 2;
                            continue;
                        default:
                            return null;
                    }
                }

                switch (www.result)
                {
                    case UnityWebRequest.Result.Success:
                        var texture = DownloadHandlerTexture.GetContent(www);
                        return texture;
                    case UnityWebRequest.Result.ConnectionError:
#if DEVELOP
                        Logger.Log($"Connection lost while trying to send request to {address}", logType);
#endif
                        await UniTask.Delay(currentDelay);
                        currentDelay *= 2;
                        continue;
                    default:
                        return null;
                }
            }

            return null;
        }

        private void DestroyTexture(string url, Texture2D texture) => Object.Destroy(texture);
    }
}