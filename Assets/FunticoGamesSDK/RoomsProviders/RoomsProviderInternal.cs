using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.APIModels.PrizesResponses;
using FunticoGamesSDK.AssetsProvider;
using FunticoGamesSDK.AuthDataProviders;
using FunticoGamesSDK.NetworkUtils;
using FunticoGamesSDK.TextureResizer;
using FunticoGamesSDK.UserDataProviders;
using FunticoGamesSDK.ViewModels;
using UnityEngine;
using Logger = FunticoGamesSDK.Logging.Logger;

namespace FunticoGamesSDK.RoomsProviders
{
    public abstract class RoomsProviderInternal
    {
        protected RoomConfig[] LastRoomsResponse;
        protected readonly IUserDataService UserDataService;
        protected readonly IAuthDataProvider AuthDataProvider;
        protected readonly string PrivateKey;

        public RoomsProviderInternal(string privateKey, IUserDataService userDataService, IAuthDataProvider authDataProvider)
        {
            PrivateKey = privateKey;
            UserDataService = userDataService;
            AuthDataProvider = authDataProvider;
        }

        #region Public

        public async UniTask<PrizePoolDistibutionViewModel> GetPrizePoolDistribution(string roomGuid)
        {
            var roomData = await GetRoomConfig(roomGuid);

            var model = new PrizePoolDistibutionViewModel
            {
                PrizePlaces = new List<PrizePlaceViewModel>(),
                TotalPlayers = roomData.Size ?? 8,
                PlatformFeePercentage = roomData.PrizePool.PlatformFee.Value ?? 0,
                PlatformFee = roomData.Computed.PlatformFee,
                TotalTicoAccumulated = roomData.Computed.Stake
            };

            foreach (var prizeDistribution in roomData.PrizeDistribution)
            {
                // Calculate prize value
                var ticoPrize = PrizeUtils.CalculatePrize(prizeDistribution.Prizes, roomData.Computed.DepositStake);

                var rewardFeeRatio = 0.0f;
                if (roomData.Ticket.CurrencyAmount.HasValue && ticoPrize > 0)
                    rewardFeeRatio = ticoPrize / (float) roomData.Ticket.CurrencyAmount;

                var placeModel = new PrizePlaceViewModel
                {
                    Place = prizeDistribution.Place ?? 0,
                    PlatformExp = prizeDistribution.Xp?.Base ?? 0,
                    PlatformExpKyc = prizeDistribution.Xp?.Kyc ?? 0,
                    RewardFeeRatio = rewardFeeRatio,
                    Prizes = new List<RoomPrizeViewModel>()
                };

                await FillPrizes(prizeDistribution, ticoPrize, placeModel.Prizes);
                model.PrizePlaces.Add(placeModel);
            }

            return model;
        }

        public async UniTask<RoomLeaderboardViewModel> GetLeaderboard(string eventId, string sessionId, string matchId, bool predictPending)
        {
            var getLeaderboardTask = GetRoomLeadersWithNames(eventId, sessionId, matchId);
            var getSessionHistoryTask = GetSessionData(sessionId);
            var getRoomDataTask = GetRoomConfig(eventId);
            var (leaderboard, sessionHistory, roomData) = 
                await UniTask.WhenAll(getLeaderboardTask, getSessionHistoryTask, getRoomDataTask);

            if (leaderboard == null)
                return null;

            return await GetLeaderboard(leaderboard, roomData, predictPending, sessionHistory.GameSessions.VoucherUsed);
        }

        public async UniTask<RoomViewModel> GetRoom(string guid)
        {
            await UserDataService.GetVouchers(false);
            var roomData = await GetRoomConfig(guid);
            return await CreateRoomViewModel(roomData);
        }

        public async UniTask<string> GetRoomSettings(string guid) => await GetRoomSettingsInternal(guid);

        public async UniTask<List<RoomViewModel>> GetRooms(RoomTierEnum? tier = null)
        {
            await UserDataService.GetVouchers(false);
            await UpdateRoomsList(tier);
            var interestedRooms = LastRoomsResponse.Where(room => tier == null || room.Details.Tier == (int) tier);
            var roomViewModelsCreationTasks = interestedRooms.Select(CreateRoomViewModel);
            var roomViewModels = (await UniTask.WhenAll(roomViewModelsCreationTasks)).ToList();

            if (roomViewModels.Count <= 0) return roomViewModels;
            // TODO Can this be done without the extra API call?
            var prepaidRooms = await GetPrePaidRooms();
            foreach (var prepaidRoom in prepaidRooms)
            {
                var prepaidViewModels = roomViewModels.Where(x => x.Guid.Equals(prepaidRoom.id));
                foreach (var prepaidViewModel in prepaidViewModels)
                {
                    prepaidViewModel.IsPrePaid = true;
                }
            }

            return roomViewModels;
        }

