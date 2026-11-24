using System.Collections.Generic;

namespace FunticoGamesSDK.APIModels
{
    /// <summary>The player's items, as returned by GET /Inventory.</summary>
    public class InventoryResponse
    {
        public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();
    }

    /// <summary>
    /// One catalog item the player owns, with all of the player's copies of it folded together.
    /// </summary>
    public class InventoryItem
    {
        /// <summary>Platform catalog id of the item (the same id prizes and entry fees use).</summary>
        public long ItemId { get; set; }

        /// <summary>The studio's own key for the item, as set in the item catalog. May be null.</summary>
        public string GameItemId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        /// <summary>Number of copies the player holds.</summary>
        public int Count { get; set; }

        /// <summary>Ids of the individual copies the player holds.</summary>
        public List<long> UserItemIds { get; set; } = new List<long>();

        public bool IsConsumable { get; set; }
        public bool IsInfinity { get; set; }

        /// <summary>True when the item belongs to the requesting game.</summary>
        public bool IsGameItem { get; set; }

        public bool IsSellable { get; set; }

        /// <summary>Rarity name from the catalog (e.g. "Rare"); null when the item has none.</summary>
        public string Rarity { get; set; }

        /// <summary>
        /// The item's catalog properties as a JSON object string; null when the item has none.
        /// Its shape is defined by the studio. Use FunticoSDK.GetInventoryItemProperties to parse it.
        /// </summary>
        public string PropertiesJson { get; set; }

        /// <summary>The item's base stats as entered in the catalog, as a JSON string; null when empty.</summary>
        public string BaseStatsJson { get; set; }

        /// <summary>Per-copy stats of an infinity item; empty for other items.</summary>
        public List<InventoryItemCopy> Copies { get; set; } = new List<InventoryItemCopy>();
    }

    public class InventoryItemCopy
    {
        /// <summary>Id of this copy (one of <see cref="InventoryItem.UserItemIds"/>).</summary>
        public long UserItemId { get; set; }

        /// <summary>Stats this copy has accumulated, as a JSON string ("[]" when none yet).</summary>
        public string StatsJson { get; set; }
    }

    /// <summary>
    /// Why the last inventory request failed. Matches the error body of the Inventory endpoint:
    /// { "Error": "ReloginRequired", "Message": "..." }. Compare <see cref="Error"/> with
    /// <see cref="InventoryErrorCodes"/>; <see cref="Message"/> is human-readable and may change.
    /// </summary>
    public class InventoryErrorResponse
    {
        public string Error { get; set; }
        public string Message { get; set; }
    }

    public static class InventoryErrorCodes
    {
        /// <summary>(HTTP 400) The player is not linked to a Funtico account.</summary>
        public const string NotLinked = "NotLinked";

        /// <summary>
        /// (HTTP 409) The player's Funtico session belongs to another game or has expired.
        /// Logging in to this game again (<c>Authentication</c> with a fresh platform token) fixes it.
        /// </summary>
        public const string ReloginRequired = "ReloginRequired";

        /// <summary>(HTTP 502) The Funtico platform is temporarily unavailable. Retry later.</summary>
        public const string PlatformUnavailable = "PlatformUnavailable";

        /// <summary>
        /// (HTTP 400) A field of a craft or fusion is missing or malformed, for example the id or the
        /// idempotency key.
        /// </summary>
        public const string InvalidRequest = "InvalidRequest";

        /// <summary>(HTTP 404) The game has no recipe or fusion with that key.</summary>
        public const string UnknownRule = "UnknownRule";

        /// <summary>(HTTP 409) The idempotency key was already used for a different action. Use a new key.</summary>
        public const string KeyReused = "KeyReused";

        /// <summary>
        /// (HTTP 422) The copies passed to FuseItems do not fit the fusion: not the player's, not eligible,
        /// or the wrong count.
        /// </summary>
        public const string WrongItems = "WrongItems";

        /// <summary>(HTTP 500) The game's recipes or fusions are set up wrong. Not something the player can fix.</summary>
        public const string Misconfigured = "Misconfigured";

        /// <summary>
        /// (HTTP 401) The player is not authenticated. Set by the SDK; call <c>Authentication</c> first.
        /// </summary>
        public const string Unauthorized = "Unauthorized";

        /// <summary>
        /// Set by the SDK when no structured error was returned: no connection, timeout, or an
        /// unexpected response. <see cref="InventoryErrorResponse.Message"/> has the details.
        /// </summary>
        public const string RequestFailed = "RequestFailed";
    }
}
