using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.ViewModels;

namespace FunticoGamesSDK.RoomsProviders
{
    public interface IRoomsProvider
    {
        public UniTask<List<TierViewModel>> GetTiers();
        public UniTask<RoomViewModel> GetRoom(string guid);
        public UniTask<string> GetRoomSettings(string guid);
        public UniTask<List<RoomViewModel>> GetRooms(RoomTierEnum? tier);
        public UniTask<RoomData> JoinRoom(string roomGuid);
        public UniTask<PrizePoolDistibutionViewModel> GetPrizePoolDistribution(string roomGuid);
        public UniTask<RoomLeaderboardViewModel> GetLeaderboard(string eventId, string sessionId, string matchId);
        public UniTask<RoomTierEnum?> GetTierByEventId(string eventId);
        public UniTask<bool> FinishRoomSession_Client(string eventId, string sessionId, int score);
        public UniTask<bool> FinishRoomSession_Server(string eventId, string sessionId, int score, int userId, int funticoUserId, string userIp);
        public UniTask<bool> FinishRoomSession_Server(string eventId, string sessionId, List<FinishedUser> participants);
    }
}