        #endregion

        #region Protected/Private

        #region APICalls

        private async UniTask<string> GetRoomSettingsInternal(string guid)
        {
            var url = $"{APIConstants.Get_Room_Settings}?eventId={guid}";
            (bool success, string response) =
                await HTTPClient.Get<string>(url);
            if (!success)
                Logger.LogError("Failed to get room settings");
            return response;
        }

        private async UniTask<RoomConfig[]> GetRoomsFromAPI(RoomTierEnum? tier, int roomType)
        {
            // TODO: Tier implementation, if !tier.HasValue, fetch all

            var link = $"{APIConstants.Get_Rooms}?limit=30&roomType={roomType}";
            (bool success, RoomsResponses response) = await HTTPClient.Get<RoomsResponses>(link);

            if (!success)
                Logger.LogError("Failed to get events");

            return response.Data.ToArray();
        }

        private async UniTask<RoomLeadersResponse> GetRoomLeaders(string roomId, string sessionId, string matchId)
        {
            var link = $"{APIConstants.Get_Room_Leaders}?session_or_match_id={sessionId}&eventId={roomId}";
            if (!string.IsNullOrWhiteSpace(matchId))
                link += $"&matchId={matchId}";
            (bool success, RoomLeadersResponse response) =
                await HTTPClient.Get<RoomLeadersResponse>(link);

            if (!success)
                Logger.LogError("Failed to get room leaders");

            return response;
        }

        public async UniTask<PrePaidEventsResponseData[]> GetPrePaidRooms()
        {
            var link = $"{APIConstants.Get_Pre_Paid}";
            (bool success, GetPrePaidEventsResponse response) =
                await HTTPClient.Get<GetPrePaidEventsResponse>(link);

            if (!success)
                Logger.LogError("Failed to get events");

            return response.data;
        }

        private async UniTask<RoomConfig> GetRoomConfig(string roomId)
        {
            var link = $"{APIConstants.Get_Room}?roomId={roomId}";
            (bool success, RoomConfig response) = await HTTPClient.Get<RoomConfig>(link);
            if (!success)
                Logger.LogError("Failed to get events");
            return response;
        }

        #endregion
    
        protected async UniTask UpdateRoomsList(RoomTierEnum? tier) => LastRoomsResponse = await GetRoomsFromAPI(tier, GetRoomType());

        protected abstract int GetRoomType();

        protected async UniTask<bool> IsRoomPrePaid(string id)
        {
            var prePaidEvents = await GetPrePaidRooms();
            var isPrePaid = prePaidEvents?.Any(x => x.id.Equals(id)) ?? false;
            Logger.Log($"Room {id} {(isPrePaid ? "is pre-paid" : "is not pre-paid")}");
            return isPrePaid;
        }

        private async UniTask<RoomViewModel> CreateRoomViewModel(RoomConfig room)
        {
            Task<Sprite> LoadCoverAsync() => AssetsLoader.LoadSpriteAsync(room.Details.CoverImage, new TextureResizeOptions(Constants.ROOM_COVER_PREFERRED_SIZE)).AsTask();

            var feeIcon = await GetIconForFee(room.Ticket);
            return new RoomViewModel(room, UserDataService.GetCachedVouchers())
            {
                LoadRoomImageTask = LoadCoverAsync,
                FeeIcon = feeIcon
            };
        }

