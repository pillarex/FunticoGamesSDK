using System;
using System.Collections.Generic;

namespace FunticoGamesSDK.APIModels
{
    /// <summary>The game's ghost pools, as returned by GET /Ghost/pools.</summary>
    public class GhostPoolsResponse
    {
        public List<GhostPool> Pools { get; set; } = new List<GhostPool>();
    }

    /// <summary>
    /// One ghost matchmaking pool: its own stored ships, ratings and rules.
    /// Pass <see cref="Key"/> as the pool argument of the other Ghost methods.
    /// </summary>
    public class GhostPool
    {
        public string Key { get; set; }
        public string Name { get; set; }

        /// <summary>The current season, e.g. "s3". Ships and ratings of different seasons never mix.</summary>
        public string Partition { get; set; }

        /// <summary>
        /// True when the pool plays runs: start a run, fight until one of the limits below is
        /// reached, and the run finishes. False: fight any time, no run needed.
        /// </summary>
        public bool HasRun { get; set; }

        /// <summary>Run limits; null means no limit of that kind.</summary>
        public int? MaxWins { get; set; }
        public int? MaxLosses { get; set; }
        public int? MaxRounds { get; set; }

        /// <summary>The attributes that describe a ship when searching for an opponent or uploading.</summary>
        public List<GhostPoolAttribute> Attributes { get; set; } = new List<GhostPoolAttribute>();

        /// <summary>The player's rating in the current season (the starting rating if the player has not fought yet).</summary>
        public int Rating { get; set; }

        /// <summary>The entry each run costs in a paid pool; null for a free pool.</summary>
        public GhostEntry Entry { get; set; }
    }

    /// <summary>The entry a paid pool charges for each run.</summary>
    public class GhostEntry
    {
        /// <summary>What the entry buys.</summary>
        public string Product { get; set; }

        /// <summary>The price tier of the entry.</summary>
        public string Tier { get; set; }

        /// <summary>The price in PLAYR as a decimal string (e.g. "100"), to show before the player enters. May be null.</summary>
        public string DisplayPrice { get; set; }
    }

    public class GhostPoolAttribute
    {
        /// <summary>The key to use in the attributes dictionary (matched case-insensitively).</summary>
        public string Name { get; set; }

        /// <summary>Allowed values, inclusive.</summary>
        public int Min { get; set; }
        public int Max { get; set; }

        /// <summary>True when opponents always have exactly the same value.</summary>
        public bool Exact { get; set; }

        /// <summary>
        /// Null, or one of <see cref="GhostAttributeBinds"/>: the server fills the value from the
        /// player's run, so the game may leave it out. If the game sends it, it must equal the run's value.
        /// </summary>
        public string Bind { get; set; }
    }

    /// <summary>Values of <see cref="GhostPoolAttribute.Bind"/>.</summary>
    public static class GhostAttributeBinds
    {
        /// <summary>The run's current round: fights so far + 1.</summary>
        public const string RunRound = "Run.Round";

        public const string RunWins = "Run.Wins";
        public const string RunLosses = "Run.Losses";
    }

    public enum GhostRunStatus
    {
        Active = 0,
        Finished = 1,
        Abandoned = 2
    }

    /// <summary>The player's run in a pool. Round = Fights + 1.</summary>
    public class GhostRun
    {
        public Guid RunId { get; set; }
        public string Pool { get; set; }
        public string Partition { get; set; }

        /// <summary>Raw wire value ("Active", "Finished", "Abandoned"); use <see cref="GetStatus"/> for the typed enum.</summary>
        public string Status { get; set; }

        public int Round { get; set; }
        public int Fights { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int? MaxWins { get; set; }
        public int? MaxLosses { get; set; }
        public int? MaxRounds { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }

        /// <summary>The run's entry payment in a paid pool; null in a free pool.</summary>
        public GhostRunEntry Entry { get; set; }

        /// <summary>The typed <see cref="Status"/>, or null if the value is not one this SDK knows.</summary>
        public GhostRunStatus? GetStatus() => GhostWireEnums.Parse<GhostRunStatus>(Status);
    }

    public enum GhostEntryStatus
    {
        /// <summary>Paid.</summary>
        Charged = 0,

        /// <summary>Not confirmed yet. Call StartGhostRun again; it never charges twice for one run.</summary>
        Pending = 1,

        /// <summary>Not paid.</summary>
        Refused = 2,

        /// <summary>Paid back: the run was abandoned before its first opponent.</summary>
        Refunded = 3,

