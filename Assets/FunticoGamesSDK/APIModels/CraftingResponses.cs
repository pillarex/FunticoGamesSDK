using System.Collections.Generic;

namespace FunticoGamesSDK.APIModels
{
    /// <summary>The crafting recipes and fusions of this game, as returned by GET /Inventory/recipes.</summary>
    public class RecipesResponse
    {
        public List<CraftRecipe> Recipes { get; set; } = new List<CraftRecipe>();
        public List<FusionRecipe> Fusions { get; set; } = new List<FusionRecipe>();
    }

    /// <summary>A fixed recipe: CraftItem takes every Inputs line from the player and gives every Outputs line.</summary>
    public class CraftRecipe
    {
        /// <summary>Pass this to CraftItem.</summary>
        public string Key { get; set; }

        /// <summary>Display name. May be null.</summary>
        public string Name { get; set; }

        /// <summary>True when the item keys contain "{id}", so CraftItem needs an id.</summary>
        public bool HasId { get; set; }

        public List<CraftRecipeItem> Inputs { get; set; } = new List<CraftRecipeItem>();
        public List<CraftRecipeItem> Outputs { get; set; } = new List<CraftRecipeItem>();
    }

    /// <summary>One line of a recipe: an item and how many of it.</summary>
    public class CraftRecipeItem
    {
        /// <summary>
        /// The studio's own item key. It may contain the placeholder "{id}", which CraftItem fills with the
        /// id you pass (for example "sg_shard_{id}" becomes "sg_shard_laser_pulse").
        /// </summary>
        public string Item { get; set; }

        public int Amount { get; set; }
    }

    /// <summary>
    /// A fusion: FuseItems takes exactly <see cref="Need"/> copies the player picked, each of an item listed
    /// in <see cref="Inputs"/>, and gives one random item listed in <see cref="Results"/>.
    /// </summary>
    public class FusionRecipe
    {
        /// <summary>Pass this to FuseItems.</summary>
        public string Key { get; set; }

        /// <summary>Display name. May be null.</summary>
        public string Name { get; set; }

        /// <summary>How many copies one fusion takes.</summary>
        public int Need { get; set; }

        /// <summary>The studio's item keys a picked copy may be of.</summary>
        public List<string> Inputs { get; set; } = new List<string>();

        /// <summary>The studio's item keys the random result may be.</summary>
        public List<string> Results { get; set; } = new List<string>();
    }

    /// <summary>The outcome of CraftItem or FuseItems.</summary>
    public class CraftResult
    {
        /// <summary>Id of the action. Quote it when reporting a problem.</summary>
        public string OperationId { get; set; }

        /// <summary>See <see cref="CraftStatus"/>.</summary>
        public string Status { get; set; }

        /// <summary>True when this answer repeats an earlier call made with the same idempotency key.</summary>
        public bool Replayed { get; set; }

        /// <summary>What was taken from the player. Empty unless Status is Succeeded.</summary>
        public List<CraftResultItem> Burned { get; set; } = new List<CraftResultItem>();

        /// <summary>What the player received. Empty unless Status is Succeeded.</summary>
        public List<CraftResultItem> Granted { get; set; } = new List<CraftResultItem>();

        /// <summary>
        /// Why a Refused action was refused, as a machine-readable string (for example "not_enough_items",
        /// "item_locked", "unknown_game_item_id", "max_supply_reached"); null otherwise.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>Human-readable detail for <see cref="Reason"/>. May be null.</summary>
        public string Message { get; set; }

        /// <summary>
        /// The idempotency key this call used: the one you passed, or the one the SDK generated. Set by the SDK.
        /// </summary>
        public string IdempotencyKey { get; set; }
    }

    /// <summary>Copies of one item taken or given by a craft or fusion.</summary>
    public class CraftResultItem
    {
        /// <summary>The studio's own item key.</summary>
        public string GameItemId { get; set; }

        /// <summary>Ids of the copies taken or given.</summary>
        public List<long> UserItemIds { get; set; } = new List<long>();
    }

    /// <summary>Values of <see cref="CraftResult.Status"/>.</summary>
    public static class CraftStatus
    {
        /// <summary>Done: the items in Burned were taken and the items in Granted were given.</summary>
        public const string Succeeded = "Succeeded";

        /// <summary>
        /// Not confirmed yet. Call again with the same arguments and the same
        /// <see cref="CraftResult.IdempotencyKey"/> until the status is Succeeded or Refused; the action
        /// never happens twice.
        /// </summary>
        public const string Pending = "Pending";

        /// <summary>Nothing changed; Reason says why. Trying again needs a new idempotency key.</summary>
        public const string Refused = "Refused";
    }

    public class CraftItemRequest
    {
        public string Recipe { get; set; }

        /// <summary>Fills "{id}" in the recipe's item keys; null for a recipe without it.</summary>
        public string Id { get; set; }

        public string IdempotencyKey { get; set; }
    }

    public class FuseItemsRequest
    {
        public string Fusion { get; set; }
        public List<long> UserItemIds { get; set; } = new List<long>();
        public string IdempotencyKey { get; set; }
    }
}