        private async UniTask<RoomLeaderboardViewModel> GetLeaderboard(RoomLeadersResponseWithNames leaderboard,
            RoomConfig roomData, bool predictPending, bool voucherUsed)
        {
            var yourFee = GetYourFee(roomData, voucherUsed);

            var view = new RoomLeaderboardViewModel
            {
                RoomName = roomData.Details.Name,
                RoomTier = roomData.Details.Tier.HasValue ? (RoomTierEnum) roomData.Details.Tier : RoomTierEnum.Challenger,
                State = (LeadersState) leaderboard.state,
                YourFee = yourFee,
                OriginalFee = roomData.Ticket,
                FeeIcon = await GetIconForFee(yourFee)
            };
            LeaderWithName playerPlace = leaderboard.leaders.FirstOrDefault(item =>
                item.id == (ulong) UserDataService.GetCachedUserData().PlatformId);
            if (playerPlace == null)
                return view;

            view.UserPlace = playerPlace.place.ToString() ?? "0";
            var playerPrizes = GetPlayerPrizes(roomData, predictPending, playerPlace);
            var ticoEarnings = PrizeUtils.CalculatePrize(playerPrizes, roomData.Computed.DepositStake);
            view.YourTicoEarnings = ticoEarnings;

            view.LeaderboardItems = await CreateLeaders(view.IsPending, roomData, leaderboard);

            var (rewardsFirstRow, rewardsSecondRow) = await GetRewards(ticoEarnings, playerPrizes);

            view.YourRewardsFirstRow = rewardsFirstRow;
            view.YourRewardsSecondRow = rewardsSecondRow;
            return view;
        }

        private Ticket GetYourFee(RoomConfig roomData, bool usedVoucher)
        {
            if (!usedVoucher)
                return roomData.Ticket;

            var voucher = UserDataService.GetCachedVouchers().FirstOrDefault(voucher =>
                voucher.Tier == roomData.Details.Tier);

            if (voucher == null)
                return roomData.Ticket;

            var voucherTicket = new Ticket()
            {
                CurrencyAmount = 1,
                Type = TicketType.Item,
                ItemId = voucher.ItemId,
                Item = new PlatformItemPrize()
                {
                    Id = voucher.ItemId,
                    Image = voucher.ItemImage,
                    Name = voucher.ItemName
                }
            };

            return voucherTicket;
        }

        private List<Prize> GetPlayerPrizes(RoomConfig roomData, bool predictPending, LeaderWithName playerPlace)
        {
            if (predictPending)
            {
                if (playerPlace.finish_reason != RoomFinishReason.Ok)
                    return new List<Prize>();

                var place = (int) playerPlace.place;
                if (roomData.PrizeDistribution.Count >= place)
                    return roomData.PrizeDistribution[place - 1].Prizes;
            }

            return playerPlace.prizes;
        }

        private async UniTask<List<LeaderboardItemViewModel>> CreateLeaders(bool isPending, RoomConfig roomData,
            RoomLeadersResponseWithNames leaders)
        {
            var createLeadersTasks = leaders.leaders.Select(leader => isPending ? CreateLeader(leader, roomData) : CreateLeader(leader));
            var result = (await UniTask.WhenAll(createLeadersTasks)).ToList();
            if (!isPending)
                return result;

            await FillWithEmptyLeaders(result, roomData);
            return result;
        }

