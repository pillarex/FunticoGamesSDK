using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.ViewModels;

namespace FunticoGamesSDK.TournamentsProviders
{
    public interface ITournamentsProvider
    {
        UniTask<List<TournamentViewModel>> GetTournaments(int page = 1, int perPage = 100, TournamentsFilterEnum? filter = null);
        UniTask<TournamentViewModel> GetTournamentDetailsLight(Guid tournamentId);
        UniTask<TournamentLeaderboardViewModel> GetTournamentLeaderboard(Guid tournamentId);
        UniTask<List<TournamentCommunityViewModel>> GetTournamentCommunities(Guid tournamentId);
        UniTask<bool> JoinTournament(Guid tournamentId, Guid? communityId = null, string password = null);
        UniTask<TournamentEnterData> EnterTournament(Guid tournamentId);
        UniTask<TournamentResultViewModel> ResultTournament(Guid tournamentId, string saveScoreId, int? score, bool isSuccess);
        UniTask<bool> ResultTournament_Client(Guid tournamentId, string saveScoreId, int? score, bool isSuccess, string customData = null);
        UniTask<TournamentPrizesViewModel> GetPrizes(Guid tournamentId);
        UniTask<TournamentHistoryViewModel> GetTournamentsHistory(int page = 1, int perPage = 10);
    }
}
