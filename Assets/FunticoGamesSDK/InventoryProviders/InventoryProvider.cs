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

        public async UniTask<RecipesResponse> GetRecipes()
        {
            InventoryErrorResponse error = null;
            (bool success, RecipesResponse response) =
                await HTTPClient.Get<RecipesResponse>(APIConstants.Get_Inventory_Recipes, www => error = ReadError(www));
            if (!success || response == null)
            {
                _lastError = error ?? new InventoryErrorResponse
                {
                    Error = InventoryErrorCodes.RequestFailed,
                    Message = "The recipes response could not be read."
                };
                Logger.LogError($"Failed to get recipes: {_lastError.Error} {_lastError.Message}");
                return null;
            }

            _lastError = null;
            Normalize(response);
            return response;
        }

        public UniTask<CraftResult> CraftItem(string recipe, string id = null, string idempotencyKey = null)
        {
            var key = string.IsNullOrEmpty(idempotencyKey) ? Guid.NewGuid().ToString() : idempotencyKey;
            var request = new CraftItemRequest
            {
                Recipe = recipe,
                Id = string.IsNullOrEmpty(id) ? null : id,
                IdempotencyKey = key
            };
            return PostCraftAction(APIConstants.Post_Inventory_Craft, request, key, "craft");
        }

        public UniTask<CraftResult> FuseItems(string fusion, IEnumerable<long> userItemIds, string idempotencyKey = null)
        {
            var key = string.IsNullOrEmpty(idempotencyKey) ? Guid.NewGuid().ToString() : idempotencyKey;
            var request = new FuseItemsRequest
            {
                Fusion = fusion,
                UserItemIds = userItemIds != null ? new List<long>(userItemIds) : new List<long>(),
                IdempotencyKey = key
            };
            return PostCraftAction(APIConstants.Post_Inventory_Fuse, request, key, "fuse");
        }

        public async UniTask<RecentRewardsResponse> GetRecentRewards(int limit = 20, DateTime? since = null)
        {
            var query = new List<string> { $"limit={Math.Clamp(limit, 1, 100)}" };
            if (since != null)
                query.Add("since=" + UnityWebRequest.EscapeURL(since.Value.ToUniversalTime().ToString("o")));

            (bool success, RecentRewardsResponse response) =
                await HTTPClient.Get<RecentRewardsResponse>(APIConstants.WithQuery(APIConstants.Get_Recent_Rewards, query.ToArray()));
            if (!success || response == null)
            {
                Logger.LogError("Failed to get recent rewards.");
                return null;
            }

            response.Rewards ??= new List<RecentReward>();
            response.Rewards.RemoveAll(reward => reward == null);
            foreach (var reward in response.Rewards)
                reward.Items ??= new List<RecentRewardItem>();
            return response;
        }

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
        /// Sends a craft or a fusion. 200 (Succeeded), 202 (Pending) and 422 (Refused) answer with the
        /// outcome; a 422 without a Status and every other failure carry an inventory error body instead.
        /// </summary>
        private async UniTask<CraftResult> PostCraftAction(string url, object request, string idempotencyKey, string action)
        {
            CraftResult refused = null;
            InventoryErrorResponse error = null;
            (bool success, CraftResult response) = await HTTPClient.Post<CraftResult>(url, request, www =>
            {
                refused = www.responseCode == 422 ? ReadCraftResult(www.downloadHandler?.text) : null;
                if (refused == null)
                    error = ReadError(www);
            });

            var result = success ? response : refused;
            if (result == null || string.IsNullOrEmpty(result.Status))
            {
                // No error captured means the request went through but the body could not be read.
                _lastError = error ?? new InventoryErrorResponse
                {
                    Error = InventoryErrorCodes.RequestFailed,
                    Message = $"The {action} response could not be read."
                };
                Logger.LogError($"Failed to {action}: {_lastError.Error} {_lastError.Message}");
                return null;
            }

            _lastError = null;
            result.IdempotencyKey = idempotencyKey;
            result.Burned = Normalize(result.Burned);
            result.Granted = Normalize(result.Granted);
            return result;
        }

        /// <summary>The outcome carried by a 422 body, or null when the body is an error body instead.</summary>
        private static CraftResult ReadCraftResult(string body)
        {
            if (string.IsNullOrEmpty(body))
                return null;

            try
            {
                var result = JsonConvert.DeserializeObject<CraftResult>(body);
                return result != null && !string.IsNullOrEmpty(result.Status) ? result : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

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

        /// <summary>Guarantees non-null lists so callers can iterate without null checks.</summary>
        private static void Normalize(RecipesResponse response)
        {
            response.Recipes ??= new List<CraftRecipe>();
            response.Recipes.RemoveAll(recipe => recipe == null);
            foreach (var recipe in response.Recipes)
            {
                recipe.Inputs ??= new List<CraftRecipeItem>();
                recipe.Inputs.RemoveAll(line => line == null);
                recipe.Outputs ??= new List<CraftRecipeItem>();
                recipe.Outputs.RemoveAll(line => line == null);
            }

            response.Fusions ??= new List<FusionRecipe>();
            response.Fusions.RemoveAll(fusion => fusion == null);
            foreach (var fusion in response.Fusions)
            {
                fusion.Inputs ??= new List<string>();
                fusion.Results ??= new List<string>();
            }
        }

        private static List<CraftResultItem> Normalize(List<CraftResultItem> items)
        {
            items ??= new List<CraftResultItem>();
            items.RemoveAll(item => item == null);
            foreach (var item in items)
                item.UserItemIds ??= new List<long>();
            return items;
        }

        #endregion
    }
}