        private async UniTask FillWithEmptyLeaders(List<LeaderboardItemViewModel> result, RoomConfig roomData)
        {
            var roomSize = roomData.Size ?? 0;
            var tasks = new List<UniTask>();
            while (result.Count < roomSize)
            {
                tasks.Add(CreateEmptyLeader(result, roomData));
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask CreateEmptyLeader(List<LeaderboardItemViewModel> result, RoomConfig roomData)
        {
            var item = LeaderboardItemViewModel.CreateEmpty();
            result.Add(item);
            var place = result.Count;
            item.Place = place.ToString();
            item.Prizes = new List<RoomPrizeViewModel>();
            var prizeDistribution =
                roomData.PrizeDistribution.FirstOrDefault(dist => dist.Place != null && dist.Place == place);
            var ticoEarnings = PrizeUtils.CalculatePrize(prizeDistribution?.Prizes, roomData.Computed.DepositStake);
            await FillPrizes(prizeDistribution, ticoEarnings, item.Prizes);
        }

        private async UniTask<Sprite> GetIconForFee(Ticket ticket)
        {
            if (ticket == null) return null;
            return ticket.Type switch
            {
                TicketType.Free => null,
                TicketType.Currency => AssetsLoader.Instance.TicoSprite,
                TicketType.Item when ticket.Item != null => await AssetsLoader.LoadSpriteAsync(ticket.Item.Image, new TextureResizeOptions(Constants.FEE_PREFERRED_SIZE)),
                _ => null
            };
        }

        private async UniTask<(List<RoomPrizeViewModel> firstRow, List<RoomPrizeViewModel> secondRow)> GetRewards(
            long ticoEarnings, List<Prize> playerPrizes)
        {
            var rewardsFirstRow = new List<RoomPrizeViewModel>();
            var rewardsSecondRow = new List<RoomPrizeViewModel>();
            var counter = 0;
            if (ticoEarnings > 0)
            {
                rewardsFirstRow.Add(new RoomPrizeViewModel
                {
                    Amount = ticoEarnings,
                    Sprite = AssetsLoader.Instance.TicoSprite,
                });
                counter++;
            }

            foreach (var prize in playerPrizes.Where(prize => prize.Type is PrizeType.Item or PrizeType.Reward))
            {
                var item = (PrizeWithAmountAndItem) prize;
                if (item is not { Item: not null } || item.Amount < 1)
                {
                    continue;
                }

                var sprite = await AssetsLoader.LoadSpriteAsync(item.Item.Image, new TextureResizeOptions(Constants.PRIZE_PREFERRED_SIZE));
                var prizeViewModel = new RoomPrizeViewModel
                {
                    Amount = item.Amount,
                    Sprite = sprite,
                };

                var collection = counter % 2 == 0 ? rewardsFirstRow : rewardsSecondRow;
                collection.Add(prizeViewModel);

                counter++;
            }

            return (rewardsFirstRow, rewardsSecondRow);
        }

        private async UniTask<LeaderboardItemViewModel> CreateLeader(LeaderWithName leader)
        {
            async UniTask<List<RoomPrizeViewModel>> GetPrizes()
            {
                var getPrizesTasks = leader.prizes.Select(GetPrizeModel);
                return (await UniTask.WhenAll(getPrizesTasks)).ToList();
            }

            return await CreateLeaderInternal(leader, GetPrizes);
        }

        private async UniTask<LeaderboardItemViewModel> CreateLeader(LeaderWithName leader, RoomConfig roomData)
        {
            async UniTask<List<RoomPrizeViewModel>> GetPrizes()
            {
                var prizes = new List<RoomPrizeViewModel>();
                var prizeDistribution =
                    roomData.PrizeDistribution.FirstOrDefault(dist =>
                        dist.Place != null && (ulong) dist.Place == leader.place);
                var ticoEarnings = PrizeUtils.CalculatePrize(prizeDistribution?.Prizes, roomData.Computed.DepositStake);
                await FillPrizes(prizeDistribution, ticoEarnings, prizes);
                return prizes;
            }

            return await CreateLeaderInternal(leader, GetPrizes);
        }

        private async UniTask<LeaderboardItemViewModel> CreateLeaderInternal(LeaderWithName leader,
            Func<UniTask<List<RoomPrizeViewModel>>> getPrizesTask)
        {
            var successFinish = leader.finish_reason == RoomFinishReason.Ok;
            var prizes = successFinish ? await getPrizesTask.Invoke() : null;

            var result = new LeaderboardItemViewModel
            {
                Avatar = leader.avatar,
                Border = leader.border,
                IsMe = IsLocalUser(leader),
            };
            if (successFinish)
            {
                result.Name = leader.name;
                result.Score = leader.score.ToString();
                result.Place = leader.place.ToString();
                result.Prizes = prizes;
            }
            else
            {
                result.Name = $"{leader.name}";
                result.Score = $"DNF";
                result.Place = "-";
            }

            return result;
        }

        private bool IsLocalUser(LeaderWithName leader) =>
            leader.id == (ulong) UserDataService.GetCachedUserData().PlatformId;

        private async UniTask FillPrizes(PrizeDistribution prizeDistribution, long ticoPrize,
            List<RoomPrizeViewModel> prizesContainer)
        {
            if (ticoPrize > 0)
            {
                prizesContainer.Add(new RoomPrizeViewModel
                {
                    Amount = ticoPrize,
                    Sprite = AssetsLoader.Instance.TicoSprite
                });
            }

            if (prizeDistribution == null) return;

            foreach (var prize in prizeDistribution.Prizes)
            {
                if (prize.Type is PrizeType.GppPoolShare or PrizeType.GppPerPlayer or PrizeType.DepositStakePerPlayer
                    or PrizeType.DepositStakePoolShare)
                    continue;

                prizesContainer.Add(await GetPrizeModel(prize));
            }
        }

        private async UniTask<RoomPrizeViewModel> GetPrizeModel(Prize prize)
        {
            var prizeModel = new RoomPrizeViewModel();

            switch (prize.Type)
            {
                case PrizeType.Item:
                    var itemAutomated = (ItemAutomatedPrize) prize;
                    prizeModel.Amount = itemAutomated.Amount;
                    if (itemAutomated.Item != null)
                        prizeModel.Sprite = await AssetsLoader.LoadSpriteAsync(itemAutomated.Item.Image, new TextureResizeOptions(Constants.PRIZE_PREFERRED_SIZE));
                    break;
                case PrizeType.DepositStakePoolShare:
                case PrizeType.DepositStakePerPlayer:
                    var itemDeposit = (APIModels.PrizesResponses.DepositStakeAutomatedPrize) prize;
                    prizeModel.Amount = (int) itemDeposit.Value;
                    prizeModel.Sprite = AssetsLoader.Instance.TicoSprite;
                    break;
                case PrizeType.GppPoolShare:
                case PrizeType.GppPerPlayer:
                    var itemGpp = (GppAutomatedPrize) prize;
                    prizeModel.Amount = (int) itemGpp.Value;
                    prizeModel.Sprite = AssetsLoader.Instance.TicoSprite;
                    break;
                case PrizeType.External:
                case PrizeType.Placeholder:
                    var manualPrize = (ManualPrize) prize;
                    prizeModel.Amount = 0; // TODO: manualPrize.ValueUsd ?
                    prizeModel.Sprite = await AssetsLoader.LoadSpriteAsync(manualPrize.ImageUrl, new TextureResizeOptions(Constants.PRIZE_PREFERRED_SIZE));
                    break;
                case PrizeType.Reward:
                    var rewardPrizeAutomated = (RewardAutomatedPrize) prize;
                    prizeModel.Amount = rewardPrizeAutomated.Amount;
                    if (rewardPrizeAutomated.Item != null)
                        prizeModel.Sprite = await AssetsLoader.LoadSpriteAsync(rewardPrizeAutomated.Item.Image, new TextureResizeOptions(Constants.PRIZE_PREFERRED_SIZE));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return prizeModel;
        }

        private async UniTask<RoomLeadersResponseWithNames> GetRoomLeadersWithNames(string roomId, string sessionId,
            string matchId)
        {
            var leadersResponse = await GetRoomLeaders(roomId, sessionId, matchId);
            if (leadersResponse == null)
                return null;

            var ids = leadersResponse.leaders.Select(x => x.id).ToArray();
            var usersInfoResponse = await GetUsersInfo(ids);
            if (usersInfoResponse == null)
                return null;

            var leadersWithNamesResponse = new RoomLeadersResponseWithNames(leadersResponse);

            foreach (var leader in leadersWithNamesResponse.leaders)
            {
                var leaderInfo = usersInfoResponse.data.FirstOrDefault(x => x.id == leader.id);
                leader.name = leaderInfo.name;
                leader.avatar = leaderInfo.avatar.url;
                leader.border = leaderInfo.border.url;
            }

            return leadersWithNamesResponse;
        }

        private async UniTask<RoomsSessionHistoryResponse> GetSessionData(string sessionId)
        {
            var url = APIConstants.WithQuery(APIConstants.Get_Room_Session_History, $"sessionId={sessionId}", $"withEvent={false}");
            (bool success, RoomsSessionHistoryResponse response) = await HTTPClient.Get<RoomsSessionHistoryResponse>(url);
            return response;
        }

        private async UniTask<UsersInfoResponse> GetUsersInfo(ulong[] ids)
        {
            var sb = new StringBuilder(APIConstants.USERS_INFO).Append("?ids[]=");
            for (var i = 0; i < ids.Length; i++)
            {
                sb.Append(ids[i]);
                if (i < ids.Length - 1)
                    sb.Append("&ids[]=");
            }

            var funticoToken = AuthDataProvider.GetPlatformToken();
            if (funticoToken.IsNullOrWhitespace())
                return null;

            (bool success, UsersInfoResponse response) =
                await HTTPClient.Get<UsersInfoResponse>(sb.ToString(), tokenToUse: funticoToken);

            if (!success)
                Logger.LogError("Failed to get users info");

            return response;
        }

        #endregion
    }
}