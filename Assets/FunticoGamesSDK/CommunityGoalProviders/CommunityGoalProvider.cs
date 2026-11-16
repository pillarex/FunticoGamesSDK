using System;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.Encryption;
using FunticoGamesSDK.NetworkUtils;
using Newtonsoft.Json;
using UnityEngine.Networking;
using Logger = FunticoGamesSDK.Logging.Logger;

namespace FunticoGamesSDK.CommunityGoalProviders
{
    public class CommunityGoalProvider : ICommunityGoalProvider
    {
        private const int MinPrivateKeyLength = 9;

        private readonly string _privateKey;
        private CommunityGoalErrorResponse _lastError;

        public CommunityGoalProvider(string privateKey)
        {
            _privateKey = privateKey;
        }

        #region Public

        // The read methods go through HTTPClient like every other call: it always sends the game key
        // and adds the player's token only when there is one, so they work before sign-in too.

        public async UniTask<CommunityGoalResponse> GetCommunityGoal(string chainKey)
        {
            if (string.IsNullOrEmpty(chainKey))
                return Fail<CommunityGoalResponse>(CommunityGoalErrorCodes.ChainNotFound, "The chain key is empty.", "get");

            var url = APIConstants.GetUrlAPIWithId(APIConstants.Get_Community_Goal, Uri.EscapeDataString(chainKey));
            return Normalize(await Get<CommunityGoalResponse>(url, "get"));
        }

        public async UniTask<CommunityGoalResponse> GetCommunityGoalById(Guid goalId) =>
            Normalize(await Get<CommunityGoalResponse>(GoalUrl(goalId), "get by id"));

        public async UniTask<CommunityGoalLiveResponse> GetCommunityGoalLive(Guid goalId)
        {
            var response = await Get<CommunityGoalLiveResponse>(GoalUrl(goalId) + "/live", "live");
            if (response != null)
                response.MilestonesPassed ??= new List<double>();
            return response;
        }

        public async UniTask<StartGoalAttemptResponse> StartGoalAttempt(Guid goalId)
        {
            var url = APIConstants.WithQuery(APIConstants.Post_Community_Goal_Attempt_Start, "goalId=" + Id(goalId));
            var response = await Post<StartGoalAttemptResponse>(url, null, "attempt start");
            if (response == null)
                return null;

            if (response.Attempt == null)
                return Fail<StartGoalAttemptResponse>(CommunityGoalErrorCodes.RequestFailed,
                    "The attempt start response has no attempt.", "attempt start");

            Normalize(response.Attempt);
            Normalize(response.Goal);
            return response;
        }

        public UniTask<CompleteGoalAttemptResponse> CompleteGoalAttempt(GoalAttemptDto attempt, long contribution,
            double? clientDurationSeconds = null, IDictionary<string, double> metrics = null, string fightHash = null)
        {
            if (attempt == null || attempt.AttemptId == Guid.Empty || attempt.GoalId == Guid.Empty)
                return UniTask.FromResult(Fail<CompleteGoalAttemptResponse>(CommunityGoalErrorCodes.AttemptNotFound,
                    "Pass the attempt returned by StartGoalAttempt.", "attempt complete"));

            if (contribution < 0)
                return UniTask.FromResult(Fail<CompleteGoalAttemptResponse>(CommunityGoalErrorCodes.InvalidContribution,
                    "Contribution must be 0 or more.", "attempt complete"));

            if (string.IsNullOrEmpty(_privateKey) || _privateKey.Length < MinPrivateKeyLength)
                return UniTask.FromResult(Fail<CompleteGoalAttemptResponse>(CommunityGoalErrorCodes.RequestFailed,
                    "The game's private key is not set; pass it to Initialize.", "attempt complete"));

            var payload = new CompleteGoalAttemptPayload
            {
                AttemptId = attempt.AttemptId,
                GoalId = attempt.GoalId,
                Contribution = contribution,
                ClientDurationSeconds = clientDurationSeconds,
                Metrics = metrics != null ? new Dictionary<string, double>(metrics) : null,
                FightHash = fightHash
            };

            var aesKey = Id(attempt.AttemptId).Substring(0, 8) + _privateKey.Substring(0, 8);
            var encrypted = AESNonDynamic.Encrypt(JsonConvert.SerializeObject(payload), aesKey);
            var hash = HashUtils.HashString(encrypted, _privateKey.Substring(8));

            var request = new CompleteGoalAttemptRequestEncrypted
            {
                AttemptId = attempt.AttemptId,
                EncryptedData = encrypted,
                Hash = hash
            };

            return CompleteAttempt(request);
        }

        public async UniTask<CommunityGoalLeaderboardResponse> GetCommunityGoalLeaderboard(Guid goalId, int page = 1, int limit = 20)
        {
            var url = APIConstants.WithQuery(GoalUrl(goalId) + "/leaderboard",
                "page=" + page.ToString(CultureInfo.InvariantCulture),
                "limit=" + limit.ToString(CultureInfo.InvariantCulture));

            var response = await Get<CommunityGoalLeaderboardResponse>(url, "leaderboard");
            if (response == null)
                return null;

            response.Items ??= new List<CommunityGoalLeaderboardEntryDto>();
            response.Items.RemoveAll(entry => entry == null);
            response.Around ??= new List<CommunityGoalLeaderboardEntryDto>();
            response.Around.RemoveAll(entry => entry == null);
            return response;
        }

