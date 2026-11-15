using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.NetworkUtils;
using Newtonsoft.Json;
using UnityEngine.Networking;
using Logger = FunticoGamesSDK.Logging.Logger;

namespace FunticoGamesSDK.InventoryProviders
{
    public class InventoryProvider : IInventoryProvider
    {
        private InventoryErrorResponse _lastError;

        #region Public

        public async UniTask<InventoryResponse> GetInventory(bool includeOtherItems = false)
        {
            var url = includeOtherItems
                ? APIConstants.WithQuery(APIConstants.Get_Inventory, "all=true")
                : APIConstants.Get_Inventory;

            InventoryErrorResponse error = null;
            (bool success, InventoryResponse response) =
                await HTTPClient.Get<InventoryResponse>(url, www => error = ReadError(www));
            if (!success || response == null)
            {
                // No error captured means the request went through but the body could not be read.
                _lastError = error ?? new InventoryErrorResponse
                {
                    Error = InventoryErrorCodes.RequestFailed,
                    Message = "The inventory response could not be read."
                };
                Logger.LogError($"Failed to get inventory: {_lastError.Error} {_lastError.Message}");
                return null;
            }

            _lastError = null;
            Normalize(response);
            return response;
        }

        public InventoryErrorResponse GetLastInventoryError() => _lastError;

        public T GetInventoryItemProperties<T>(InventoryItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.PropertiesJson))
                return default;

            try
            {
                return JsonConvert.DeserializeObject<T>(item.PropertiesJson);
            }
            catch (JsonException e)
            {
                throw new JsonSerializationException(
                    $"Cannot read PropertiesJson of inventory item '{item.Name}' ({item.ItemId}) as {typeof(T).Name}: {e.Message}", e);
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Turns a failed request into an <see cref="InventoryErrorResponse"/>: the structured
        /// { Error, Message } body when the server sent one, an SDK-side code otherwise.
        /// </summary>
        private static InventoryErrorResponse ReadError(UnityWebRequest www)
        {
            if (www.responseCode == 401)
            {
                return new InventoryErrorResponse
                {
                    Error = InventoryErrorCodes.Unauthorized,
                    Message = "The player is not authenticated."
                };
            }

            var body = www.downloadHandler?.text;
            if (www.responseCode >= 400 && !string.IsNullOrEmpty(body))
            {
                try
                {
                    var error = JsonConvert.DeserializeObject<InventoryErrorResponse>(body);
                    if (error != null && !string.IsNullOrEmpty(error.Error))
                        return error;
                }
                catch (Exception)
                {
                    // Not a structured inventory error body — fall through to RequestFailed.
                }
            }

            var reason = www.responseCode > 0 ? $"HTTP {www.responseCode}" : www.error;
            return new InventoryErrorResponse
            {
                Error = InventoryErrorCodes.RequestFailed,
                Message = $"The inventory request failed: {reason}"
            };
        }

        /// <summary>Guarantees non-null lists so callers can iterate without null checks.</summary>
        private static void Normalize(InventoryResponse response)
        {
            response.Items ??= new List<InventoryItem>();
            response.Items.RemoveAll(item => item == null);
            foreach (var item in response.Items)
            {
                item.UserItemIds ??= new List<long>();
                item.Copies ??= new List<InventoryItemCopy>();
            }
        }

        #endregion
    }
}
