using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.AuthDataProviders;
using FunticoGamesSDK.Encryption;
using FunticoGamesSDK.RoomsProviders;
using FunticoGamesSDK.SessionsManagement;
using FunticoGamesSDK.UserDataProviders;
using FunticoGamesSDK.ViewModels;

namespace FunticoGamesSDK
{
	public partial class FunticoSDK : IRoomsProvider
	{
		private IRoomsProvider _roomsProvider;

		private void SetupRoomsProvider(IPayloadSigner signer, IUserDataService userDataService,
			IAuthDataProvider authDataProvider, IClientSessionManager clientSessionManager,
			IServerSessionManager serverSessionManager, IErrorHandler errorHandler)
		{
			_roomsProvider = new RoomsProvider(signer, userDataService, authDataProvider, clientSessionManager,
				serverSessionManager, errorHandler);
		}

		public UniTask<List<TierViewModel>> GetTiers() => _roomsProvider.GetTiers();

		public UniTask<RoomViewModel> GetRoom(string guid) => _roomsProvider.GetRoom(guid);
		
		public UniTask<string> GetRoomSettings(string guid) => _roomsProvider.GetRoomSettings(guid);
		
		public UniTask<T> GetRoomSettingsObject<T>(string guid) => _roomsProvider.GetRoomSettingsObject<T>(guid);

		public UniTask<List<RoomViewModel>> GetRooms(RoomTierEnum? tier, RoomType roomType = RoomType.singleplayer) =>
			_roomsProvider.GetRooms(tier, roomType);

		public UniTask<RoomData> JoinRoom(string roomGuid) => _roomsProvider.JoinRoom(roomGuid);

		/// <summary>
		/// Registers this player in a match of an external (PvP) room that your own matchmaking formed.
		/// Every player of the match calls it with the same matchId: a GUID your game server makes once
		/// per match. The room's entry fee, if any, is charged here.
		/// When the match is over, report every player's score under the same matchId with
		/// FinishRoomSession_Server (a null score means the player left). Prizes are paid by place once
		/// every player of the match has a score.
		/// Calling it again for the same match returns the same match. If the player still has an
		/// unfinished game in this room under another id, this returns null: finish that game first.
		/// </summary>
		/// <param name="eventId">The room's id.</param>
		/// <param name="matchId">The match's GUID, the same for all of its players.</param>
		/// <param name="password">The room's password, for a room that has one.</param>
		/// <param name="useVoucher">Spend a free-entry voucher instead of the entry fee.</param>
		/// <returns>The room data to report under (SessionOrMatchId is the match id), or null on failure.</returns>
		public UniTask<RoomData> JoinExternalMatch(string eventId, string matchId, string password = null,
			bool useVoucher = false) =>
			_roomsProvider.JoinExternalMatch(eventId, matchId, password, useVoucher);

		public UniTask<PrizePoolDistibutionViewModel> GetPrizePoolDistribution(string roomGuid) =>
			_roomsProvider.GetPrizePoolDistribution(roomGuid);

		public UniTask<RoomLeaderboardViewModel> GetLeaderboard(string eventId, string sessionId, string matchId) =>
			_roomsProvider.GetLeaderboard(eventId, sessionId, matchId);

		public UniTask<RoomTierEnum?> GetTierByEventId(string eventId, RoomType roomType) =>
			_roomsProvider.GetTierByEventId(eventId, roomType);

		public UniTask<bool> FinishRoomSession_Client(string eventId, string sessionId, int score) =>
			_roomsProvider.FinishRoomSession_Client(eventId, sessionId, score);

		public UniTask<bool> FinishRoomSession_Server(string eventId, string sessionId, int? score, int userId, int funticoId, string userIp, long? gameManagedAward = null) =>
			_roomsProvider.FinishRoomSession_Server(eventId, sessionId, score, userId, funticoId, userIp, gameManagedAward);

		public UniTask<bool> FinishRoomSession_Server(string eventId, string sessionId, List<FinishedUser> participants) => 
			_roomsProvider.FinishRoomSession_Server(eventId, sessionId, participants);

		public UniTask<RoomsHistoryResponse> GetHistory(int page, string filters = null, int? limit = 10, string cursor = null) =>
			_roomsProvider.GetHistory(page, filters, limit, cursor);
	}
}