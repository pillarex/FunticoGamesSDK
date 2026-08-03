using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.APIModels.PrizesResponses;
using FunticoGamesSDK.AssetsProvider;
using FunticoGamesSDK.NetworkUtils;
using FunticoGamesSDK.TextureResizer;
using FunticoGamesSDK.ViewModels;
using UnityEngine;
using Logger = FunticoGamesSDK.Logging.Logger;

namespace FunticoGamesSDK.HallOfFameProviders
{
    public class HallOfFameProvider : IHallOfFameProvider
    {
        public async UniTask<HallOfFameResponse> GetHallOfFame(
            HallOfFamePeriodMode mode,
            int page,
            int limit,
            HallOfFameGameModeFilter gameMode,
            int? year = null,
            int? month = null)
        {
            var basicURL = mode switch {
                HallOfFamePeriodMode.Global => APIConstants.Get_Hall_Of_Fame_Global,
                HallOfFamePeriodMode.Monthly => APIConstants.Get_Hall_Of_Fame_Monthly,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };

            var url = $"{basicURL}?page={page}&limit={limit}";

            // Період має сенс тільки для Monthly; на Global бекенд його все одно ігнорує
            if (mode == HallOfFamePeriodMode.Monthly)
            {
                // Валідуємо тут, бо на бекенді невалідний місяць/рік падає в 500, а не в 400
                if (month.HasValue && (month.Value < 1 || month.Value > 12))
                {
                    throw new ArgumentOutOfRangeException(nameof(month), month.Value, "Month must be in the 1-12 range.");
                }
                if (year.HasValue && (year.Value < DateTime.MinValue.Year || year.Value > DateTime.MaxValue.Year))
                {
                    throw new ArgumentOutOfRangeException(nameof(year), year.Value, "Year is outside the supported range.");
                }

                if (year.HasValue)
                {
                    url += $"&year={year.Value}";
                }
                if (month.HasValue)
                {
                    url += $"&month={month.Value}";
                }
            }

            if (gameMode != HallOfFameGameModeFilter.All)
            {
                var backendMode = gameMode switch
                {
                    HallOfFameGameModeFilter.SpecialTournament => HallOfFameGameMode.SpecialTournament,
                    HallOfFameGameModeFilter.Rooms => HallOfFameGameMode.Rooms,
                    HallOfFameGameModeFilter.Practice => HallOfFameGameMode.Practice,
                    _ => throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, null)
                };
                url += $"&gameMode={(int)backendMode}";
            }
            
            (bool success, HallOfFameResponse response) = await HTTPClient.Get<HallOfFameResponse>(url);
            if (!success)
            {
                Logger.LogError("Failed to get Hall of Fame leaderboard");
            }
            return response;
        }

        public async UniTask<HOFViewModel> GetHallOfFameDistribution()
        {
            (bool success, HOFDistributionDto dto) = await HTTPClient.Get<HOFDistributionDto>(APIConstants.Get_Hall_Of_Fame_Distribution);
            if (!success || dto == null || dto.Items == null)
            {
                Logger.LogError("Failed to get Hall of Fame distributions");
                return new HOFViewModel { Items = new List<HOFDistributionItemViewModel>() };
            }
            
            var items = new List<HOFDistributionItemViewModel>();
            foreach (var itemDto in dto.Items)
            {
                var prizes = new List<RoomPrizeViewModel>();
                if (itemDto.Prizes != null)
                {
                    foreach (var prize in itemDto.Prizes)
                    {
                        var prizeModel = await GetPrizeModel(prize);
                        if (prizeModel != null) 
                            prizes.Add(prizeModel);
                    }
                }
                
                items.Add(new HOFDistributionItemViewModel
                {
                    Place = itemDto.Place,
                    EndPlace = itemDto.EndPlace,
                    Range = itemDto.Range,
                    Prizes = prizes
                });
            }
            return new HOFViewModel { Items = items };
        }

        private async UniTask<RoomPrizeViewModel> GetPrizeModel(Prize prize)
        {
            if (prize == null) return null;
            var prizeModel = new RoomPrizeViewModel();
            
            switch (prize.Type)
            {
                case PrizeType.Item:
                    var item = (ItemAutomatedPrize)prize;
                    prizeModel.Amount = item.Amount;
                    if (item.Item != null)
                    {
                        prizeModel.Sprite = await AssetsLoader.LoadSpriteAsync(item.Item.Image, new TextureResizeOptions(Constants.PRIZE_PREFERRED_SIZE));
                    }
                    break;
                case PrizeType.DepositStakePoolShare:
                case PrizeType.DepositStakePerPlayer:
                    var itemDeposit = (DepositStakeAutomatedPrize)prize;
                    prizeModel.Amount = (int)itemDeposit.Value;
                    prizeModel.Sprite = AssetsLoader.Instance.TicoSprite;
                    break;
                case PrizeType.GppPoolShare:
                case PrizeType.GppPerPlayer:
                    var itemGpp = (GppAutomatedPrize)prize;
                    prizeModel.Amount = (int)itemGpp.Value;
                    prizeModel.Sprite = AssetsLoader.Instance.TicoSprite;
                    break;
                case PrizeType.External:
                case PrizeType.Placeholder:
                    var manualPrize = (ManualPrize)prize;
                    prizeModel.Amount = 0;
                    prizeModel.Sprite = await AssetsLoader.LoadSpriteAsync(manualPrize.ImageUrl, new TextureResizeOptions(Constants.PRIZE_PREFERRED_SIZE));
                    break;
                case PrizeType.Reward:
                    var rewardPrizeAutomated = (RewardAutomatedPrize)prize;
                    prizeModel.Amount = rewardPrizeAutomated.Amount;
                    if (rewardPrizeAutomated.Item != null)
                    {
                        prizeModel.Sprite = await AssetsLoader.LoadSpriteAsync(rewardPrizeAutomated.Item.Image, new TextureResizeOptions(Constants.PRIZE_PREFERRED_SIZE));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return prizeModel;
        }
    }
}
