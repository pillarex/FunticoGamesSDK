using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.AuthDataProviders;
using FunticoGamesSDK.RoomsProviders;
using FunticoGamesSDK.SessionsManagement;
using FunticoGamesSDK.UserDataProviders;
using FunticoGamesSDK.ViewModels;

namespace FunticoGamesSDK
{
	public partial class FunticoSDK : IRoomsProvider
	{
		private IRoomsProvider _roomsProvider;

		private void SetupRoomsProvider(string privateGameKey, IUserDataService userDataService,
			IAuthDataProvider authDataProvider, IClientSessionManager clientSessionManager,
			IServerSessionManager serverSessionManager, IErrorHandler errorHandler)
		{
			_roomsProvider = new RoomsProvider(privateGameKey, userDataService, authDataProvider, clientSessionManager,
				serverSessionManager, errorHandler);
		}

		public UniTask<List<TierViewModel>> GetTiers() => _roomsProvider.GetTiers();

		public UniTask<RoomViewModel> GetRoom(string guid) => _roomsProvider.GetRoom(guid);

		public UniTask<string> GetRoomSettings(string guid) => _roomsProvider.GetRoomSettings(guid);

		public UniTask<List<RoomViewModel>> GetRooms(RoomTierEnum? tier) => _roomsProvider.GetRooms(tier);

		public UniTask<RoomData> JoinRoom(string roomGuid) => _roomsProvider.JoinRoom(roomGuid);

		public UniTask<PrizePoolDistibutionViewModel> GetPrizePoolDistribution(string roomGuid) =>
			_roomsProvider.GetPrizePoolDistribution(roomGuid);

		public UniTask<RoomLeaderboardViewModel> GetLeaderboard(string eventId, string sessionId, string matchId) =>
			_roomsProvider.GetLeaderboard(eventId, sessionId, matchId);

		public UniTask<RoomTierEnum?> GetTierByEventId(string eventId) => _roomsProvider.GetTierByEventId(eventId);

		public UniTask<bool> FinishRoomSession_Client(string eventId, string sessionId, int score) =>
			_roomsProvider.FinishRoomSession_Client(eventId, sessionId, score);

		public UniTask<bool> FinishRoomSession_Server(string eventId, string sessionId, int score, int userId, string userIp) =>
			_roomsProvider.FinishRoomSession_Server(eventId, sessionId, score, userId, userIp);

		public UniTask<bool> FinishRoomSession_Server(string eventId, string sessionId, List<FinishedUser> participants) => 
			_instance.FinishRoomSession_Server(eventId, sessionId, participants);
	}
}