using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.RoomsProviders;
using FunticoGamesSDK.ViewModels;

namespace FunticoGamesSDK
{
	public partial class FunticoSDK : IRoomsProvider
	{
		private IRoomsProvider _roomsProvider;

		private void SetupRoomsProvider()
		{
			_roomsProvider = new RoomsProvider(_userDataService, _authDataProvider, _errorHandler);
		}

		public UniTask<List<TierViewModel>> GetTiers() => _roomsProvider.GetTiers();

		public UniTask<RoomViewModel> GetRoom(string guid) => _roomsProvider.GetRoom(guid);

		public UniTask<string> GetRoomSettings(string guid) => _roomsProvider.GetRoomSettings(guid);

		public UniTask<List<RoomViewModel>> GetRooms(RoomTierEnum? tier) => _roomsProvider.GetRooms(tier);

		public UniTask<RoomData> JoinRoom(Ticket ticket, string roomGuid, bool useVoucher = false, bool prePaid = false) =>
			_roomsProvider.JoinRoom(ticket, roomGuid, useVoucher, prePaid);

		public UniTask<PrizePoolDistibutionViewModel> GetPrizePoolDistribution(string roomGuid) =>
			_roomsProvider.GetPrizePoolDistribution(roomGuid);

		public UniTask<RoomLeaderboardViewModel> GetLeaderboard(string eventId, string sessionId, string matchId) =>
			_roomsProvider.GetLeaderboard(eventId, sessionId, matchId);

		public UniTask<RoomData> TryToReconnect(Ticket ticket, string guid) => _roomsProvider.TryToReconnect(ticket, guid);

		public UniTask<RoomTierEnum?> GetTierByEventId(string eventId) => _roomsProvider.GetTierByEventId(eventId);
	}
}