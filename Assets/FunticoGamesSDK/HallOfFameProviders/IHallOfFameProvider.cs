using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.ViewModels;

namespace FunticoGamesSDK.HallOfFameProviders
{
    public interface IHallOfFameProvider
    {
        UniTask<HallOfFameResponse> GetHallOfFame(
            HallOfFamePeriodMode mode, 
            int page, 
            int limit,
            HallOfFameGameModeFilter gameMode);
            
        UniTask<HOFViewModel> GetHallOfFameDistribution();
    }
}
