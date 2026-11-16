using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;

namespace FunticoGamesSDK.CommunityGoalProviders
{
    /// <summary>
    /// Community goals: every player of the game pushes one shared counter toward a target (e.g. a
    /// world boss that everyone hits). A chain, named by its key, opens its goals one after another.
    /// The read methods work without a signed-in player; starting and completing attempts need one.
    /// Every method calls the server (nothing is cached), returns null on failure and leaves the
    /// reason in <see cref="GetLastCommunityGoalError"/>. No IErrorHandler popup is shown.
    /// </summary>
    public interface ICommunityGoalProvider
    {
        /// <summary>
        /// Fetches the chain's open goal (or its latest one while the next is not open yet) and, for a
        /// signed-in player, the player's own state. Returns null on failure.
        /// </summary>
        /// <param name="chainKey">The chain key, e.g. "world-boss".</param>
        UniTask<CommunityGoalResponse> GetCommunityGoal(string chainKey);

        /// <summary>
        /// Fetches any goal of the game by id, open or past, with the player's own state.
        /// Returns null on failure.
        /// </summary>
        UniTask<CommunityGoalResponse> GetCommunityGoalById(Guid goalId);

        /// <summary>
        /// Fetches the goal's shared state only. Poll it while the goal is on screen, waiting
        /// <c>PollAfterSeconds</c> between calls. Returns null on failure.
        /// </summary>
        UniTask<CommunityGoalLiveResponse> GetCommunityGoalLive(Guid goalId);

        /// <summary>
        /// Starts an attempt on an open goal, spending one of today's attempts. If the player already
        /// has a live attempt in the chain, that attempt is returned again with <c>Resumed</c> true and
        /// nothing is spent (use it to resume after a reload). Returns null on failure:
        /// <c>GoalClosed</c> (see <see cref="CommunityGoalErrorResponse.NextGoalId"/>) and
        /// <c>NoAttemptsLeft</c> (see <see cref="CommunityGoalErrorResponse.ResetsAt"/>) are normal outcomes.
        /// </summary>
        UniTask<StartGoalAttemptResponse> StartGoalAttempt(Guid goalId);

        /// <summary>
        /// Completes an attempt with its contribution, encrypted and signed with the game's private key.
        /// An attempt is completed once; completing it again returns the first result with
        /// <c>Replayed</c> true, so retrying after a network error is safe. An attempt started before
        /// the goal closed still counts. Returns null on failure.
        /// </summary>
        /// <param name="attempt">The attempt from StartGoalAttempt (its AttemptId and GoalId are sent).</param>
        /// <param name="contribution">What the attempt achieved toward the goal (damage, coins...): 0 or more.</param>
        /// <param name="clientDurationSeconds">Optional: the game's own measure of the attempt; informative only.</param>
        /// <param name="metrics">Optional: numbers for analytics (up to 16 keys are kept).</param>
        /// <param name="fightHash">Optional: the game's hash of the attempt for a later check (up to 128 characters are kept).</param>
        UniTask<CompleteGoalAttemptResponse> CompleteGoalAttempt(GoalAttemptDto attempt, long contribution,
            double? clientDurationSeconds = null, IDictionary<string, double> metrics = null, string fightHash = null);

        /// <summary>
        /// Fetches the goal's contributors ranked by score, plus the player's own row and the rows
        /// around it. Returns null on failure.
        /// </summary>
        /// <param name="goalId">The goal.</param>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Entries per page, 1-100.</param>
        UniTask<CommunityGoalLeaderboardResponse> GetCommunityGoalLeaderboard(Guid goalId, int page = 1, int limit = 20);

        /// <summary>
        /// Fetches the chain's past goals (closed or settled), newest first, with the signed-in
        /// player's part in each: score, best strike, share of the goal, final place, bracket and
        /// reward status. For one goal's full state and the rewards it paid, use GetCommunityGoalById.
        /// Returns null on failure.
        /// </summary>
        /// <param name="chainKey">The chain key, e.g. "world-boss".</param>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Entries per page, 1-20.</param>
        UniTask<CommunityGoalHistoryResponse> GetCommunityGoalHistory(string chainKey, int page = 1, int limit = 10);

        /// <summary>
        /// Polls the goal's live state at the interval the server asks for and calls
        /// <paramref name="onChange"/> whenever the value, status or contributor count changes (and once
        /// at the start). Polling pauses while the application has no focus and stops by itself once the
        /// goal is settled. Stop the returned watch when the goal leaves the screen.
        /// </summary>
        /// <param name="goalId">The goal to follow.</param>
        /// <param name="onChange">Called on the main thread with the new live state.</param>
        /// <param name="onError">Optional: called when a poll fails; polling goes on.</param>
        /// <param name="minIntervalSeconds">Never poll more often than this, whatever the server asks.</param>
        /// <param name="pollInBackground">Keep polling while the application has no focus.</param>
        CommunityGoalWatch WatchCommunityGoal(Guid goalId, Action<CommunityGoalLiveResponse> onChange,
            Action<CommunityGoalErrorResponse> onError = null, int minIntervalSeconds = 2, bool pollInBackground = false);

        /// <summary>
        /// Returns why the last Community Goal call failed, or null if it succeeded (or nothing has
        /// been called yet). Compare <c>Error</c> with <see cref="CommunityGoalErrorCodes"/>.
        /// </summary>
        CommunityGoalErrorResponse GetLastCommunityGoalError();
    }
}
