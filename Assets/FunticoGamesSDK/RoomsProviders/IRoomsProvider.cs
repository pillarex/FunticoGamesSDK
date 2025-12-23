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
        public UniTask<RoomData> JoinRoom(Ticket ticket, string roomGuid, bool useVoucher = false, bool prePaid = false);
        public UniTask<PrizePoolDistibutionViewModel> GetPrizePoolDistribution(string roomGuid);
        public UniTask<RoomLeaderboardViewModel> GetLeaderboard(string eventId, string sessionId, string matchId);
        public UniTask<RoomData> TryToReconnect(Ticket ticket, string guid);
        public UniTask<RoomTierEnum?> GetTierByEventId(string eventId);
    }
}