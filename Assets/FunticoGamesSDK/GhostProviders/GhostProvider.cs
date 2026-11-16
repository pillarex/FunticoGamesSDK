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

namespace FunticoGamesSDK.GhostProviders
{
    public class GhostProvider : IGhostProvider
    {
        private const int MaxExclusions = 20;

        private readonly IPayloadSigner _signer;
        private GhostErrorResponse _lastError;

        public GhostProvider(IPayloadSigner signer)
        {
            _signer = signer;
        }

        #region Public

        public async UniTask<GhostPoolsResponse> GetGhostPools()
        {
            var response = await Get<GhostPoolsResponse>(APIConstants.Get_Ghost_Pools, "pools");
            if (response == null)
                return null;

            response.Pools ??= new List<GhostPool>();
            response.Pools.RemoveAll(pool => pool == null);
            foreach (var pool in response.Pools)
            {
                pool.Attributes ??= new List<GhostPoolAttribute>();
                pool.Attributes.RemoveAll(attribute => attribute == null);
            }

            return response;
        }

        public async UniTask<GhostRunResponse> GetGhostRun(string pool) =>
            Normalize(await Get<GhostRunResponse>(PoolUrl(APIConstants.Get_Ghost_Run, pool), "run"));

        public async UniTask<GhostRunResponse> StartGhostRun(string pool) =>
            Normalize(await Post<GhostRunResponse>(PoolUrl(APIConstants.Post_Ghost_Run_Start, pool), null, "run start"));

        public async UniTask<GhostRunResponse> AbandonGhostRun(string pool) =>
            Normalize(await Post<GhostRunResponse>(PoolUrl(APIConstants.Post_Ghost_Run_Abandon, pool), null, "run abandon"));

        public async UniTask<GhostFindResponse> FindGhostOpponent(string pool, Dictionary<string, int> attributes,
            string mySnapshotJson = null, IEnumerable<Guid> exclude = null, bool allowBots = true)
        {
            var request = new GhostFindRequest
            {
                Pool = pool,
                Attributes = attributes ?? new Dictionary<string, int>(),
                Snapshot = mySnapshotJson,
                Exclude = FirstDistinct(exclude),
                AllowBots = allowBots
            };

            var response = await Post<GhostFindResponse>(APIConstants.Post_Ghost_Find, request, "find");
            if (response == null)
                return null;

            if (response.Match == null)
                return Fail<GhostFindResponse>(GhostErrorCodes.RequestFailed, "The find response has no match.", "find");

            Normalize(response.Match);
            return response;
        }

        public UniTask<GhostUploadResponse> UploadGhostSnapshot(string pool, Dictionary<string, int> attributes, string snapshotJson)
        {
            var request = new GhostUploadRequest
            {
                Pool = pool,
                Attributes = attributes ?? new Dictionary<string, int>(),
                Snapshot = snapshotJson
            };

            return Post<GhostUploadResponse>(APIConstants.Post_Ghost_Upload, request, "upload");
        }

        public async UniTask<GhostReportResponse> ReportGhostResult(Guid matchId, GhostOutcome outcome, string fightHash = null)
        {
            if (!Enum.IsDefined(typeof(GhostOutcome), outcome))
                return Fail<GhostReportResponse>(GhostErrorCodes.InvalidOutcome,
                    "Outcome must be Win, Loss or Draw.", "report");

            // Only a key-based signer needs the key; a game's own signer holds it elsewhere.
            if (_signer is LocalPayloadSigner local && !local.HasKey)
                return Fail<GhostReportResponse>(GhostErrorCodes.RequestFailed,
                    "The game's private key is not set; pass it to Initialize.", "report");

            var payload = new GhostReportPayload
            {
                MatchId = matchId,
                Outcome = outcome.ToString(),
                FightHash = fightHash
            };

            var signed = await PayloadSigning.Sign(_signer, PayloadKind.Ghost, matchId.ToString(), payload);
            if (signed == null)
                return Fail<GhostReportResponse>(GhostErrorCodes.RequestFailed,
                    "The result could not be signed, so it was not sent. Retry with the same match id.", "report");

            var request = new GhostReportRequest
            {
                MatchId = matchId,
                EncryptedData = signed.Data,
                Hash = signed.Hash
            };

            return await Post<GhostReportResponse>(APIConstants.Post_Ghost_Report, request, "report");
        }

        public UniTask<GhostReportSummaryResponse> GetGhostReportSummary(string pool) =>
            Get<GhostReportSummaryResponse>(PoolUrl(APIConstants.Get_Ghost_Report_Summary, pool), "report summary");

        public async UniTask<GhostHistoryResponse> GetGhostHistory(string pool, int page = 1, int limit = 20)
        {
            var url = APIConstants.WithQuery(APIConstants.Get_Ghost_History,
                "pool=" + Escape(pool),
                "page=" + page.ToString(CultureInfo.InvariantCulture),
                "limit=" + limit.ToString(CultureInfo.InvariantCulture));

            var response = await Get<GhostHistoryResponse>(url, "history");
            if (response == null)
                return null;

            response.Items ??= new List<GhostHistoryItem>();
            response.Items.RemoveAll(item => item == null);
            return response;
        }

