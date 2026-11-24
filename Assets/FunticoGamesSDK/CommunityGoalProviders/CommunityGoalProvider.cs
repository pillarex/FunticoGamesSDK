using System;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.Encryption;
using FunticoGamesSDK.NetworkUtils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Logger = FunticoGamesSDK.Logging.Logger;

namespace FunticoGamesSDK.CommunityGoalProviders
{
    public class CommunityGoalProvider : ICommunityGoalProvider
    {
        private readonly IPayloadSigner _signer;
        private CommunityGoalErrorResponse _lastError;

        public CommunityGoalProvider(IPayloadSigner signer)
        {
            _signer = signer;
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

        public async UniTask<StartGoalAttemptResponse> StartGoalAttempt(Guid goalId, string startData = null, bool paid = false)
        {
            var url = APIConstants.WithQuery(APIConstants.Post_Community_Goal_Attempt_Start, "goalId=" + Id(goalId));
            var body = string.IsNullOrEmpty(startData) && !paid
                ? null
                : new StartGoalAttemptRequest { StartData = string.IsNullOrEmpty(startData) ? null : startData, Paid = paid };
            var response = await Post<StartGoalAttemptResponse>(url, body, "attempt start");
            if (response == null)
                return null;

            if (response.Attempt == null)
                return Fail<StartGoalAttemptResponse>(CommunityGoalErrorCodes.RequestFailed,
                    "The attempt start response has no attempt.", "attempt start");

            Normalize(response.Attempt);
            Normalize(response.Goal);
            return response;
        }

        public async UniTask<CompleteGoalAttemptResponse> CompleteGoalAttempt(GoalAttemptDto attempt, long contribution,
            double? clientDurationSeconds = null, IDictionary<string, double> metrics = null, string fightHash = null,
            string strikeData = null)
        {
            if (attempt == null || attempt.AttemptId == Guid.Empty || attempt.GoalId == Guid.Empty)
                return Fail<CompleteGoalAttemptResponse>(CommunityGoalErrorCodes.AttemptNotFound,
                    "Pass the attempt returned by StartGoalAttempt.", "attempt complete");

            if (contribution < 0)
                return Fail<CompleteGoalAttemptResponse>(CommunityGoalErrorCodes.InvalidContribution,
                    "Contribution must be 0 or more.", "attempt complete");

            // Only a key-based signer needs the key; a game's own signer holds it elsewhere.
            if (_signer is LocalPayloadSigner local && !local.HasKey)
                return Fail<CompleteGoalAttemptResponse>(CommunityGoalErrorCodes.RequestFailed,
                    "The game's private key is not set; pass it to Initialize.", "attempt complete");

            var payload = new CompleteGoalAttemptPayload
            {
                AttemptId = attempt.AttemptId,
                GoalId = attempt.GoalId,
                Contribution = contribution,
                ClientDurationSeconds = clientDurationSeconds,
                Metrics = metrics != null ? new Dictionary<string, double>(metrics) : null,
                FightHash = fightHash,
                StrikeData = string.IsNullOrEmpty(strikeData) ? null : strikeData
            };

            var signed = await PayloadSigning.Sign(_signer, PayloadKind.CommunityGoal, Id(attempt.AttemptId), payload);
            if (signed == null)
                return Fail<CompleteGoalAttemptResponse>(CommunityGoalErrorCodes.RequestFailed,
                    "The result could not be signed, so it was not sent. Retry with the same attempt.", "attempt complete");

            var request = new CompleteGoalAttemptRequestEncrypted
            {
                AttemptId = attempt.AttemptId,
                EncryptedData = signed.Data,
                Hash = signed.Hash
            };

            return await CompleteAttempt(request);
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

        public UniTask<CommunityGoalStrikeDto> GetCommunityGoalStrike(Guid goalId, Guid strikeId) =>
            Get<CommunityGoalStrikeDto>(GoalUrl(goalId) + "/strike/" + Id(strikeId), "strike");

        public async UniTask<CommunityGoalHistoryResponse> GetCommunityGoalHistory(string chainKey, int page = 1, int limit = 10)
        {
            if (string.IsNullOrEmpty(chainKey))
                return Fail<CommunityGoalHistoryResponse>(CommunityGoalErrorCodes.ChainNotFound, "The chain key is empty.", "history");

            var url = APIConstants.WithQuery(
                APIConstants.GetUrlAPIWithId(APIConstants.Get_Community_Goal, Uri.EscapeDataString(chainKey)) + "/history",
                "page=" + page.ToString(CultureInfo.InvariantCulture),
                "limit=" + limit.ToString(CultureInfo.InvariantCulture));

            var response = await Get<CommunityGoalHistoryResponse>(url, "history");
            if (response == null)
                return null;

            response.Items ??= new List<CommunityGoalHistoryEntryDto>();
            response.Items.RemoveAll(entry => entry == null);
            return response;
        }

        public CommunityGoalWatch WatchCommunityGoal(Guid goalId, Action<CommunityGoalLiveResponse> onChange,
            Action<CommunityGoalErrorResponse> onError = null, int minIntervalSeconds = 2, bool pollInBackground = false)
        {
            var watch = new CommunityGoalWatch();
            RunWatch(goalId, onChange, onError, Math.Max(1, minIntervalSeconds), pollInBackground, watch).Forget();
            return watch;
        }

        public CommunityGoalErrorResponse GetLastCommunityGoalError() => _lastError;

        #endregion

        #region Private

        /// <summary>
        /// The polling loop of <see cref="WatchCommunityGoal"/>: one live read, then a wait of the
        /// interval the server asked for, until stopped or the goal is settled (it never changes again).
        /// </summary>
        private async UniTaskVoid RunWatch(Guid goalId, Action<CommunityGoalLiveResponse> onChange,
            Action<CommunityGoalErrorResponse> onError, int minIntervalSeconds, bool pollInBackground, CommunityGoalWatch watch)
        {
            var token = watch.Token;
            var interval = 5;
            string last = null;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    // In the background (no focus on desktop and WebGL, paused on mobile) nobody sees the value.
                    if (!pollInBackground && !Application.isFocused)
                    {
                        await UniTask.WaitUntil(() => Application.isFocused, cancellationToken: token);
                        continue;
                    }

                    var live = await GetCommunityGoalLive(goalId);
                    if (token.IsCancellationRequested)
                        break;

                    if (live != null)
                    {
                        if (live.PollAfterSeconds > 0)
                            interval = live.PollAfterSeconds;

                        var key = $"{live.Status}|{live.CurrentValue}|{live.ContributorCount}|{live.NextGoalId}";
                        if (key != last)
                        {
                            last = key;
                            Notify(onChange, live);
                        }

                        if (live.Status == CommunityGoalStatuses.Settled)
                            break;
                    }
                    else
                    {
                        Notify(onError, GetLastCommunityGoalError());
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(Math.Max(minIntervalSeconds, interval)), true, PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Stopped.
            }
            finally
            {
                watch.MarkStopped();
            }
        }

        /// <summary>A throwing callback must not end the watch.</summary>
        private static void Notify<T>(Action<T> callback, T value)
        {
            if (callback == null)
                return;
            try
            {
                callback(value);
            }
            catch (Exception e)
            {
                Logger.LogError($"Community goal watch callback failed: {e}");
            }
        }

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
            // A closed goal, a used-up daily quota and a paid attempt the player cannot pay for are normal outcomes, not faults.
            if (_lastError.Error == CommunityGoalErrorCodes.GoalClosed || _lastError.Error == CommunityGoalErrorCodes.NoAttemptsLeft ||
                _lastError.Error == CommunityGoalErrorCodes.NoPaidAttemptsLeft || _lastError.Error == CommunityGoalErrorCodes.InsufficientFunds ||
                _lastError.Error == CommunityGoalErrorCodes.PaymentPending)
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
            if (response.Me != null)
            {
                Normalize(response.Me.OpenAttempt);
                response.Me.Rewards ??= new List<RecentReward>();
                response.Me.Rewards.RemoveAll(reward => reward == null);
                foreach (var reward in response.Me.Rewards)
                    reward.Items ??= new List<RecentRewardItem>();
            }
            return response;
        }

        /// <summary>Guarantees non-null lists so callers can read them without null checks.</summary>
        private static void Normalize(CommunityGoalDto goal)
        {
            if (goal == null)
                return;

            goal.MilestonesPassed ??= new List<double>();
            goal.Rewards ??= new List<CommunityGoalBracketDto>();
            goal.Rewards.RemoveAll(bracket => bracket == null);
            foreach (var bracket in goal.Rewards)
            {
                bracket.Display ??= new List<CommunityGoalRewardDisplayDto>();
                bracket.Display.RemoveAll(shown => shown == null);
            }
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
            public string StrikeData { get; set; }
        }

        #endregion
    }
}
