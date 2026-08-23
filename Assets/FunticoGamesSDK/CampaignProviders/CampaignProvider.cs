using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.Encryption;
using FunticoGamesSDK.NetworkUtils;
using FunticoGamesSDK.SessionsManagement;
using Newtonsoft.Json;
using UnityEngine.Networking;
using Logger = FunticoGamesSDK.Logging.Logger;

namespace FunticoGamesSDK.CampaignProviders
{
    public class CampaignProvider : ICampaignProvider
    {
        private const int MetricsMinCount = 1;
        private const int MetricsMaxCount = 32;
        private const int MetricKeyMinLength = 1;
        private const int MetricKeyMaxLength = 64;

        private const string ErrorAttemptNotFound = "AttemptNotFound";
        private const string ErrorCompletionTooFast = "CompletionTooFast";

        private readonly string _privateKey;
        private readonly IClientSessionManager _clientSessionManager;

        private CampaignAvailabilityResponse _availability;
        private CampaignLevelsResponse _levels;
        private StartLevelResponse _activeAttempt;

        public CampaignProvider(string privateKey, IClientSessionManager clientSessionManager)
        {
            _privateKey = privateKey;
            _clientSessionManager = clientSessionManager;
        }

        #region Public

        public async UniTask<CampaignAvailabilityResponse> GetCampaignAvailability(bool useCache = true)
        {
            if (_availability != null && useCache)
                return _availability;

            (bool success, CampaignAvailabilityResponse response) =
                await HTTPClient.Get<CampaignAvailabilityResponse>(APIConstants.Get_Campaign_Availability);
            if (!success || response == null)
            {
                Logger.LogError("Failed to get campaign availability");
                return _availability ?? new CampaignAvailabilityResponse();
            }

            _availability = response;
            return _availability;
        }

        public CampaignAvailabilityResponse GetCachedCampaignAvailability() => _availability;

        public async UniTask<CampaignLevelsResponse> GetCampaignLevels(bool useCache = true)
        {
            if (_levels != null && useCache)
                return _levels;

            (bool success, CampaignLevelsResponse response) =
                await HTTPClient.Get<CampaignLevelsResponse>(APIConstants.Get_Campaign_Levels);
            if (!success || response == null)
            {
                Logger.LogError("Failed to get campaign levels");
                return _levels ?? new CampaignLevelsResponse();
            }

            _levels = response;
            return _levels;
        }

        public CampaignLevelsResponse GetCachedCampaignLevels() => _levels;

        public async UniTask<StartLevelResponse> StartCampaignLevel(Guid levelId)
        {
            var url = $"{APIConstants.Post_Campaign_Start_Level}?levelId={levelId}";
            (bool success, StartLevelResponse response) = await HTTPClient.Post<StartLevelResponse>(url, null,
                www => HTTPClient.DefaultErrorHandler(www));
            if (!success || response == null)
            {
                Logger.LogError("Failed to start campaign level");
                return null;
            }

            _activeAttempt = response;
            return _activeAttempt;
        }

        public StartLevelResponse GetActiveCampaignAttempt() => _activeAttempt;

        public void AbandonCampaignAttempt() => _activeAttempt = null;

