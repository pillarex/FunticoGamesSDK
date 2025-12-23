using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.APIModels.UserData;
using FunticoGamesSDK.NetworkUtils;

namespace FunticoGamesSDK.UserDataProviders
{
	public class UserDataService : IUserDataService
	{
		#region Fields & Properties

		private UserData Data { get; set; }
		private BalanceResponse Balance { get; set; }
		private List<VoucherData> Vouchers { get; set; }
		private List<VoucherStaticData> _vouchersStaticData;

		#endregion

		#region Public Methods

		public List<VoucherData> GetCachedVouchers() => Vouchers;

		public async UniTask<List<VoucherData>> GetVouchers(bool useCache = true)
		{
			if (Vouchers == null || !useCache)
			{
				await UpdateVouchers();
			}

			return Vouchers;
		}


		public BalanceResponse GetCachedBalance() => Balance;

		public async UniTask<BalanceResponse> GetBalance(bool useCache = true)
		{
			if (Balance == null || !useCache)
			{
				await UpdateUserBalance();
			}

			return Balance;
		}

		public UserData GetCachedUserData() => Data;

		public async UniTask<UserData> GetUserData(bool useCache = true)
		{
			if (Data == null || !useCache)
			{
				await UpdateUserData();
			}

			return Data;
		}

		public bool CanAffordFromCache(EntryFeeType type, int amount)
		{
			var data = GetCachedBalance();
			switch (type)
			{
				case EntryFeeType.Tico:
					return data.Diamonds >= amount;
				case EntryFeeType.SemifinalsTickets:
					return data.SemiFinalTickets >= amount;
				case EntryFeeType.FinalTickets:
					return data.FinalTickets >= amount;
				case EntryFeeType.PrivateTickets:
					return data.PrivateTickets >= amount;
				case EntryFeeType.Free:
					return true;
				default:
					break;
			}

			return true; // to make API check
		}

		public async UniTask<bool> CanAfford(EntryFeeType type, int amount)
		{
			await GetUserData(false);
			return CanAffordFromCache(type, amount);
		}

		#endregion

		#region Private Methods

		private async UniTask UpdateVouchers()
		{
			if (_vouchersStaticData == null)
			{
				var (_, vouchersStaticData) =
					await HTTPClient.Get<VoucherStaticDataResponse>(APIConstants.VOUCHERS_STATIC_DATA);

				_vouchersStaticData = vouchersStaticData?.Data;
			}

			var (_, response) = await HTTPClient.Get<VoucherResponse>(APIConstants.VOUCHERS);
			ProcessVouchersResponse(response);
		}

		private void ProcessVouchersResponse(VoucherResponse voucherResponse)
		{
			Vouchers.Clear();
			foreach (var staticData in _vouchersStaticData)
			{
				Vouchers.Add(new VoucherData()
				{
					ItemImage = staticData.Image,
					ItemName = staticData.Name,
					ItemId = staticData.Id,
					Tier = (int)staticData.RoomTierEnum,
					PlaysRequiredToActivate = 10
				});
			}

			if (voucherResponse == null)
				return;

			foreach (var voucher in voucherResponse.Data)
			{
				var item = Vouchers.FirstOrDefault(data => data.Tier == voucher.Tier);
				if (item == null)
				{
					Vouchers.Add(voucher);
				}
				else
				{
					item.Count = voucher.Count;
					item.PlayCount = voucher.PlayCount;
					item.PlaysRequiredToActivate = voucher.PlaysRequiredToActivate;
				}
			}
		}

		private async UniTask UpdateUserBalance()
		{
			var (_, balance) = await HTTPClient.Get<BalanceResponse>(APIConstants.USER_BALANCE);
			Balance = balance;
		}

		private async UniTask UpdateUserData()
		{
			var (_, response) = await HTTPClient.Get<UserData>(APIConstants.USER_ALL_DATA);
			Data = response;
		}

		#endregion
	}
}