using System;
using System.Collections.Generic;
using FunticoGamesSDK.APIModels.Converters;
using Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Converters;

namespace FunticoGamesSDK.APIModels
{
    public enum TicketType
    {
        Free = 0,
        Currency = 1,
        Item = 2
    }

    public enum RoomTierEnum
    {
        Contender = 0,
        Challenger = 1,
        Champion = 2,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PrizeType
    {
        Item = 0,
        DepositStakePoolShare = 1,
        DepositStakePerPlayer = 2,
        GppPoolShare = 3,
        GppPerPlayer = 4,
        External = 5,
        Placeholder = 6,
        Reward = 7
    }

    public enum RoomFinishReason
    {
        Ok = 0,
        AutomaticallyClosed = 1,
        Kicked = 2,
        Unfinished = 3
    }

    public enum EntryFeeType
    {
        IC = 1,
        Tico = 2,
        SemifinalsTickets = 3,
        FinalTickets = 4,
        Free = 5,
        InventoryItem = 6,
        PrivateTickets = 7
    }

    public class ErrorResponse
    {
        public string Error { get; set; }
        public string Details { get; set; }
    }

    public class Error
    {
        public string Message { get; set; }
    }

    public class Computed
    {
        [JsonProperty("deposit_stake")]
        public long DepositStake { get; set; }

        [JsonProperty("stake")]
        public long Stake { get; set; }

        [JsonProperty("platform_fee")]
        public long PlatformFee { get; set; }

        [JsonProperty("burn")]
        public long Burn { get; set; }

        [JsonProperty("platform_fee_per_ticket")]
        public long PlatformFeePerTicket { get; set; }
    }

    public class Data
    {
        [JsonProperty("game_sessions")]
        public List<GameSession> GameSessions { get; set; }

        [JsonProperty("events")]
        public List<RoomConfig> Events { get; set; }

        [JsonProperty("item_ids")]
        public List<string> ItemIds { get; set; }
        
        [JsonProperty("reward_ids")]
        public List<string> RewardIds { get; set; }
    }

    public class Details
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("tier")]
        public int? Tier { get; set; }

        [JsonProperty("has_password")]
        public bool? HasPassword { get; set; }

        [JsonProperty("map_id")]
        public int? MapId { get; set; }

        [JsonProperty("cover_image")]
        public string CoverImage { get; set; }

        [JsonProperty("terms_and_conditions")]
        public string TermsAndConditions { get; set; }

        [JsonProperty("is_leaderboard_locked")]
        public bool? IsLeaderboardLocked { get; set; }

        [JsonProperty("is_play_button_visible")]
        public bool? IsPlayButtonVisible { get; set; }

