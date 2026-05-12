using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.Encryption;
using FunticoGamesSDK.NetworkUtils;
using FunticoGamesSDK.ViewModels;
using Newtonsoft.Json;
using Logger = FunticoGamesSDK.Logging.Logger;

namespace FunticoGamesSDK.TournamentsProviders
{
    public abstract class TournamentsProviderInternal
    {
        protected readonly string PrivateKey;

        protected TournamentsProviderInternal(string privateKey)
        {
            PrivateKey = privateKey;
        }

        #region Public

        public async UniTask<List<TournamentViewModel>> GetTournaments(int page = 1, int perPage = 100, TournamentsFilterEnum? filter = null)
        {
            var url = $"{APIConstants.Get_Tournaments}?page={page}&perPage={perPage}";
            if (filter.HasValue)
                url += $"&filterEnum={(int)filter.Value}";

            (bool success, List<TournamentViewModel> response) = await HTTPClient.Get<List<TournamentViewModel>>(url);
            if (!success)
                Logger.LogError("Failed to get tournaments");
            return response ?? new List<TournamentViewModel>();
        }

        public async UniTask<TournamentViewModel> GetTournamentDetailsLight(Guid tournamentId)
        {
            var url = $"{APIConstants.Get_Tournament_Details_Light}?tournamentId={tournamentId}";
            (bool success, TournamentViewModel response) = await HTTPClient.Get<TournamentViewModel>(url);
            if (!success)
                Logger.LogError("Failed to get tournament details");
            return response;
        }

        public async UniTask<TournamentLeaderboardViewModel> GetTournamentLeaderboard(Guid tournamentId)
        {
            var url = $"{APIConstants.Get_Tournament_Leaderboard}?tournamentId={tournamentId}";
            (bool success, TournamentLeaderboardViewModel response) = await HTTPClient.Get<TournamentLeaderboardViewModel>(url);
            if (!success)
                Logger.LogError("Failed to get tournament leaderboard");
            return response;
        }

        public async UniTask<List<TournamentCommunityViewModel>> GetTournamentCommunities(Guid tournamentId)
        {
            var url = $"{APIConstants.Get_Tournament_Communities}?tournamentId={tournamentId}";
            (bool success, List<TournamentCommunityViewModel> response) = await HTTPClient.Get<List<TournamentCommunityViewModel>>(url);
            if (!success)
                Logger.LogError("Failed to get tournament communities");
            return response ?? new List<TournamentCommunityViewModel>();
        }

        public async UniTask<TournamentPrizesViewModel> GetPrizes(Guid tournamentId)
        {
            var url = $"{APIConstants.Get_Tournament_Prizes}?tournamentId={tournamentId}";
            (bool success, TournamentPrizesViewModel response) = await HTTPClient.Get<TournamentPrizesViewModel>(url);
            if (!success)
                Logger.LogError("Failed to get tournament prizes");
            return response ?? new TournamentPrizesViewModel();
        }

        public async UniTask<TournamentHistoryViewModel> GetTournamentsHistory(int page = 1, int perPage = 10)
        {
            var url = $"{APIConstants.Get_Tournaments_History}?page={page}&per_page={perPage}";
            (bool success, TournamentHistoryResponse response) = await HTTPClient.Get<TournamentHistoryResponse>(url);
            if (!success)
                Logger.LogError("Failed to get tournaments history");
            if (response == null) return new TournamentHistoryViewModel();
            return new TournamentHistoryViewModel { Entries = response.Data, Meta = response.Meta };
        }

        #endregion

        #region Protected API calls (used by TournamentsProvider)

        protected async UniTask<bool> SendJoinTournament(Guid tournamentId, Guid? communityId, string password)
        {
            var url = $"{APIConstants.Get_Join_Tournament}?tournamentId={tournamentId}";
            if (communityId.HasValue)
                url += $"&communityId={communityId.Value}";
            if (!string.IsNullOrEmpty(password))
                url += $"&password={Uri.EscapeDataString(password)}";

            (bool success, string _) = await HTTPClient.Get<string>(url);
            if (!success)
                Logger.LogError("Failed to join tournament");
            return success;
        }

        protected async UniTask<TournamentEnterData> SendEnterTournament(Guid tournamentId)
        {
            var url = $"{APIConstants.Get_Enter_Tournament}?tournamentId={tournamentId}";
            (bool success, TournamentEnterData response) = await HTTPClient.Get<TournamentEnterData>(url);
            if (!success)
                Logger.LogError("Failed to enter tournament");
            return response;
        }

        protected async UniTask<TournamentResultViewModel> SendTournamentResult(Guid tournamentId, string saveScoreId, TournamentSaveScoreRequest request)
        {
            var url = $"{APIConstants.Post_Tournament_Result}?tournamentId={tournamentId}&saveScoreId={saveScoreId}";
            (bool success, TournamentResultViewModel response) = await HTTPClient.Post<TournamentResultViewModel>(url, request);
            if (!success)
                Logger.LogError("Failed to submit tournament result");
            return response;
        }

        protected async UniTask<bool> SendTournamentResultClient(Guid tournamentId, string saveScoreId, int? score, bool isSuccess, string customData)
        {
            var payload = new TournamentSaveScorePayload
            {
                Score = score,
                IsSuccess = isSuccess,
                CustomData = customData,
            };

            var gameDataKey = saveScoreId.Substring(0, 8) + PrivateKey.Substring(0, 8);
            var encrypted = AESNonDynamic.Encrypt(JsonConvert.SerializeObject(payload), gameDataKey);
            var hash = HashUtils.HashString(encrypted, PrivateKey.Substring(8));

            var request = new RoomSaveScoreRequestEncrypted
            {
                EncryptedData = encrypted,
                TournamentId = tournamentId,
                Hash = hash,
            };

            var url = $"{APIConstants.Post_Tournament_Result_Client}?saveScoreId={saveScoreId}";
            var success = await HTTPClient.Post_Short(url, request);
            if (!success)
                Logger.LogError("Failed to submit encrypted tournament result");
            return success;
        }

        #endregion
    }
}
