using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.InventoryProviders;

namespace FunticoGamesSDK
{
	public partial class FunticoSDK : IInventoryProvider
	{
		private IInventoryProvider _inventoryProvider;

		private void SetupInventoryProvider()
		{
			_inventoryProvider = new InventoryProvider();
		}

		public UniTask<InventoryResponse> GetInventory(bool includeOtherItems = false) =>
			_inventoryProvider.GetInventory(includeOtherItems);

		public InventoryErrorResponse GetLastInventoryError() => _inventoryProvider.GetLastInventoryError();

		public T GetInventoryItemProperties<T>(InventoryItem item) =>
			_inventoryProvider.GetInventoryItemProperties<T>(item);

		public UniTask<RecipesResponse> GetRecipes() => _inventoryProvider.GetRecipes();

		public UniTask<CraftResult> CraftItem(string recipe, string id = null, string idempotencyKey = null) =>
			_inventoryProvider.CraftItem(recipe, id, idempotencyKey);

		public UniTask<CraftResult> FuseItems(string fusion, IEnumerable<long> userItemIds, string idempotencyKey = null) =>
			_inventoryProvider.FuseItems(fusion, userItemIds, idempotencyKey);

		public UniTask<RecentRewardsResponse> GetRecentRewards(int limit = 20, System.DateTime? since = null) =>
			_inventoryProvider.GetRecentRewards(limit, since);
	}
}
