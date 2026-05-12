using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.TournamentsProviders;
using FunticoGamesSDK.ViewModels;

namespace FunticoGamesSDK
{
    public partial class FunticoSDK : ITournamentsProvider
    {
        private ITournamentsProvider _tournamentsProvider;

        private void SetupTournamentsProvider()
        {
            _tournamentsProvider = new TournamentsProvider(_privateGameKey, _userDataService);
        }

        public UniTask<List<TournamentViewModel>> GetTournaments(int page = 1, int perPage = 100, TournamentsFilterEnum? filter = null)
            => _tournamentsProvider.GetTournaments(page, perPage, filter);

        public UniTask<TournamentViewModel> GetTournamentDetailsLight(Guid tournamentId)
            => _tournamentsProvider.GetTournamentDetailsLight(tournamentId);

        public UniTask<TournamentLeaderboardViewModel> GetTournamentLeaderboard(Guid tournamentId)
            => _tournamentsProvider.GetTournamentLeaderboard(tournamentId);

        public UniTask<List<TournamentCommunityViewModel>> GetTournamentCommunities(Guid tournamentId)
            => _tournamentsProvider.GetTournamentCommunities(tournamentId);

        public UniTask<bool> JoinTournament(Guid tournamentId, Guid? communityId = null, string password = null)
            => _tournamentsProvider.JoinTournament(tournamentId, communityId, password);

        public UniTask<TournamentEnterData> EnterTournament(Guid tournamentId)
            => _tournamentsProvider.EnterTournament(tournamentId);

        public UniTask<TournamentResultViewModel> ResultTournament(Guid tournamentId, string saveScoreId, int? score, bool isSuccess)
            => _tournamentsProvider.ResultTournament(tournamentId, saveScoreId, score, isSuccess);

        public UniTask<bool> ResultTournament_Client(Guid tournamentId, string saveScoreId, int? score, bool isSuccess, string customData = null)
            => _tournamentsProvider.ResultTournament_Client(tournamentId, saveScoreId, score, isSuccess, customData);

        public UniTask<TournamentPrizesViewModel> GetPrizes(Guid tournamentId)
            => _tournamentsProvider.GetPrizes(tournamentId);

        public UniTask<TournamentHistoryViewModel> GetTournamentsHistory(int page = 1, int perPage = 10)
            => _tournamentsProvider.GetTournamentsHistory(page, perPage);
    }
}
