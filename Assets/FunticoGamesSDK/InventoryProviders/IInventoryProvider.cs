using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;

namespace FunticoGamesSDK.InventoryProviders
{
    public interface IInventoryProvider
    {
        /// <summary>
        /// Fetches the items the player owns on the Funtico platform. Read-only.
        /// Always calls the server; the result is not cached.
        /// Returns null on failure; <see cref="GetLastInventoryError"/> then says why.
        /// </summary>
        /// <param name="includeOtherItems">
        /// False (default): only items that belong to this game.
        /// True: also platform-wide items and other games' consumables (check <see cref="InventoryItem.IsGameItem"/>).
        /// </param>
        UniTask<InventoryResponse> GetInventory(bool includeOtherItems = false);

        /// <summary>
        /// Returns why the last <see cref="GetInventory"/> call failed, or null if it succeeded
        /// (or nothing has been fetched yet). Compare <c>Error</c> with <see cref="InventoryErrorCodes"/>.
        /// </summary>
        InventoryErrorResponse GetLastInventoryError();

        /// <summary>
        /// Deserializes the item's PropertiesJson into a game-defined type.
        /// Returns default (null for reference types) if the item is null or has no properties.
        /// Throws Newtonsoft.Json.JsonException if PropertiesJson cannot be read as <typeparamref name="T"/>.
        /// </summary>
        T GetInventoryItemProperties<T>(InventoryItem item);

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
