using System;
using System.Collections.Generic;
using FunticoGamesSDK.APIModels;
using Newtonsoft.Json;

namespace FunticoGamesSDK.ViewModels
{
    // ── Tournament (matches TournamentResponse from game server) ──────────────

    [Serializable]
    public class TournamentViewModel
    {
        [JsonProperty("Plaftorfm_Guid")] public string Guid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SpecialTournamentsType SpecialTournamentsType { get; set; }
        public EntryFeeType EntryFeeType { get; set; }
        public double EntryFee { get; set; }
        public long? EntryFeeItemId { get; set; }
        public string EntryFeeItemName { get; set; }
        public string EntryFeeItemImage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double LeftSeconds { get; set; }
        public int? PlayerLimit { get; set; }
        public int EntryLimit { get; set; }
        public bool? HavePassword { get; set; }
        public string PrizePoolLabel { get; set; }
        public int GuaranteedPrizePoolUsd { get; set; }
        public int? Gpp { get; set; }
        public string GppLabel { get; set; }
        public string ScoreMode { get; set; }
        public string ScoreType { get; set; }
        public bool IsKycRequired { get; set; }
        public string Settings { get; set; }
        public DateTime? PracticeStartTime { get; set; }
        public DateTime? PracticeEndTime { get; set; }
        public uint MinPlayerLevel { get; set; }
        public uint MaxPlayerLevel { get; set; }
    }

    // ── Leaderboard (matches TournamentWithPlayersResponse from game server) ──

    public class TournamentLeaderboardViewModel : TournamentViewModel
    {
        public List<TournamentPlayerViewModel> Players { get; set; } = new();
        public TournamentPlayerViewModel CurrentPlayer { get; set; }
        public int TotalPlayers { get; set; }
    }

    // ── Player (matches TournamentRacePlayerResponse from game server) ────────

    public class TournamentPlayerViewModel
    {
        public int FunticoId { get; set; }
        public string Name { get; set; }
        public int Place { get; set; }
        public int Points { get; set; }
        public int TimesPlayed { get; set; }
        public double BestTime { get; set; }
        public FunticoUserAvatar user_avatar { get; set; }
        public FunticoUserAvatar user_border { get; set; }
        public TournamentCommunityViewModel Community { get; set; }
    }

    // ── Community (matches TournamentCommunityResponse from game server) ──────

    public class TournamentCommunityViewModel
    {
        [JsonProperty("UUID")] public string Uuid { get; set; }
        public string Name { get; set; }
        public string IconUri { get; set; }
    }

    // ── Enter tournament (matches TournamentEnterResponse) ───────────────────

    public class TournamentEnterData
    {
        public string TournamentScoreUuid { get; set; }
        [JsonProperty("LocalTournamentUUID")] public Guid LocalTournamentUuid { get; set; }
    }

    // ── Prizes (matches TournamentPrizeRootResponse) ─────────────────────────

    public class TournamentPrizesViewModel
    {
        public List<TournamentPrizePlaceViewModel> Prizes { get; set; } = new();
    }

    public class TournamentPrizePlaceViewModel
    {
        public string Uuid { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public List<TournamentPrizeItemViewModel> Prizes { get; set; } = new();
    }

    public class TournamentPrizeItemViewModel
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int Amount { get; set; }
        [JsonProperty("Value_usd")] public int? ValueUsd { get; set; }
    }

    // ── Result (matches RaceResultResponse) ──────────────────────────────────

    public class TournamentResultViewModel
    {
        public double Score { get; set; }
        public double Time { get; set; }
        public uint XpReceive { get; set; }
        public int IcReceive { get; set; }
    }

    // ── History ───────────────────────────────────────────────────────────────

    public class TournamentHistoryViewModel
    {
        public List<TournamentHistoryEntry> Entries { get; set; } = new();
        public TournamentHistoryMeta Meta { get; set; }
    }
}
