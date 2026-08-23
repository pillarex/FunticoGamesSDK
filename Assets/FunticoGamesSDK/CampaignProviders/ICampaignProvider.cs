using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;

namespace FunticoGamesSDK.CampaignProviders
{
    public interface ICampaignProvider
    {
        /// <summary>
        /// Fetches campaign availability for the current game (level count, max stars).
        /// </summary>
        /// <param name="useCache">If true, returns the cached value when present; if false, always re-fetches.</param>
        UniTask<CampaignAvailabilityResponse> GetCampaignAvailability(bool useCache = true);

        /// <summary>
        /// Returns the cached availability instantly, or null if nothing has been fetched yet.
        /// </summary>
        CampaignAvailabilityResponse GetCachedCampaignAvailability();

        /// <summary>
        /// Fetches the full level list with the player's per-level progress.
        /// </summary>
        /// <param name="useCache">If true, returns the cached value when present; if false, always re-fetches.</param>
        UniTask<CampaignLevelsResponse> GetCampaignLevels(bool useCache = true);

        /// <summary>
        /// Returns the cached level list instantly, or null if nothing has been fetched yet.
        /// </summary>
        CampaignLevelsResponse GetCachedCampaignLevels();

        /// <summary>
        /// Starts an attempt on a level. The attempt is tracked internally for
        /// <see cref="CompleteCampaignLevel"/>. Returns null on failure.
        /// </summary>
        UniTask<StartLevelResponse> StartCampaignLevel(Guid levelId);

        /// <summary>
        /// Returns the currently tracked attempt, or null if none is active.
        /// </summary>
        StartLevelResponse GetActiveCampaignAttempt();

        /// <summary>
        /// Clears the tracked attempt locally. No server call is made.
        /// </summary>
        void AbandonCampaignAttempt();

        /// <summary>
        /// Completes the tracked attempt with the collected gameplay metrics.
        /// Stars are computed server-side; losing is a normal response (IsWin=false).
        /// Returns null on failure. The attempt is kept for network/5xx errors so the call may be
        /// retried, but is cleared when the server reports AttemptNotFound or CompletionTooFast
        /// (the server-side attempt is dead and retrying can never succeed).
        /// On success the recorded client-session events are cleared (they were delivered).
        /// </summary>
        /// <param name="metrics">Flat metric dictionary: 1-32 entries, key length 1-64, finite values. Fractional values are legal.</param>
        UniTask<CompleteLevelResponse> CompleteCampaignLevel(Dictionary<string, double> metrics);

        /// <summary>
        /// Deserializes the level's LevelJsonData into the game-defined config type.
        /// Throws <see cref="InvalidOperationException"/> if the level is locked (LevelJsonData is null).
        /// </summary>
        T GetCampaignLevelConfig<T>(CampaignLevelDto level);

        /// <summary>
        /// Parses the level's StarRulesJson for display purposes.
        /// Returns null if the rules are absent (locked level) or unparsable.
        /// </summary>
        CampaignStarRules GetCampaignStarRules(CampaignLevelDto level);
    }
}
