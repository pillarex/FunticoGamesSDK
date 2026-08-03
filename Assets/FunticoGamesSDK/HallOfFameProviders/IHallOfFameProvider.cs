using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.ViewModels;

namespace FunticoGamesSDK.HallOfFameProviders
{
    public interface IHallOfFameProvider
    {
        /// <summary>
        /// Fetches a page of the Hall of Fame ranking.
        /// </summary>
        /// <param name="mode">Global (all-time) or Monthly.</param>
        /// <param name="page">1-based page index.</param>
        /// <param name="limit">Entries per page.</param>
        /// <param name="gameMode">Restricts the ranking to a single mode; All applies no filter.</param>
        /// <param name="year">Calendar year for <see cref="HallOfFamePeriodMode.Monthly"/>. Null falls back to the current UTC year. Ignored for Global.</param>
        /// <param name="month">Calendar month, 1-12, for <see cref="HallOfFamePeriodMode.Monthly"/>. Null falls back to the current UTC month. Ignored for Global.</param>
        UniTask<HallOfFameResponse> GetHallOfFame(
            HallOfFamePeriodMode mode,
            int page,
            int limit,
            HallOfFameGameModeFilter gameMode,
            int? year = null,
            int? month = null);
            
        UniTask<HOFViewModel> GetHallOfFameDistribution();
    }
}