        public async UniTask<GhostLeaderboardResponse> GetGhostLeaderboard(string pool, int page = 1, int limit = 20, string partition = null)
        {
            var query = new List<string>
            {
                "pool=" + Escape(pool),
                "page=" + page.ToString(CultureInfo.InvariantCulture),
                "limit=" + limit.ToString(CultureInfo.InvariantCulture)
            };
            if (!string.IsNullOrEmpty(partition))
                query.Add("partition=" + Escape(partition));

            var response = await Get<GhostLeaderboardResponse>(APIConstants.WithQuery(APIConstants.Get_Ghost_Leaderboard, query.ToArray()), "leaderboard");
            if (response == null)
                return null;

            response.Items ??= new List<GhostLeaderboardEntry>();
            response.Items.RemoveAll(entry => entry == null);
            return response;
        }

        public GhostErrorResponse GetLastGhostError() => _lastError;

        #endregion

        #region Private

        private async UniTask<T> Get<T>(string url, string action) where T : class
        {
            GhostErrorResponse error = null;
            (bool success, T response) = await HTTPClient.Get<T>(url, www => error = ReadError(www));
            return Complete(success, response, error, action);
        }

        private async UniTask<T> Post<T>(string url, object body, string action) where T : class
        {
            GhostErrorResponse error = null;
            (bool success, T response) = await HTTPClient.Post<T>(url, body, www => error = ReadError(www));
            return Complete(success, response, error, action);
        }

        private T Complete<T>(bool success, T response, GhostErrorResponse error, string action) where T : class
        {
            if (success && response != null)
            {
                _lastError = null;
                return response;
            }

            // No error captured means the request went through but the body could not be read.
            _lastError = error ?? new GhostErrorResponse
            {
                Error = GhostErrorCodes.RequestFailed,
                Message = $"The ghost {action} response could not be read."
            };
            Log(action);
            return null;
        }

        private T Fail<T>(string code, string message, string action) where T : class
        {
            _lastError = new GhostErrorResponse { Error = code, Message = message };
            Log(action);
            return null;
        }

        private void Log(string action)
        {
            var text = $"Ghost {action} failed: {_lastError.Error} {_lastError.Message}";
            // No opponent available is a normal outcome, not a fault.
            if (_lastError.Error == GhostErrorCodes.NoOpponent)
                Logger.Log(text);
            else
                Logger.LogError(text);
        }

        /// <summary>
        /// Turns a failed request into a <see cref="GhostErrorResponse"/>: the structured
        /// { Error, Message } body when the server sent one, an SDK-side code otherwise.
        /// </summary>
        private static GhostErrorResponse ReadError(UnityWebRequest www)
        {
            if (www.responseCode == 401)
            {
                return new GhostErrorResponse
                {
                    Error = GhostErrorCodes.Unauthorized,
                    Message = "The player is not authenticated."
                };
            }

            var body = www.downloadHandler?.text;
            if (www.responseCode >= 400 && !string.IsNullOrEmpty(body))
            {
                try
                {
                    var error = JsonConvert.DeserializeObject<GhostErrorResponse>(body);
                    if (error != null && !string.IsNullOrEmpty(error.Error))
                        return error;
                }
                catch (Exception)
                {
                    // Not a structured ghost error body — fall through to RequestFailed.
                }
            }

            var reason = www.responseCode > 0 ? $"HTTP {www.responseCode}" : www.error;
            return new GhostErrorResponse
            {
                Error = GhostErrorCodes.RequestFailed,
                Message = $"The ghost request failed: {reason}"
            };
        }

        private static string PoolUrl(string url, string pool) => APIConstants.WithQuery(url, "pool=" + Escape(pool));

        private static string Escape(string value) => Uri.EscapeDataString(value ?? string.Empty);

        private static List<Guid> FirstDistinct(IEnumerable<Guid> ids)
        {
            var result = new List<Guid>();
            if (ids == null)
                return result;

            foreach (var id in ids)
            {
                if (id == Guid.Empty || result.Contains(id))
                    continue;
                result.Add(id);
                if (result.Count == MaxExclusions)
                    break;
            }

            return result;
        }

        private static GhostRunResponse Normalize(GhostRunResponse response)
        {
            if (response?.OpenMatch != null)
                Normalize(response.OpenMatch);
            return response;
        }

        /// <summary>Guarantees a non-null attributes dictionary so callers can read it without null checks.</summary>
        private static void Normalize(GhostMatch match)
        {
            match.OpponentAttributes ??= new Dictionary<string, int>();
        }

        /// <summary>Plaintext payload that gets AES-encrypted before sending.</summary>
        private class GhostReportPayload
        {
            public Guid MatchId { get; set; }
            public string Outcome { get; set; }
            public string FightHash { get; set; }
        }

        #endregion
    }
}