        /// <summary>Being paid back.</summary>
        RefundPending = 4
    }

    /// <summary>The entry payment of a run in a paid pool.</summary>
    public class GhostRunEntry
    {
        /// <summary>
        /// Raw wire value ("Charged", "Pending", "Refused", "Refunded", "RefundPending"); use
        /// <see cref="GetStatus"/> for the typed enum.
        /// </summary>
        public string Status { get; set; }

        /// <summary>The amount charged in PLAYR as a decimal string, or null.</summary>
        public string Amount { get; set; }

        /// <summary>The player's PLAYR balance after the charge as a decimal string, or null.</summary>
        public string BalanceAfter { get; set; }

        /// <summary>The typed <see cref="Status"/>, or null if the value is not one this SDK knows.</summary>
        public GhostEntryStatus? GetStatus() => GhostWireEnums.Parse<GhostEntryStatus>(Status);
    }

    /// <summary>Response of GetGhostRun, StartGhostRun and AbandonGhostRun.</summary>
    public class GhostRunResponse
    {
        /// <summary>The active run (after abandoning: the closed run); null when the player has none.</summary>
        public GhostRun Run { get; set; }

        /// <summary>The run's open match, if one was issued and not reported yet, so a reloaded game can resume the same fight.</summary>
        public GhostMatch OpenMatch { get; set; }
    }

    /// <summary>Body of POST /Ghost/find.</summary>
    public class GhostFindRequest
    {
        public string Pool { get; set; }

        /// <summary>Attribute values by name. Attributes bound to the run may be left out.</summary>
        public Dictionary<string, int> Attributes { get; set; } = new Dictionary<string, int>();

        /// <summary>Optional: the player's own ship as a JSON string, stored in the pool in the same request.</summary>
        public string Snapshot { get; set; }

        /// <summary>Snapshot ids the game does not want to face (at most 20).</summary>
        public List<Guid> Exclude { get; set; } = new List<Guid>();

        /// <summary>False forbids bot opponents for this search; true allows them where the pool does.</summary>
        public bool AllowBots { get; set; } = true;
    }

    /// <summary>One issued opponent. Report the fight with <see cref="MatchId"/>, once.</summary>
    public class GhostMatch
    {
        public Guid MatchId { get; set; }

        /// <summary>Id of the opponent's stored ship (what <c>exclude</c> of FindGhostOpponent takes).</summary>
        public Guid SnapshotId { get; set; }

        /// <summary>The opponent's ship exactly as it was uploaded (a JSON string).</summary>
        public string SnapshotJson { get; set; }

        /// <summary>Lowercase hex SHA-256 of the UTF-8 bytes of <see cref="SnapshotJson"/>.</summary>
        public string SnapshotSha256 { get; set; }

        public bool IsBot { get; set; }
        public string OwnerName { get; set; }
        public string OwnerAvatarUrl { get; set; }
        public string OwnerBorderUrl { get; set; }
        public int OwnerRating { get; set; }

        /// <summary>The player's rating at the time of the match.</summary>
        public int PlayerRating { get; set; }

        /// <summary>The opponent ship's attribute values by name.</summary>
        public Dictionary<string, int> OpponentAttributes { get; set; } = new Dictionary<string, int>();

        /// <summary>Name of the search step that found the opponent ("Fallback" for a fallback bot).</summary>
        public string Step { get; set; }

        /// <summary>Report the fight before this time (UTC); after it the match expires.</summary>
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>Response of POST /Ghost/find.</summary>
    public class GhostFindResponse
    {
        public GhostMatch Match { get; set; }

        /// <summary>The run after the search; null in pools without runs.</summary>
        public GhostRun Run { get; set; }

        /// <summary>Id of the player's ship stored by this request; null when no ship was sent.</summary>
        public Guid? UploadedSnapshotId { get; set; }
    }

    /// <summary>Body of POST /Ghost/upload: store a ship in the pool without searching.</summary>
    public class GhostUploadRequest
    {
        public string Pool { get; set; }
        public Dictionary<string, int> Attributes { get; set; } = new Dictionary<string, int>();
        public string Snapshot { get; set; }
    }

    public class GhostUploadResponse
    {
        public Guid SnapshotId { get; set; }

        /// <summary>Lowercase hex SHA-256 of the UTF-8 bytes of the stored ship.</summary>
        public string Sha256 { get; set; }

        public string Partition { get; set; }
    }

