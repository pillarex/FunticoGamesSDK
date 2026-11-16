using System;
using System.Collections.Generic;

namespace FunticoGamesSDK.APIModels
{
    /// <summary>The player's latest rewards in this game, newest first, as returned by GET /Rewards/recent.</summary>
    public class RecentRewardsResponse
    {
        public List<RecentReward> Rewards { get; set; } = new List<RecentReward>();
    }

    /// <summary>One reward a run, a campaign level or an arena run paid (or is paying) the player.</summary>
    public class RecentReward
    {
        public Guid Id { get; set; }

        /// <summary>Room, Tournament, Practice, Campaign, Arena or CommunityGoal.</summary>
        public string Source { get; set; }

        /// <summary>The run it belongs to.</summary>
        public string RunId { get; set; }

        /// <summary>The collected key or reward table that produced it; for a Community Goal, the reward bracket.</summary>
        public string Key { get; set; }

        /// <summary>See <see cref="RecentRewardStatus"/>.</summary>
        public string Status { get; set; }

        /// <summary>What it pays. Empty for an Empty draw and for some rewards not delivered yet.</summary>
        public List<RecentRewardItem> Items { get; set; } = new List<RecentRewardItem>();

        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }

    public class RecentRewardItem
    {
        /// <summary>Platform catalog id of the item.</summary>
        public long ItemId { get; set; }

        /// <summary>The studio's own key for the item. May be null.</summary>
        public string GameItemId { get; set; }

        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int Amount { get; set; }
    }

    /// <summary>Values of <see cref="RecentReward.Status"/>.</summary>
    public static class RecentRewardStatus
    {
        /// <summary>Accepted and on its way to the player's inventory.</summary>
        public const string Queued = "Queued";

        /// <summary>In the player's inventory.</summary>
        public const string Delivered = "Delivered";

        /// <summary>A random draw that paid nothing.</summary>
        public const string Empty = "Empty";

        /// <summary>The platform refused it.</summary>
        public const string Refused = "Refused";

        /// <summary>Gave up after repeated failures.</summary>
        public const string Failed = "Failed";
    }
}
