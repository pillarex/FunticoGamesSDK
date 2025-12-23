using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.APIModels.UserData;

namespace FunticoGamesSDK.UserDataProviders
{
	public interface IUserDataService
	{
		List<VoucherData> GetCachedVouchers();
		UniTask<List<VoucherData>> GetVouchers(bool useCache = true);
		BalanceResponse GetCachedBalance();
		UniTask<BalanceResponse> GetBalance(bool useCache = true);
		UserData GetCachedUserData();
		UniTask<UserData> GetUserData(bool useCache = true);
		bool CanAffordFromCache(EntryFeeType type, int amount);
		UniTask<bool> CanAfford(EntryFeeType type, int amount);
	}
}