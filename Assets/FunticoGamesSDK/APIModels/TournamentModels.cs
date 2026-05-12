using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FunticoGamesSDK.APIModels
{
    // ── Enums ──────────────────────────────────────────────────────────────────

    public enum TournamentsFilterEnum
    {
        OnGoing,
        Upcoming,
        Finished,
    }

    public enum SpecialTournamentsType
    {
        Regular,
        Special,
        Finals,
        Semifinals,
    }

    // EntryFeeType is defined in RoomsResponses.cs (same namespace)

    // ── Avatar (used in TournamentPlayerViewModel) ────────────────────────────

    public class FunticoUserAvatar
    {
        public string name { get; set; }
        public string url { get; set; }
        public string type { get; set; }
        public string description { get; set; }
    }

    // ── Client encrypted save-score ───────────────────────────────────────────

    // Inner payload — gets AES-encrypted before sending
    public class TournamentSaveScorePayload
    {
        public int? Score { get; set; }
        public bool IsSuccess { get; set; }
        public string CustomData { get; set; }
    }

    // Outer request body reuses RoomSaveScoreRequestEncrypted (same fields)

    // ── Server save-score request (plain, requires server API key) ────────────

    public class TournamentSaveScoreRequest
    {
        public int UserId { get; set; }
        public int? Score { get; set; }
        public string Tournament_uuid { get; set; }
        public string Tournament_score_uuid { get; set; }
        public bool IsSuccess { get; set; }
        public string Ip { get; set; } = "";
    }

    // ── History response (snake_case JSON from Funtico platform) ─────────────

    public class TournamentHistoryResponse
    {
        [JsonProperty("data")] public List<TournamentHistoryEntry> Data { get; set; }
        [JsonProperty("meta")] public TournamentHistoryMeta Meta { get; set; }
    }

    public class TournamentHistoryMeta
    {
        [JsonProperty("current_page")] public int? CurrentPage { get; set; }
        [JsonProperty("from")] public int? From { get; set; }
        [JsonProperty("last_page")] public int? LastPage { get; set; }
        [JsonProperty("per_page")] public int? PerPage { get; set; }
        [JsonProperty("to")] public int? To { get; set; }
        [JsonProperty("total")] public int? Total { get; set; }
    }

    public class TournamentHistoryEntry
    {
        [JsonProperty("tournament_name")] public string TournamentName { get; set; }
        [JsonProperty("tournament_uuid")] public string TournamentUuid { get; set; }
        [JsonProperty("starts_at")] public DateTime? StartsAt { get; set; }
        [JsonProperty("ends_at")] public DateTime? EndsAt { get; set; }
        [JsonProperty("scored_at")] public DateTime? ScoredAt { get; set; }
        [JsonProperty("is_finished")] public bool? IsFinished { get; set; }
        [JsonProperty("entries_count")] public int? EntriesCount { get; set; }
        [JsonProperty("max_entries")] public int? MaxEntries { get; set; }
        [JsonProperty("best_score")] public int? BestScore { get; set; }
        [JsonProperty("leaderboard_place")] public int? LeaderboardPlace { get; set; }
        [JsonProperty("prizes")] public List<TournamentHistoryPrize> Prizes { get; set; }
        [JsonProperty("game_id")] public int? GameId { get; set; }
        [JsonProperty("map_id")] public int? MapId { get; set; }
    }

    public class TournamentHistoryPrize
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("amount")] public int? Amount { get; set; }
        [JsonProperty("image_url")] public string ImageUrl { get; set; }
        [JsonProperty("is_redeemed")] public bool? IsRedeemed { get; set; }
    }
}
