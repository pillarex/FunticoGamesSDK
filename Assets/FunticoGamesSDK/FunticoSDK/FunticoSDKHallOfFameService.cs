using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.ViewModels;
using FunticoGamesSDK.HallOfFameProviders;

namespace FunticoGamesSDK
{
	public partial class FunticoSDK : IHallOfFameProvider
	{
		private void SetupHallOfFameProvider()
		{
			_hallOfFameProvider = new HallOfFameProvider();
		}

		public UniTask<HallOfFameResponse> GetHallOfFame(
			HallOfFamePeriodMode mode, 
			int page, 
			int limit,
			HallOfFameGameModeFilter gameMode)
		{
			return _hallOfFameProvider.GetHallOfFame(mode, page, limit, gameMode);
		}

		public UniTask<HOFViewModel> GetHallOfFameDistribution()
		{
			return _hallOfFameProvider.GetHallOfFameDistribution();
		}
	}
}