    /// <summary>A fight outcome, from the reporting player's point of view.</summary>
    public enum GhostOutcome
    {
        Win = 0,
        Loss = 1,
        Draw = 2
    }

    /// <summary>Body of POST /Ghost/report. Built and signed by the SDK.</summary>
    public class GhostReportRequest
    {
        public Guid MatchId { get; set; }
        public string EncryptedData { get; set; }
        public string Hash { get; set; }
    }

    public class GhostReportResponse
    {
        public Guid MatchId { get; set; }

        /// <summary>Raw wire value ("Win", "Loss", "Draw"); use <see cref="GetOutcome"/> for the typed enum.</summary>
        public string Outcome { get; set; }

        public int PlayerRatingBefore { get; set; }
        public int PlayerRatingAfter { get; set; }
        public int OpponentRatingBefore { get; set; }
        public int OpponentRatingAfter { get; set; }

        /// <summary>The run after the fight; null in pools without runs.</summary>
        public GhostRun Run { get; set; }

        /// <summary>The typed <see cref="Outcome"/>, or null if the value is not one this SDK knows.</summary>
        public GhostOutcome? GetOutcome() => GhostWireEnums.Parse<GhostOutcome>(Outcome);
    }

    /// <summary>What the player's ghosts did: other players fought the player's stored ships.</summary>
    public class GhostReportSummaryResponse
    {
        public string Pool { get; set; }
        public string Partition { get; set; }

        /// <summary>The player's current rating in this season.</summary>
        public int Rating { get; set; }

        /// <summary>Fights the player's ghosts won, lost and drew since the previous GetGhostReportSummary call.</summary>
        public int NewGhostWins { get; set; }
        public int NewGhostLosses { get; set; }
        public int NewGhostDraws { get; set; }

        /// <summary>Totals for this season.</summary>
        public int GhostWins { get; set; }
        public int GhostLosses { get; set; }
        public int GhostDraws { get; set; }
    }

    /// <summary>The player's fights and the fights against the player's ghosts, newest first.</summary>
    public class GhostHistoryResponse
    {
        public List<GhostHistoryItem> Items { get; set; } = new List<GhostHistoryItem>();
        public int Page { get; set; }
        public int Limit { get; set; }
        public bool HasMore { get; set; }
    }

    public enum GhostMatchRole
    {
        /// <summary>The player fought someone's ghost.</summary>
        Attacker = 0,

        /// <summary>Someone fought the player's ghost.</summary>
        Defender = 1
    }

    public class GhostHistoryItem
    {
        public Guid MatchId { get; set; }

        /// <summary>Raw wire value ("Attacker", "Defender"); use <see cref="GetRole"/> for the typed enum.</summary>
        public string Role { get; set; }

        /// <summary>
        /// Raw wire value ("Win", "Loss", "Draw") from the player's point of view; null when the match
        /// closed without an outcome. Use <see cref="GetOutcome"/> for the typed enum.
        /// </summary>
        public string Outcome { get; set; }

        /// <summary>True when the match expired unreported instead of being reported.</summary>
        public bool Expired { get; set; }

        public string OpponentName { get; set; }
        public string OpponentAvatarUrl { get; set; }
        public bool OpponentIsBot { get; set; }

        /// <summary>The player's rating before and after this fight.</summary>
        public int RatingBefore { get; set; }
        public int RatingAfter { get; set; }

        public string Partition { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        /// <summary>The typed <see cref="Role"/>, or null if the value is not one this SDK knows.</summary>
        public GhostMatchRole? GetRole() => GhostWireEnums.Parse<GhostMatchRole>(Role);

        /// <summary>The typed <see cref="Outcome"/>, or null when there is none.</summary>
        public GhostOutcome? GetOutcome() => GhostWireEnums.Parse<GhostOutcome>(Outcome);
    }

    /// <summary>Players of a pool's season ranked by rating, plus the player's own place.</summary>
    public class GhostLeaderboardResponse
    {
        public string Pool { get; set; }
        public string Partition { get; set; }
        public List<GhostLeaderboardEntry> Items { get; set; } = new List<GhostLeaderboardEntry>();

        /// <summary>The player's own row and place, wherever it is; null until the player has fought this season.</summary>
        public GhostLeaderboardEntry Me { get; set; }

        public int Page { get; set; }
        public int Limit { get; set; }
        public bool HasMore { get; set; }
    }

    public class GhostLeaderboardEntry
    {
        /// <summary>1-based place: rating, then wins, high to low.</summary>
        public int Place { get; set; }

        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public int Rating { get; set; }

