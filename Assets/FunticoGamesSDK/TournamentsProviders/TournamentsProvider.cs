using System;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.UserDataProviders;
using FunticoGamesSDK.ViewModels;

namespace FunticoGamesSDK.TournamentsProviders
{
    public class TournamentsProvider : TournamentsProviderInternal, ITournamentsProvider
    {
        private readonly IUserDataService _userDataService;

        public TournamentsProvider(string privateKey, IUserDataService userDataService) : base(privateKey)
        {
            _userDataService = userDataService;
        }

        public async UniTask<bool> JoinTournament(Guid tournamentId, Guid? communityId = null, string password = null)
            => await SendJoinTournament(tournamentId, communityId, password);

        public async UniTask<TournamentEnterData> EnterTournament(Guid tournamentId)
            => await SendEnterTournament(tournamentId);

        public async UniTask<TournamentResultViewModel> ResultTournament(Guid tournamentId, string saveScoreId, int? score, bool isSuccess)
        {
            var userData = _userDataService.GetCachedUserData();
            var request = new TournamentSaveScoreRequest
            {
                UserId = userData.UserId,
                Score = score,
                Tournament_uuid = tournamentId.ToString(),
                Tournament_score_uuid = saveScoreId,
                IsSuccess = isSuccess,
            };
            return await SendTournamentResult(tournamentId, saveScoreId, request);
        }

        public async UniTask<bool> ResultTournament_Client(Guid tournamentId, string saveScoreId, int? score, bool isSuccess, string customData = null)
            => await SendTournamentResultClient(tournamentId, saveScoreId, score, isSuccess, customData);
    }
}
