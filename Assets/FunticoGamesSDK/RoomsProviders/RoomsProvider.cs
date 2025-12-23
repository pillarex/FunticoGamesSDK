using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.AssetsProvider;
using FunticoGamesSDK.AuthDataProviders;
using FunticoGamesSDK.NetworkUtils;
using FunticoGamesSDK.TextureResizer;
using FunticoGamesSDK.UserDataProviders;
using FunticoGamesSDK.ViewModels;
using UnityEngine;

namespace FunticoGamesSDK.RoomsProviders
{
    public class RoomsProvider : RoomsProviderInternal, IRoomsProvider
    {
        public RoomsProvider(IUserDataService userDataService, IAuthDataProvider authDataProvider, IErrorHandler errorHandler) : base(userDataService, authDataProvider, errorHandler) { }

        #region Public

        public async UniTask<RoomLeaderboardViewModel> GetLeaderboard(string eventId, string sessionId, string matchId) => 
            await GetLeaderboard(eventId, sessionId, matchId, false);

        public async UniTask<RoomData> JoinRoom(Ticket ticket, string roomGuid, bool useVoucher = false,
            bool prePaid = false)
        {
            if (!prePaid && !useVoucher && !CanAfford(ticket, out var feeType))
            {
                ShowNotEnoughCurrencyError(feeType);
                return null;
            }

            var joinResponse = await JoinSinglePlayerRoom(roomGuid, null, null, useVoucher);
            if (joinResponse == null)
                return null;

            var hasAlreadyJoinedRoom = !string.IsNullOrWhiteSpace(joinResponse.game_session_id_or_match_id);
            var sessionGuid = hasAlreadyJoinedRoom
                ? Guid.Parse(joinResponse.game_session_id_or_match_id)
                : (joinResponse.lobby_quote?.Id ?? Guid.NewGuid());
            var roomData = new RoomData()
            {
                EventId = roomGuid,
            };
            if (!hasAlreadyJoinedRoom)
            {
                var enterResponse = await EnterSinglePlayerRoom(roomGuid, sessionGuid);
                roomData.SessionOrMatchId = enterResponse.id;
            }
            else
            {
                roomData.SessionOrMatchId = sessionGuid.ToString();
            }

            return roomData;
        }

        public async UniTask<RoomData> TryToReconnect(Ticket ticket, string guid)
        {
            // Same flow for reconnects
            return await JoinRoom(ticket, guid, prePaid: true);
        }

        public async UniTask<List<TierViewModel>> GetTiers()
        {
            var tiers = await GetTiersFromAPI();
            var model = new List<TierViewModel>();

            foreach (var tier in tiers.Data)
            {
                model.Add(new TierViewModel
                {
                    Tier = (RoomTierEnum) tier.Tier,
                    TierName = tier.Name,
                    EntryFeeLowerBound = tier.LowerBoundEntryFee,
                    EntryFeeUpperBound = tier.UpperBoundEntryFee,
                    TierImage = AssetsLoader.LoadSpriteAsync(tier.Image, new TextureResizeOptions(Constants.ROOM_COVER_PREFERRED_SIZE)).AsTask(),
                    Hidden = tier.Hidden,
                });
            }

            return model;
        }

        public async UniTask<RoomTierEnum?> GetTierByEventId(string eventId)
        {
            if (LastRoomsResponse == null)
            {
                await UpdateRoomsList(null);
            }

            var tier = LastRoomsResponse?.FirstOrDefault(config => config.Id == eventId)?.Details.Tier;
            return tier == null ? null : (RoomTierEnum) tier.Value;
        }

        #endregion

        #region Private

        protected override int GetRoomType() => 0;

        private bool CanAfford(Ticket ticket, out EntryFeeType feeType)
        {
            feeType = EntryFeeType.Tico;
            return ticket.Type switch
            {
                TicketType.Free => true,
                TicketType.Currency => ticket.CurrencyAmount != null &&
                                       UserDataService.CanAffordFromCache(feeType, (int) ticket.CurrencyAmount),
                TicketType.Item => true, // TODO
                _ => true
            };
        }

        public void ShowNotEnoughCurrencyError(EntryFeeType feeType)
        {
            var text = "You don't have enough currency to participate!";
            var ticketName = feeType.GetString();
            var buttonText = IErrorHandler.DefaultButtonText;
            Action action = () => Application.OpenURL(APIConstants.FUNTICO_SHOP);
            var defaultDontEnough = $"You don't have enough {ticketName} to participate!";

            switch (feeType)
            {
                case EntryFeeType.Tico:
                    action = () => Application.OpenURL(APIConstants.WithQuery(APIConstants.FUNTICO_BASE, "wallet_open=open"));
                    text = defaultDontEnough;
                    break;
                case EntryFeeType.SemifinalsTickets:
                case EntryFeeType.FinalTickets:
                case EntryFeeType.PrivateTickets:
                    text = $"You don't have enough {ticketName} to participate!\nGet more {ticketName} by engaging with the Funtico platform features (Daily Spin, Tico Bar, Capsules, etc.)";
                    break;
            }

            ErrorHandler.ShowError(text, buttonText: buttonText, additionalActionOnCloseClick: action);
        }

        #endregion

        #region RoomsAPI

        private async UniTask<TierResponse> GetTiersFromAPI()
        {
            (bool success, TierResponse response) = await HTTPClient.Get<TierResponse>(APIConstants.Get_Tiers);
            if (!success)
                Debug.LogError("Failed to get tiers");
            return response;
        }

        private async UniTask<APIModels.JoinRoomResponse> JoinSinglePlayerRoom(string roomId, string communityID,
            string password, bool useVoucher)
        {
            var link = APIConstants.WithQuery(APIConstants.Post_Join_Room, $"idempotencyKey={Guid.NewGuid()}",
                $"getLobbyQuote=true");
            (bool ok, var response) = await HTTPClient.Post<APIModels.JoinRoomResponse>(link, new
            {
                tournamentId = roomId,
                communityId = communityID,
                password = string.IsNullOrWhiteSpace(password) ? string.Empty : password,
                UseVoucher = useVoucher,
            }, www => HTTPClient.DefaultErrorHandler(www));

            if (!ok)
            {
                Debug.LogError("Failed to join room");
                return null;
            }

            return response;
        }

        private async UniTask<EnterRoomResponse> EnterSinglePlayerRoom(string roomId, Guid sessionId)
        {
            var link = APIConstants.WithQuery(APIConstants.Post_Room_Started, $"eventId={roomId}",
                $"sessionOrMatchId={sessionId.ToString()}", "isLobby=true");
            (bool ok, var response) = await HTTPClient.Post<EnterRoomResponse>(link, null);

            if (!ok)
            {
                Debug.LogError("Failed to enter room");
                return null;
            }

            return response;
        }

        #endregion
    }
}