        public async UniTask<CompleteLevelResponse> CompleteCampaignLevel(Dictionary<string, double> metrics)
        {
            if (_activeAttempt == null)
            {
                Logger.LogError("No active campaign attempt. Call StartCampaignLevel before CompleteCampaignLevel.");
                return null;
            }

            if (!ValidateMetrics(metrics))
                return null;

            var gameEvents = _clientSessionManager?.GetCurrentSessionEvents_Client() ?? new List<string>();
            var payload = new CampaignCompletePayload
            {
                AttemptId = _activeAttempt.AttemptId,
                LevelId = _activeAttempt.LevelId,
                Metrics = metrics,
                GameEvents = gameEvents
            };

            var attemptIdStr = _activeAttempt.AttemptId.ToString();
            var aesKey = attemptIdStr.Substring(0, 8) + _privateKey.Substring(0, 8);
            var encrypted = AESNonDynamic.Encrypt(JsonConvert.SerializeObject(payload), aesKey);
            var hash = HashUtils.HashString(encrypted, _privateKey.Substring(8));

            var request = new CampaignCompleteLevelRequest
            {
                AttemptId = _activeAttempt.AttemptId,
                EncryptedData = encrypted,
                Hash = hash
            };

            (bool success, CompleteLevelResponse response) = await HTTPClient.Post<CompleteLevelResponse>(
                APIConstants.Post_Campaign_Complete_Level, request,
                www =>
                {
                    ClearAttemptIfDead(www);
                    HTTPClient.DefaultErrorHandler(www);
                });
            if (!success || response == null)
            {
                Logger.LogError("Failed to complete campaign level");
                // _activeAttempt is kept for network/5xx errors so the game may retry;
                // ClearAttemptIfDead already dropped it for AttemptNotFound/CompletionTooFast.
                return null;
            }

            _activeAttempt = null;
            // Progress changed (statuses/stars/unlocks) — drop both caches so the next Get re-fetches.
            _availability = null;
            _levels = null;
            // events were delivered in this submission; the client session itself is untouched
            _clientSessionManager?.ClearSessionEvents_Client();
            return response;
        }

        public T GetCampaignLevelConfig<T>(CampaignLevelDto level)
        {
            if (level == null)
                throw new InvalidOperationException("Cannot read campaign level config: level is null.");
            if (level.LevelJsonData == null)
                throw new InvalidOperationException(
                    $"Cannot read config of campaign level '{level.Name}' ({level.LevelId}): LevelJsonData is null. Locked levels do not expose their config.");

            return JsonConvert.DeserializeObject<T>(level.LevelJsonData);
        }

        public CampaignStarRules GetCampaignStarRules(CampaignLevelDto level)
        {
            if (level == null || string.IsNullOrEmpty(level.StarRulesJson))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<CampaignStarRules>(level.StarRulesJson);
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to parse star rules of campaign level '{level.Name}' ({level.LevelId}): {e.Message}");
                return null;
            }
        }

        #endregion

        #region Private

        private static bool ValidateMetrics(Dictionary<string, double> metrics)
        {
            if (metrics == null || metrics.Count < MetricsMinCount || metrics.Count > MetricsMaxCount)
            {
                Logger.LogError($"Campaign metrics must contain {MetricsMinCount}-{MetricsMaxCount} entries");
                return false;
            }

            foreach (var pair in metrics)
            {
                if (string.IsNullOrEmpty(pair.Key) || pair.Key.Length > MetricKeyMaxLength)
                {
                    Logger.LogError($"Campaign metric keys must be {MetricKeyMinLength}-{MetricKeyMaxLength} characters long");
                    return false;
                }

                if (double.IsNaN(pair.Value) || double.IsInfinity(pair.Value))
                {
                    Logger.LogError($"Campaign metric '{pair.Key}' must be a finite number");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Drops the tracked attempt when the server says it is dead (AttemptNotFound, or
        /// CompletionTooFast — which flips the attempt to Abandoned before responding), so the
        /// game does not retry a submission that can never succeed. Network errors and 5xx
        /// responses keep the attempt intact for retry.
        /// </summary>
        private void ClearAttemptIfDead(UnityWebRequest www)
        {
            if (www.responseCode < 400 || www.responseCode >= 500)
                return;

            var body = www.downloadHandler?.text;
            if (string.IsNullOrEmpty(body))
                return;

            try
            {
                var error = JsonConvert.DeserializeObject<CampaignErrorBody>(body);
                if (error != null && (error.Error == ErrorAttemptNotFound || error.Error == ErrorCompletionTooFast))
                    _activeAttempt = null;
            }
            catch (Exception)
            {
                // Not a structured campaign error body — keep the attempt.
            }
        }

        /// <summary>Plaintext payload that gets AES-encrypted before sending.</summary>
        private class CampaignCompletePayload
        {
            public Guid AttemptId { get; set; }
            public Guid LevelId { get; set; }
            public Dictionary<string, double> Metrics { get; set; }
            public List<string> GameEvents { get; set; }
        }

        /// <summary>Minimal shape of the backend's structured error body.</summary>
        private class CampaignErrorBody
        {
            public string Error { get; set; }
        }

        #endregion
    }
}
