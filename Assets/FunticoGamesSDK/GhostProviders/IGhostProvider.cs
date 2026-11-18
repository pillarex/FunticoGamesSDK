using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;

namespace FunticoGamesSDK.GhostProviders
{
    /// <summary>
    /// Asynchronous ghost matchmaking: the player fights stored ships ("ghosts") of other players
    /// or bots, and other players fight the player's stored ships while the player is away.
    /// Every method calls the server (nothing is cached), returns null on failure and leaves the
    /// reason in <see cref="GetLastGhostError"/>. No IErrorHandler popup is shown.
    /// </summary>
    public interface IGhostProvider
    {
        /// <summary>
        /// Fetches the game's ghost pools with their attribute schema and the player's rating in each.
        /// Returns null on failure.
        /// </summary>
        UniTask<GhostPoolsResponse> GetGhostPools();

        /// <summary>
        /// Fetches the player's active run in the pool and its open match, if any (use it to resume
        /// after a reload). <c>Run</c> is null when the player has no active run; in pools without runs
        /// both are always null (FindGhostOpponent returns the open match again). Returns null on failure.
        /// </summary>
        UniTask<GhostRunResponse> GetGhostRun(string pool);

        /// <summary>
        /// Starts a run in the pool, or returns the active one if the player already has it.
        /// Only for pools with <c>HasRun</c>. Returns null on failure.
        /// </summary>
        UniTask<GhostRunResponse> StartGhostRun(string pool);

        /// <summary>
        /// Gives up the player's active run. An open match of the run is closed as unreported.
        /// The response carries the closed run. Returns null on failure.
        /// </summary>
        UniTask<GhostRunResponse> AbandonGhostRun(string pool);

        /// <summary>
        /// Finds an opponent and issues a match. Searching again before reporting returns the same
        /// open match. Returns null on failure; <c>NoOpponent</c> in <see cref="GetLastGhostError"/>
        /// is a normal outcome (nobody and no bot available).
        /// </summary>
        /// <param name="pool">The pool key.</param>
        /// <param name="attributes">Attribute values by name. Attributes bound to the run may be left out.</param>
        /// <param name="mySnapshotJson">Optional: the player's own ship as a JSON string, stored in the pool in the same request.</param>
        /// <param name="exclude">Snapshot ids not to face (e.g. recent opponents). The first 20 distinct ids are sent.</param>
        /// <param name="allowBots">False forbids bot opponents for this search.</param>
        UniTask<GhostFindResponse> FindGhostOpponent(string pool, Dictionary<string, int> attributes,
            string mySnapshotJson = null, IEnumerable<Guid> exclude = null, bool allowBots = true);

        /// <summary>
        /// Stores the player's ship in the pool without searching. Returns null on failure.
        /// </summary>
        /// <param name="pool">The pool key.</param>
        /// <param name="attributes">Attribute values by name. Attributes bound to the run may be left out.</param>
        /// <param name="snapshotJson">The ship as a JSON string. Opponents receive it exactly as sent.</param>
        UniTask<GhostUploadResponse> UploadGhostSnapshot(string pool, Dictionary<string, int> attributes, string snapshotJson);

        /// <summary>
        /// Reports the fight of a match, encrypted and signed with the game's private key. A match is
        /// reported once; reporting it again returns the first result, so retrying after a network
        /// error is safe. Returns null on failure.
        /// </summary>
        /// <param name="matchId"><c>GhostMatch.MatchId</c> of the issued match.</param>
        /// <param name="outcome">The outcome from the player's point of view.</param>
        /// <param name="fightHash">Optional: the game's hash of the fight (up to 128 characters are kept).</param>
        /// <param name="matchData">
        /// Optional: your evidence for a later check of this fight, in your own format (e.g. your run's log up to and
        /// including this fight). Signed with the report. At most 32 KB in UTF-8; a larger one fails with
        /// <c>DataTooLarge</c> and nothing is recorded, so report again without it.
        /// </param>
        UniTask<GhostReportResponse> ReportGhostResult(Guid matchId, GhostOutcome outcome, string fightHash = null, string matchData = null);

        /// <summary>
        /// Fetches what the player's ghosts did in the pool. The <c>New*</c> counts are the fights
        /// since the previous call; each call resets them. Returns null on failure.
        /// </summary>
        UniTask<GhostReportSummaryResponse> GetGhostReportSummary(string pool);

        /// <summary>
        /// Fetches the player's fights and the fights against the player's ghosts in the pool, newest
        /// first. Returns null on failure.
        /// </summary>
        /// <param name="pool">The pool key.</param>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page, 1-50.</param>
        UniTask<GhostHistoryResponse> GetGhostHistory(string pool, int page = 1, int limit = 20);

        /// <summary>
        /// Fetches the players of the pool's season ranked by rating, plus the player's own place
        /// (<c>Me</c>). Only players who fought this season are ranked. Returns null on failure.
        /// </summary>
        /// <param name="pool">The pool key.</param>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Entries per page, 1-100.</param>
        /// <param name="partition">A past season such as "s1"; null for the current one.</param>
        UniTask<GhostLeaderboardResponse> GetGhostLeaderboard(string pool, int page = 1, int limit = 20, string partition = null);

        /// <summary>
        /// Returns why the last Ghost call failed, or null if it succeeded (or nothing has been
        /// called yet). Compare <c>Error</c> with <see cref="GhostErrorCodes"/>.
        /// </summary>
        GhostErrorResponse GetLastGhostError();
    }
}
