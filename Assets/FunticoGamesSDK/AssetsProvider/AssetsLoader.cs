using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.TextureResizer;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FunticoGamesSDK.AssetsProvider
{
    public class AssetsLoader
    {
        private static AssetsLoader _instance;
        public static AssetsLoader Instance => _instance ??= new AssetsLoader();
        public readonly Sprite TicoSprite;

        private readonly ResourcesAssetProvider _resourcesAssetProvider = new ResourcesAssetProvider();
        private readonly UrlAssetProvider _urlAssetProvider = new UrlAssetProvider();

        private AssetsLoader()
        {
            TicoSprite = LoadSpriteFromResources("Tico", LogType.Error);
        }

        public static async UniTask<Sprite> LoadSpriteAsync(string address, TextureResizeOptions resizeOptions = null, LogType logType = LogType.Error)
        {
            return address.IsUrl()
                ? await LoadSpriteAsyncFromLink(address, resizeOptions, logType)
                : await LoadSpriteAsyncFromResources(address, logType);
        }

        public static async UniTask<Texture2D> LoadTextureAsync(string address, TextureResizeOptions resizeOptions = null, LogType logType = LogType.Error)
        {
            return address.IsUrl()
                ? await LoadTextureAsyncFromLink(address, resizeOptions, logType)
                : await LoadAssetAsync<Texture2D>(address, logType);
        }

        public static Sprite LoadSprite(string address, TextureResizeOptions resizeOptions = null, LogType logType = LogType.Error)
        {
            if (!address.IsUrl()) return LoadSpriteFromResources(address, logType);
            var loadSpriteAsyncFromLinkTask = LoadSpriteAsyncFromLink(address, resizeOptions, logType);
            loadSpriteAsyncFromLinkTask.Wait();
            return loadSpriteAsyncFromLinkTask.Result;
        }

        public static T LoadAsset<T>(string address, LogType logType = LogType.Error) where T : Object =>
            Instance._resourcesAssetProvider.Load<T>(address, logType);

        public static async UniTask<T> LoadAssetAsync<T>(string address, LogType logType = LogType.Error)
            where T : Object =>
            await Instance._resourcesAssetProvider.LoadAsync<T>(address, logType);

        public static bool IsCached(string address, out Texture2D texture2D)
        {
            texture2D = null;
            return address.IsUrl() && Instance._urlAssetProvider.IsCached(address, out texture2D);
        }

        public static void AddToCache(string address, Texture2D texture)
        {
            if (!address.IsUrl())
                return;

            Instance._urlAssetProvider.AddToCache(address, texture);
        }

        private static async UniTask<Sprite> LoadSpriteAsyncFromResources(string address, LogType logType) =>
            await Instance._resourcesAssetProvider.LoadAsync<Sprite>(address, logType);

        private static Sprite LoadSpriteFromResources(string address, LogType logType) =>
            Instance._resourcesAssetProvider.Load<Sprite>(address, logType);

        private static async Task<Sprite> LoadSpriteAsyncFromLink(string address, TextureResizeOptions resizeOptions,
            LogType logType = LogType.Error)
        {
            var texture2D = await LoadTextureAsyncFromLink(address, resizeOptions, logType);
            return texture2D == null ? null : Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
        }

        private static async Task<Texture2D> LoadTextureAsyncFromLink(string address, TextureResizeOptions resizeOptions,
            LogType logType = LogType.Error) => 
            await Instance._urlAssetProvider.LoadTexture(address, resizeOptions, logType);
    }
}