        public CommunityGoalErrorResponse GetLastCommunityGoalError() => _lastError;

        #endregion

        #region Private

        private async UniTask<CompleteGoalAttemptResponse> CompleteAttempt(CompleteGoalAttemptRequestEncrypted request)
        {
            var response = await Post<CompleteGoalAttemptResponse>(APIConstants.Post_Community_Goal_Attempt_Complete, request, "attempt complete");
            if (response != null)
                response.MilestonesCrossed ??= new List<double>();
            return response;
        }

        private async UniTask<T> Get<T>(string url, string action) where T : class
        {
            CommunityGoalErrorResponse error = null;
            (bool success, T response) = await HTTPClient.Get<T>(url, www => error = ReadError(www));
            return Complete(success, response, error, action);
        }

        private async UniTask<T> Post<T>(string url, object body, string action) where T : class
        {
            CommunityGoalErrorResponse error = null;
            (bool success, T response) = await HTTPClient.Post<T>(url, body, www => error = ReadError(www));
            return Complete(success, response, error, action);
        }

        private T Complete<T>(bool success, T response, CommunityGoalErrorResponse error, string action) where T : class
        {
            if (success && response != null)
            {
                _lastError = null;
                return response;
            }

            // No error captured means the request went through but the body could not be read.
            _lastError = error ?? new CommunityGoalErrorResponse
            {
                Error = CommunityGoalErrorCodes.RequestFailed,
                Message = $"The community goal {action} response could not be read."
            };
            Log(action);
            return null;
        }

        private T Fail<T>(string code, string message, string action) where T : class
        {
            _lastError = new CommunityGoalErrorResponse { Error = code, Message = message };
            Log(action);
            return null;
        }

        private void Log(string action)
        {
            var text = $"Community goal {action} failed: {_lastError.Error} {_lastError.Message}";
            // A closed goal and a used-up daily quota are normal outcomes, not faults.
            if (_lastError.Error == CommunityGoalErrorCodes.GoalClosed || _lastError.Error == CommunityGoalErrorCodes.NoAttemptsLeft)
                Logger.Log(text);
            else
                Logger.LogError(text);
        }

        /// <summary>
        /// Turns a failed request into a <see cref="CommunityGoalErrorResponse"/>: the structured
        /// { Error, Message, NextGoalId, ResetsAt } body when the server sent one, an SDK-side code otherwise.
        /// </summary>
        private static CommunityGoalErrorResponse ReadError(UnityWebRequest www)
        {
            if (www.responseCode == 401)
            {
                return new CommunityGoalErrorResponse
                {
                    Error = CommunityGoalErrorCodes.Unauthorized,
                    Message = "The player is not authenticated."
                };
            }

            var body = www.downloadHandler?.text;
            if (www.responseCode >= 400 && !string.IsNullOrEmpty(body))
            {
                try
                {
                    var error = JsonConvert.DeserializeObject<CommunityGoalErrorResponse>(body);
                    if (error != null && !string.IsNullOrEmpty(error.Error))
                        return error;
                }
                catch (Exception)
                {
                    // Not a structured community goal error body — fall through to RequestFailed.
                }
            }

            var reason = www.responseCode > 0 ? $"HTTP {www.responseCode}" : www.error;
            return new CommunityGoalErrorResponse
            {
                Error = CommunityGoalErrorCodes.RequestFailed,
                Message = $"The community goal request failed: {reason}"
            };
        }

        private static string GoalUrl(Guid goalId) => APIConstants.GetUrlAPIWithId(APIConstants.Get_Community_Goal_By_Id, Id(goalId));

        /// <summary>The id as the server writes it: lowercase, with hyphens.</summary>
        private static string Id(Guid id) => id.ToString("D");

        private static CommunityGoalResponse Normalize(CommunityGoalResponse response)
        {
            if (response == null)
                return null;

            if (response.Chain != null)
                response.Chain.Milestones ??= new List<double>();
            Normalize(response.Goal);
            Normalize(response.Me?.OpenAttempt);
            return response;
        }

        /// <summary>Guarantees non-null lists so callers can read them without null checks.</summary>
        private static void Normalize(CommunityGoalDto goal)
        {
            if (goal != null)
                goal.MilestonesPassed ??= new List<double>();
        }

        private static void Normalize(GoalAttemptDto attempt)
        {
            if (attempt != null)
                attempt.MilestonesAtStart ??= new List<double>();
        }

        /// <summary>Plaintext payload that gets AES-encrypted before sending.</summary>
        private class CompleteGoalAttemptPayload
        {
            public Guid AttemptId { get; set; }
            public Guid GoalId { get; set; }
            public long Contribution { get; set; }
            public double? ClientDurationSeconds { get; set; }
            public Dictionary<string, double> Metrics { get; set; }
            public string FightHash { get; set; }
        }

        #endregion
    }
}
