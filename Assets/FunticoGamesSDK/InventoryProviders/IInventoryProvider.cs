using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;

namespace FunticoGamesSDK.InventoryProviders
{
    public interface IInventoryProvider
    {
        /// <summary>
        /// Fetches the items the player owns on the Funtico platform.
        /// Always calls the server; the result is not cached.
        /// Returns null on failure; <see cref="GetLastInventoryError"/> then says why.
        /// </summary>
        /// <param name="includeOtherItems">
        /// False (default): only items that belong to this game.
        /// True: also platform-wide items and other games' consumables (check <see cref="InventoryItem.IsGameItem"/>).
        /// </param>
        UniTask<InventoryResponse> GetInventory(bool includeOtherItems = false);

        /// <summary>
        /// Returns why the last <see cref="GetInventory"/>, <see cref="GetRecipes"/>, <see cref="CraftItem"/>
        /// or <see cref="FuseItems"/> call failed, or null if it succeeded (or nothing has been fetched yet).
        /// Compare <c>Error</c> with <see cref="InventoryErrorCodes"/>.
        /// </summary>
        InventoryErrorResponse GetLastInventoryError();

        /// <summary>
        /// Deserializes the item's PropertiesJson into a game-defined type.
        /// Returns default (null for reference types) if the item is null or has no properties.
        /// Throws Newtonsoft.Json.JsonException if PropertiesJson cannot be read as <typeparamref name="T"/>.
        /// </summary>
        T GetInventoryItemProperties<T>(InventoryItem item);

        /// <summary>
        /// The crafting recipes and fusions of this game: what <see cref="CraftItem"/> and
        /// <see cref="FuseItems"/> can do. Always calls the server; the result is not cached.
        /// Returns null on failure; <see cref="GetLastInventoryError"/> then says why.
        /// </summary>
        UniTask<RecipesResponse> GetRecipes();

        /// <summary>
        /// Crafts by a fixed recipe: takes the recipe's Inputs from the player and gives its Outputs.
        /// Returns the outcome when it is Succeeded, Pending or Refused (see <see cref="CraftStatus"/>); a
        /// refusal is a normal result, not an error. On Pending, call again with the same arguments and
        /// <see cref="CraftResult.IdempotencyKey"/>. Returns null on any other failure;
        /// <see cref="GetLastInventoryError"/> then says why.
        /// </summary>
        /// <param name="recipe"><see cref="CraftRecipe.Key"/>.</param>
        /// <param name="id">
        /// Fills "{id}" in the recipe's item keys: 1-48 characters of a-z, 0-9 and _. Null for a recipe whose
        /// <see cref="CraftRecipe.HasId"/> is false.
        /// </param>
        /// <param name="idempotencyKey">
        /// Optional: 8-64 characters of A-Z, a-z, 0-9 and _ . : -. Calls with the same key are one action,
        /// done at most once. Null: the SDK generates a new key for this call.
        /// </param>
        UniTask<CraftResult> CraftItem(string recipe, string id = null, string idempotencyKey = null);

        /// <summary>
        /// Fuses copies the player picked: takes them and gives one random item of the fusion's Results.
        /// Returns like <see cref="CraftItem"/>.
        /// </summary>
        /// <param name="fusion"><see cref="FusionRecipe.Key"/>.</param>
        /// <param name="userItemIds">
        /// Exactly <see cref="FusionRecipe.Need"/> copy ids, each once, from <see cref="InventoryItem.UserItemIds"/>
        /// of items listed in the fusion's Inputs. Copies that do not fit fail with
        /// <see cref="InventoryErrorCodes.WrongItems"/>.
        /// </param>
        /// <param name="idempotencyKey">Optional, as for <see cref="CraftItem"/>.</param>
        UniTask<CraftResult> FuseItems(string fusion, IEnumerable<long> userItemIds, string idempotencyKey = null);

        /// <summary>
        /// The player's latest rewards in this game, newest first: what runs (RecordCollected_Client),
        /// campaign levels and arena runs paid. Use it after a result to show what the run brought.
        /// Returns null on failure.
        /// </summary>
        /// <param name="limit">1-100, default 20.</param>
        /// <param name="since">Only rewards created at or after this time (UTC), e.g. when the run started.</param>
        UniTask<RecentRewardsResponse> GetRecentRewards(int limit = 20, System.DateTime? since = null);
    }
}
