using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Logger = FunticoGamesSDK.Logging.Logger;
using Object = UnityEngine.Object;

namespace FunticoGamesSDK.AssetsProvider
{
    public class ResourcesAssetProvider
    {
        public async UniTask<T> LoadAsync<T>(string address, LogType logType) where T : Object
        {
            try
            {
                var loaded = await Resources.LoadAsync<T>(address);
                return (T) loaded;
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString(), logType);
                return null;
            }
        }
        public T Load<T>(string address, LogType logType) where T : Object
        {
            try
            {
                return Resources.Load<T>(address);
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString(), logType);
                return null;
            }
        }
    }
}