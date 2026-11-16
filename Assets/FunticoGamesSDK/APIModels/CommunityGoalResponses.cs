using System;
using System.Collections.Generic;

namespace FunticoGamesSDK.APIModels
{
    /// <summary>
    /// Response of GetCommunityGoal and GetCommunityGoalById: a goal of the chain with its shared
    /// state and, for a signed-in player, the player's own state.
    /// </summary>
    public class CommunityGoalResponse
    {
        public CommunityGoalChainDto Chain { get; set; }

        /// <summary>The open goal (by chain key) or the requested one (by id); null when the chain has never had a goal.</summary>
        public CommunityGoalDto Goal { get; set; }

        /// <summary>The chain has no open goal right now: the last one closed and the next is not open yet.</summary>
        public bool AwaitingNext { get; set; }

        /// <summary>The player's own state; null when no player is signed in.</summary>
        public CommunityGoalMeDto Me { get; set; }

        /// <summary>How long to wait before polling the live state again.</summary>
        public int PollAfterSeconds { get; set; }

        /// <summary>The server's clock (UTC) when the response was made.</summary>
        public DateTime ServerTime { get; set; }
    }

    /// <summary>A chain: the goals opened one after another under one key, and the rules they share.</summary>
    public class CommunityGoalChainDto
    {
        public string Key { get; set; }
        public string Name { get; set; }

        /// <summary>How a player's contributions form their place: one of <see cref="CommunityGoalScorings"/>.</summary>
        public string Scoring { get; set; }

        /// <summary>Free attempts per player per UTC day across the whole chain; 0 means no limit.</summary>
        public int FreeAttemptsPerDay { get; set; }

        /// <summary>Shortest accepted attempt, by the server clock. A faster completion fails with CompletionTooFast.</summary>
        public int MinAttemptSeconds { get; set; }

        /// <summary>An attempt not completed within this time expires with no contribution.</summary>
        public int AttemptTtlSeconds { get; set; }

        /// <summary>Progress fractions (0 &lt; f &lt; 1) the game wants to know about, ascending.</summary>
        public List<double> Milestones { get; set; } = new List<double>();
    }

    public class CommunityGoalDto
    {
        public Guid GoalId { get; set; }

        /// <summary>1, 2, 3... within the chain.</summary>
        public int Number { get; set; }

        public string Name { get; set; }

        /// <summary>One of <see cref="CommunityGoalStatuses"/>.</summary>
        public string Status { get; set; }

        public long TargetValue { get; set; }

        /// <summary>Sum of every accepted contribution; may pass TargetValue (late attempts still count).</summary>
        public long CurrentValue { get; set; }

        /// <summary>CurrentValue / TargetValue, capped at 1.</summary>
        public double Progress { get; set; }

        /// <summary>Milestones the goal has passed.</summary>
        public List<double> MilestonesPassed { get; set; } = new List<double>();

        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int ContributorCount { get; set; }

        /// <summary>The game's own data for this goal (e.g. the boss to show), exactly as it was configured; may be null.</summary>
        public string PayloadJson { get; set; }

        /// <summary>When this goal is closed: the goal opened after it, if any.</summary>
        public Guid? NextGoalId { get; set; }

        /// <summary>
        /// The goal's reward brackets, top-down: at settlement each contributor gets the first bracket
        /// their final place matches, and only that one.
        /// </summary>
        public List<CommunityGoalBracketDto> Rewards { get; set; } = new List<CommunityGoalBracketDto>();
    }

    /// <summary>One reward bracket of a goal: who falls into it and what it pays, as configured for display.</summary>
    public class CommunityGoalBracketDto
    {
        /// <summary>The bracket's key; <see cref="CommunityGoalMeDto.Bracket"/> names it.</summary>
        public string Name { get; set; }

        /// <summary>What to show for the bracket ("#1", "TOP 10%"). May be null.</summary>
        public string Label { get; set; }

        /// <summary>Exactly one of MaxPlace, MaxPercentile and AnyContribution is set: final places 1..MaxPlace.</summary>
        public int? MaxPlace { get; set; }

        /// <summary>Final place x 100 / contributors at most this.</summary>
        public double? MaxPercentile { get; set; }

        /// <summary>Every contributor not in an earlier bracket.</summary>
        public bool AnyContribution { get; set; }

        /// <summary>What the bracket pays, as configured for display. The actual items are drawn at settlement.</summary>
        public List<CommunityGoalRewardDisplayDto> Display { get; set; } = new List<CommunityGoalRewardDisplayDto>();
    }

