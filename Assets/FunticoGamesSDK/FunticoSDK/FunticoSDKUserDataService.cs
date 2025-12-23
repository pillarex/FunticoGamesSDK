using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.APIModels.UserData;
using FunticoGamesSDK.UserDataProviders;

namespace FunticoGamesSDK
{
	public partial class FunticoSDK : IUserDataService
	{
		private IUserDataService _userDataService;

		private void SetupUserDataService()
		{
			_userDataService = new UserDataService();
		}

		public List<VoucherData> GetCachedVouchers() => _userDataService.GetCachedVouchers();

		public UniTask<List<VoucherData>> GetVouchers(bool useCache = true) => _userDataService.GetVouchers(useCache);

		public BalanceResponse GetCachedBalance() => _userDataService.GetCachedBalance();

		public UniTask<BalanceResponse> GetBalance(bool useCache = true) => _userDataService.GetBalance(useCache);

		public UserData GetCachedUserData() => _userDataService.GetCachedUserData();

		public UniTask<UserData> GetUserData(bool useCache = true) => _userDataService.GetUserData(useCache);

		public bool CanAffordFromCache(EntryFeeType type, int amount) =>
			_userDataService.CanAffordFromCache(type, amount);

		public UniTask<bool> CanAfford(EntryFeeType type, int amount) => _userDataService.CanAfford(type, amount);
	}
}