        [JsonProperty("prize_pool_label")]
        public string PrizePoolLabel { get; set; }
    }

    public class RoomConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public RoomType? Type { get; set; }

        [JsonProperty("version")]
        public int? Version { get; set; }

        [JsonProperty("game_id")]
        public string GameId { get; set; }

        [JsonProperty("size")]
        public int? Size { get; set; }

        [JsonProperty("ticket")]
        public Ticket Ticket { get; set; }

        [JsonProperty("prize_pool")]
        public PrizePool PrizePool { get; set; }

        [JsonProperty("leaderboard")]
        public Leaderboard Leaderboard { get; set; }

        [JsonProperty("prize_distribution")]
        public List<PrizeDistribution> PrizeDistribution { get; set; }

        [JsonProperty("computed")]
        public Computed Computed { get; set; }

        [JsonProperty("details")]
        public Details Details { get; set; }

        [JsonProperty("item_ids")]
        public List<string> ItemIds { get; set; }
        
        [JsonProperty("reward_ids")]
        public List<string> RewardIds { get; set; }

        [JsonProperty("matchmaking_configuration_name")]
        public string MatchmakingConfigurationName { get; set; }

        [JsonProperty("updated_at")]
        public long? UpdatedAt { get; set; }
        [JsonProperty("settings")]
        public string Settings { get; set; }
    }
    
    public class Event2
    {
        [JsonProperty("repeat")]
        public int? Repeat { get; set; }

        [JsonProperty("startWhen")]
        public int? StartWhen { get; set; }

        [JsonProperty("min")]
        public int? Min { get; set; }

        [JsonProperty("max")]
        public int? Max { get; set; }
    }

    public class GameSession
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("registered_at")]
        public DateTime? RegisteredAt { get; set; }

        [JsonProperty("started_at")]
        public DateTime? StartedAt { get; set; }

        [JsonProperty("match_id")]
        public string MatchId { get; set; }

        [JsonProperty("score")]
        public int? Score { get; set; }

        [JsonProperty("scored_at")]
        public DateTime? ScoredAt { get; set; }

        [JsonProperty("distributed_at")]
        public DateTime? DistributedAt { get; set; }

        [JsonProperty("match_type")]
        public int? MatchType { get; set; }

        [JsonProperty("game_id")]
        public string GameId { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("event_id")]
        public string EventId { get; set; }
        [JsonProperty("leader")]
        public Leader Leader { get; set; }
        [JsonProperty("opponent_name")]
        public string OpponentName { get; set; }
        [JsonProperty("return_requested_at")]
        public DateTime? ReturnRequestedAt { get; set; }
        [JsonProperty("returned_at")]
        public DateTime? ReturnedAt  { get; set; }
        [JsonProperty("voucher_used")]
        [JsonConverter(typeof(FlexibleBoolConverter))]
        public bool VoucherUsed { get; set; }
        
    }

    public class Gpp
    {
        [JsonProperty("type")]
        public PrizePoolType? Type { get; set; }

        [JsonProperty("currency")]
        public EditableCurrency? Currency { get; set; }

        [JsonProperty("value")]
        public ulong Value { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }
    }

    public class Leaderboard
    {
        [JsonProperty("field")]
        public GameScoringSystemField? Field { get; set; }

        [JsonProperty("placement_mode")]
        public GamePlacementOrder? PlacementMode { get; set; }

        [JsonProperty("type")]
        public LeaderboardType? Type { get; set; }
        
        [JsonProperty("claim_limit")]
        public int? ClaimLimit { get; set; }

        [JsonProperty("attempts")]
        public int? Attempts { get; set; }

        [JsonProperty("fn")]
        public Fn? Fn { get; set; }
    }

    public class PlatformFee
    {
        [JsonProperty("type")]
        public PrizePoolType? Type { get; set; }

        [JsonProperty("value")]
        public long? Value { get; set; }

        [JsonProperty("burn_percentage")]
        public long? BurnPercentage { get; set; }
    }

    public class PrizeDistribution
    {
        [JsonProperty("prizes")]
        public List<PrizesResponses.Prize> Prizes { get; set; }

        [JsonProperty("place")]
        public int? Place { get; set; }
        
        [JsonProperty("xp")]
        public XPPoints? Xp { get; set; }
    }

    public class PrizePool
    {
        [JsonProperty("gpp")]
        public Gpp Gpp { get; set; }

        [JsonProperty("platform_fee")]
        public PlatformFee PlatformFee { get; set; }
    }
    
    public class XPPoints
    {
        [JsonProperty("base")]
        public int Base { get; set; }

        [JsonProperty("kyc")]
        public int Kyc { get; set; }
    }

    public class RoomsResponses
    {
        [JsonProperty("data")]
        public List<RoomConfig> Data { get; set; }

        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }

    public class RoomsHistoryResponse
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }
    
    public class RoomsSessionHistoryResponse
    {
        [JsonProperty("game_session")]
        public GameSession GameSessions { get; set; }
        [JsonProperty("event")]
        public RoomConfig Events { get; set; }
    }

    public class Ticket
    {
        [JsonProperty("type")]
        public TicketType? Type { get; set; }
        [JsonProperty("currency_amount")]
        public long? CurrencyAmount { get; set; }
        [JsonProperty("item_id")]
        public long? ItemId { get; set; }
        [JsonProperty("item")]
        public PlatformItemPrize? Item { get; set; }
    }
    
    public static class TicketExtensions
    {
        public static bool IsFree(this Ticket ticket) =>
            !ticket.Type.HasValue || ticket.Type.Value == TicketType.Free ||
            !ticket.CurrencyAmount.HasValue || ticket.CurrencyAmount == 0;
    }
}