        /// <summary>The player's own fights.</summary>
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }

        /// <summary>Fights against the player's ghosts, from the ghost's side.</summary>
        public int GhostWins { get; set; }
        public int GhostLosses { get; set; }
        public int GhostDraws { get; set; }

        public bool IsMe { get; set; }
    }

    /// <summary>
    /// Why the last Ghost request failed. Matches the error body of the Ghost endpoints:
    /// { "Error": "NoOpponent", "Message": "..." }. Compare <see cref="Error"/> with
    /// <see cref="GhostErrorCodes"/>; <see cref="Message"/> is human-readable and may change.
    /// </summary>
    public class GhostErrorResponse
    {
        public string Error { get; set; }
        public string Message { get; set; }
    }

    public static class GhostErrorCodes
    {
        /// <summary>The game has no enabled pool with this key.</summary>
        public const string PoolNotFound = "PoolNotFound";

        /// <summary>The pool has no runs, so there is nothing to start or abandon.</summary>
        public const string RunNotSupported = "RunNotSupported";

        /// <summary>The pool plays runs and the player has no active one: start a run first.</summary>
        public const string RunNotActive = "RunNotActive";

        /// <summary>An attribute bound to the run (round, wins, losses) differs from the run's value. Leave bound attributes out.</summary>
        public const string RunStateMismatch = "RunStateMismatch";

        /// <summary>An attribute is unknown, missing or out of its Min..Max range.</summary>
        public const string InvalidAttributes = "InvalidAttributes";

        /// <summary>The ship is missing, not valid JSON, or larger than the pool allows.</summary>
        public const string InvalidSnapshot = "InvalidSnapshot";

        /// <summary>
        /// Nobody matched the search and no bot was available. A normal outcome, not a fault:
        /// let the player retry later (or search with other attributes).
        /// </summary>
        public const string NoOpponent = "NoOpponent";

        /// <summary>No match with this id was issued to the player.</summary>
        public const string MatchNotFound = "MatchNotFound";

        /// <summary>The match expired before it was reported.</summary>
        public const string MatchExpired = "MatchExpired";

        /// <summary>The report could not be verified.</summary>
        public const string InvalidSignature = "InvalidSignature";

        /// <summary>The outcome is not Win, Loss or Draw.</summary>
        public const string InvalidOutcome = "InvalidOutcome";

        /// <summary>The report's match data is over 32 KB. Nothing was recorded; report again without it.</summary>
        public const string DataTooLarge = "DataTooLarge";

        /// <summary>Paid pool: the player's balance is below the entry price. Nothing was charged and no run started.</summary>
        public const string InsufficientFunds = "InsufficientFunds";

        /// <summary>Paid pool: the player must complete identity verification before paying an entry.</summary>
        public const string KycRequired = "KycRequired";

        /// <summary>
        /// Paid pool: the entry cannot be bought right now (no such product or price, the player is
        /// suspended, or paid entries are unavailable).
        /// </summary>
        public const string EntryUnavailable = "EntryUnavailable";

        /// <summary>
        /// Paid pool: the entry payment is not confirmed yet. Call StartGhostRun again; it never charges
        /// twice for one run. Until then the run gets no opponent (FindGhostOpponent fails with this too).
        /// </summary>
        public const string PaymentPending = "PaymentPending";

        /// <summary>
        /// The player's Funtico session belongs to another game or has expired. Logging in to this game
        /// again (<c>Authentication</c> with a fresh platform token) fixes it.
        /// </summary>
        public const string ReloginRequired = "ReloginRequired";

        /// <summary>
        /// (HTTP 401) The player is not authenticated. Set by the SDK; call <c>Authentication</c> first.
        /// </summary>
        public const string Unauthorized = "Unauthorized";

        /// <summary>
        /// Set by the SDK when no structured error was returned: no connection, timeout, or an
        /// unexpected response. <see cref="GhostErrorResponse.Message"/> has the details.
        /// </summary>
        public const string RequestFailed = "RequestFailed";
    }

    internal static class GhostWireEnums
    {
        /// <summary>Parses a wire name ("Win", "Active", ...); null for null, numbers and unknown names.</summary>
        public static T? Parse<T>(string value) where T : struct, Enum
        {
            if (string.IsNullOrEmpty(value) || int.TryParse(value, out _))
                return null;
            if (Enum.TryParse(value, true, out T parsed) && Enum.IsDefined(typeof(T), parsed))
                return parsed;
            return null;
        }
    }
}