    public class CommunityGoalRewardDisplayDto
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int? Amount { get; set; }
    }

    public class CommunityGoalMeDto
    {
        /// <summary>True once the player has made an accepted contribution of more than zero to this goal.</summary>
        public bool IsContributor { get; set; }

        public long Score { get; set; }

        /// <summary>The player's place among contributors; null when not a contributor.</summary>
        public int? Place { get; set; }

        /// <summary>How much the player's contributions added to CurrentValue.</summary>
        public long Contributed { get; set; }

        /// <summary>The player's largest single accepted contribution to this goal ("best strike").</summary>
        public long Best { get; set; }

        /// <summary>Completed attempts on this goal.</summary>
        public int Attempts { get; set; }

        /// <summary>Attempts started today (UTC) anywhere in the chain.</summary>
        public int AttemptsUsedToday { get; set; }

        /// <summary>Null when the chain has no daily limit.</summary>
        public int? AttemptsLeftToday { get; set; }

        /// <summary>When the daily attempts reset (UTC midnight).</summary>
        public DateTime ResetsAt { get; set; }

        /// <summary>The player's live attempt in the chain (possibly on another goal); finish it before starting a new one.</summary>
        public GoalAttemptDto OpenAttempt { get; set; }

        /// <summary>Settled goals: the player's final place.</summary>
        public int? FinalPlace { get; set; }

        /// <summary>Settled goals: the reward bracket the final place fell into.</summary>
        public string Bracket { get; set; }

        /// <summary>
        /// Settled goals: where the bracket rewards are, one of <see cref="CommunityGoalRewardStatuses"/>.
        /// Null before settlement or for a non-contributor.
        /// </summary>
        public string RewardStatus { get; set; }

        /// <summary>
        /// What this goal paid the player, with item names and images. GetRecentRewards lists the same
        /// entries with Source "CommunityGoal".
        /// </summary>
        public List<RecentReward> Rewards { get; set; } = new List<RecentReward>();
    }

    /// <summary>One attempt on a goal. Pass it to CompleteGoalAttempt when the attempt is played.</summary>
    public class GoalAttemptDto
    {
        public Guid AttemptId { get; set; }
        public Guid GoalId { get; set; }

        /// <summary>Server-chosen random number for the attempt; the same every time the attempt is resumed.</summary>
        public int Seed { get; set; }

        public DateTime StartedAt { get; set; }

        /// <summary>Complete the attempt before this time (UTC); after it the attempt expires with no contribution.</summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>The goal's value when the attempt started; the same every time the attempt is resumed.</summary>
        public long GoalValueAtStart { get; set; }

        /// <summary>Milestones the goal had passed when the attempt started: the phase to play the attempt in.</summary>
        public List<double> MilestonesAtStart { get; set; } = new List<double>();
    }

    /// <summary>Response of GetCommunityGoalLive: the shared state only, for polling.</summary>
    public class CommunityGoalLiveResponse
    {
        public Guid GoalId { get; set; }
        public int Number { get; set; }

        /// <summary>One of <see cref="CommunityGoalStatuses"/>.</summary>
        public string Status { get; set; }

        public long TargetValue { get; set; }
        public long CurrentValue { get; set; }
        public double Progress { get; set; }
        public List<double> MilestonesPassed { get; set; } = new List<double>();
        public int ContributorCount { get; set; }

        /// <summary>When the goal is closed: the goal opened after it, if any.</summary>
        public Guid? NextGoalId { get; set; }

        /// <summary>How long to wait before polling again.</summary>
        public int PollAfterSeconds { get; set; }

        public DateTime ServerTime { get; set; }
    }

    /// <summary>Response of StartGoalAttempt.</summary>
    public class StartGoalAttemptResponse
    {
        public GoalAttemptDto Attempt { get; set; }

        /// <summary>True when this is the player's already-live attempt being returned again (no attempt was spent).</summary>
        public bool Resumed { get; set; }

        /// <summary>The goal the attempt belongs to, as it stood at the start. Play against this state and payload.</summary>
        public CommunityGoalDto Goal { get; set; }

        /// <summary>Null when the chain has no daily limit.</summary>
        public int? AttemptsLeftToday { get; set; }

        /// <summary>When the daily attempts reset (UTC midnight).</summary>
        public DateTime ResetsAt { get; set; }
    }

    /// <summary>Body of the attempt completion. Built and signed by the SDK.</summary>
    public class CompleteGoalAttemptRequestEncrypted
    {
        public Guid AttemptId { get; set; }
        public string EncryptedData { get; set; }
        public string Hash { get; set; }
    }

    /// <summary>Response of CompleteGoalAttempt.</summary>
    public class CompleteGoalAttemptResponse
    {
        public Guid AttemptId { get; set; }
        public Guid GoalId { get; set; }

        /// <summary>True when this answers a repeat of an already completed attempt.</summary>
        public bool Replayed { get; set; }

        /// <summary>The contribution the game sent.</summary>
        public long Reported { get; set; }

        /// <summary>What counted: Reported, cut down to the chain's plausibility limits.</summary>
        public long Accepted { get; set; }

        /// <summary>Accepted is less than Reported because of a limit.</summary>
        public bool Capped { get; set; }

        public long ValueBefore { get; set; }
        public long ValueAfter { get; set; }
        public long TargetValue { get; set; }

        /// <summary>Milestones this contribution carried the goal past.</summary>
        public List<double> MilestonesCrossed { get; set; } = new List<double>();

        /// <summary>One of <see cref="CommunityGoalStatuses"/>.</summary>
        public string GoalStatus { get; set; }

        /// <summary>The goal is closed now (by this contribution or before it).</summary>
        public bool GoalClosed { get; set; }

        /// <summary>This contribution reached the target.</summary>
        public bool ClosedByMe { get; set; }

        /// <summary>When the goal is closed: the goal that opened after it, if any.</summary>
        public Guid? NextGoalId { get; set; }

        public long Score { get; set; }

        /// <summary>True when this contribution raised the player's score.</summary>
        public bool ScoreImproved { get; set; }

        /// <summary>The player's largest single accepted contribution to this goal, this one included.</summary>
        public long Best { get; set; }

        /// <summary>This contribution beat every earlier one of the player on this goal (the same on a replay).</summary>
        public bool IsNewBest { get; set; }

        public int? Place { get; set; }

        /// <summary>Null when the chain has no daily limit.</summary>
        public int? AttemptsLeftToday { get; set; }

        public DateTime ResetsAt { get; set; }
    }

    /// <summary>Response of GetCommunityGoalLeaderboard: contributors ranked by score.</summary>
    public class CommunityGoalLeaderboardResponse
    {
        public Guid GoalId { get; set; }
        public List<CommunityGoalLeaderboardEntryDto> Items { get; set; } = new List<CommunityGoalLeaderboardEntryDto>();

        /// <summary>The player's own row, wherever it is; null until the player contributes (or when nobody is signed in).</summary>
        public CommunityGoalLeaderboardEntryDto Me { get; set; }

        /// <summary>Up to two rows above and below the player, the player included.</summary>
        public List<CommunityGoalLeaderboardEntryDto> Around { get; set; } = new List<CommunityGoalLeaderboardEntryDto>();

        public int Total { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public bool HasMore { get; set; }
    }

    public class CommunityGoalLeaderboardEntryDto
    {
        /// <summary>1-based place.</summary>
        public int Place { get; set; }

        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public long Score { get; set; }
        public long Contributed { get; set; }

        /// <summary>The player's largest single accepted contribution to this goal.</summary>
        public long Best { get; set; }

        public int Attempts { get; set; }
        public bool IsMe { get; set; }
    }

    /// <summary>Response of GetCommunityGoalHistory: the chain's past goals, newest first.</summary>
    public class CommunityGoalHistoryResponse
    {
        public List<CommunityGoalHistoryEntryDto> Items { get; set; } = new List<CommunityGoalHistoryEntryDto>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public bool HasMore { get; set; }
    }

    public class CommunityGoalHistoryEntryDto
    {
        public Guid GoalId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }

        /// <summary><see cref="CommunityGoalStatuses.Closed"/> or <see cref="CommunityGoalStatuses.Settled"/>.</summary>
        public string Status { get; set; }

        public long TargetValue { get; set; }
        public long CurrentValue { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        /// <summary>How long the goal held out, in seconds.</summary>
        public long LifetimeSeconds { get; set; }

        public int ContributorCount { get; set; }

        /// <summary>The game's own data for this goal, exactly as it was configured.</summary>
        public string PayloadJson { get; set; }

        /// <summary>The signed-in player's part in this goal; null when nobody is signed in.</summary>
        public CommunityGoalHistoryMeDto Me { get; set; }
    }

    public class CommunityGoalHistoryMeDto
    {
        public bool IsContributor { get; set; }
        public long Score { get; set; }
        public long Contributed { get; set; }
        public long Best { get; set; }
        public int Attempts { get; set; }

        /// <summary>Contributed / the goal's CurrentValue, 0..1.</summary>
        public double Share { get; set; }

        public int? FinalPlace { get; set; }
        public string Bracket { get; set; }

        /// <summary>One of <see cref="CommunityGoalRewardStatuses"/>; null before settlement or for a non-contributor.</summary>
        public string RewardStatus { get; set; }
    }

    /// <summary>Values of <see cref="CommunityGoalMeDto.RewardStatus"/>.</summary>
    public static class CommunityGoalRewardStatuses
    {
        /// <summary>No bracket for this place (or nothing payable).</summary>
        public const string None = "None";

        /// <summary>Placed in a bracket; the rewards are not drawn yet (usually within a minute of settling).</summary>
        public const string Pending = "Pending";

        /// <summary>Drawn; at least one is still on its way to the inventory (at the latest after the next sign-in).</summary>
        public const string Queued = "Queued";

        /// <summary>Everything drawn has arrived. A draw may come up empty.</summary>
        public const string Delivered = "Delivered";

        /// <summary>Nothing is left to send, but part of it was not accepted.</summary>
        public const string Failed = "Failed";
    }

    /// <summary>
    /// Why the last Community Goal request failed. Matches the error body of the Community Goal
    /// endpoints: { "Error": "NoAttemptsLeft", "Message": "...", "ResetsAt": "..." }. Compare
    /// <see cref="Error"/> with <see cref="CommunityGoalErrorCodes"/>; <see cref="Message"/> is
    /// human-readable and may change.
    /// </summary>
    public class CommunityGoalErrorResponse
    {
        public string Error { get; set; }
        public string Message { get; set; }

        /// <summary>GoalClosed: the goal that is open in the chain now, if any.</summary>
        public Guid? NextGoalId { get; set; }

        /// <summary>NoAttemptsLeft: when the daily attempts reset (UTC midnight).</summary>
        public DateTime? ResetsAt { get; set; }
    }

    public static class CommunityGoalErrorCodes
    {
        /// <summary>The game has no enabled chain with this key.</summary>
        public const string ChainNotFound = "ChainNotFound";

        /// <summary>The game has no goal with this id.</summary>
        public const string GoalNotFound = "GoalNotFound";

        /// <summary>
        /// The goal is no longer open. <see cref="CommunityGoalErrorResponse.NextGoalId"/> names the
        /// goal open now, if any: load it (it carries its own data) before starting again.
        /// </summary>
        public const string GoalClosed = "GoalClosed";

        /// <summary>
        /// Today's attempts of the chain are used up. <see cref="CommunityGoalErrorResponse.ResetsAt"/>
        /// says when they reset.
        /// </summary>
        public const string NoAttemptsLeft = "NoAttemptsLeft";

        /// <summary>No attempt with this id was started by the player.</summary>
        public const string AttemptNotFound = "AttemptNotFound";

        /// <summary>The attempt was not completed in time; it counts as used, with no contribution.</summary>
        public const string AttemptExpired = "AttemptExpired";

        /// <summary>Completed faster than the chain allows; the attempt counts as used, with no contribution.</summary>
        public const string CompletionTooFast = "CompletionTooFast";

        /// <summary>The completion could not be verified.</summary>
        public const string InvalidSignature = "InvalidSignature";

        /// <summary>The contribution is not a whole number of zero or more.</summary>
        public const string InvalidContribution = "InvalidContribution";

        /// <summary>
        /// (HTTP 401) The player is not authenticated. Set by the SDK; starting and completing
        /// attempts need a signed-in player.
        /// </summary>
        public const string Unauthorized = "Unauthorized";

        /// <summary>
        /// Set by the SDK when no structured error was returned: no connection, timeout, or an
        /// unexpected response. <see cref="CommunityGoalErrorResponse.Message"/> has the details.
        /// </summary>
        public const string RequestFailed = "RequestFailed";
    }

    /// <summary>Values of a goal's status.</summary>
    public static class CommunityGoalStatuses
    {
        /// <summary>Accepting attempts.</summary>
        public const string Open = "Open";

        /// <summary>Target reached; attempts started before the close may still complete and count.</summary>
        public const string Closed = "Closed";

        /// <summary>Every late attempt is in; final places are fixed.</summary>
        public const string Settled = "Settled";
    }

    /// <summary>Values of a chain's scoring rule: how a player's contributions form their place.</summary>
    public static class CommunityGoalScorings
    {
        /// <summary>Sum of every contribution.</summary>
        public const string Sum = "Sum";

        /// <summary>The single best contribution.</summary>
        public const string Best = "Best";

        /// <summary>Sum of the best contribution of each UTC day.</summary>
        public const string BestPerDay = "BestPerDay";
    }
}
