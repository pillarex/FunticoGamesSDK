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
			HallOfFameGameModeFilter gameMode,
			int? year = null,
			int? month = null)
		{
			return _hallOfFameProvider.GetHallOfFame(mode, page, limit, gameMode, year, month);
		}

		public UniTask<HOFViewModel> GetHallOfFameDistribution()
		{
			return _hallOfFameProvider.GetHallOfFameDistribution();
		}
	}
}
