using System.Collections.Generic;
using FunticoGamesSDK.APIModels.Converters;
using Newtonsoft.Json;

namespace FunticoGamesSDK.APIModels.PrizesResponses
{
    [JsonConverter(typeof(PrizeConverter))]
    public abstract class Prize
    {
        [JsonProperty("type")]
        public PrizeType Type { get; set; }
    }

    public class ManualPrize : Prize
    {
        [JsonProperty("id")]
        public uint Id { get; set; }
        
        [JsonProperty("value_usd")]
        public decimal? ValueUsd { get; set; }
        
        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("chain")]
        public string Chain { get; set; }
        
        [JsonProperty("contract_address")]
        public string ContractAddress { get; set; }
        
        [JsonProperty("explorer_url")]
        public string ExplorerUrl { get; set; }
        
        [JsonProperty("wallet_address")]
        public string WalletAddress { get; set; }
        
        [JsonProperty("token_id")]
        public long? TokenId { get; set; }
    }

    public abstract class PrizeWithAmountAndItem : Prize
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }
        [JsonProperty("item")]
        public PlatformItemPrize? Item { get; set; }
    }

    public class ItemAutomatedPrize : PrizeWithAmountAndItem
    {
        [JsonProperty("item_id")]
        public ulong ItemId { get; set; }
        
        public ItemAutomatedPrize()
        {
            Type = PrizeType.Item;
        }
    }

    public class RewardAutomatedPrize : PrizeWithAmountAndItem
    {
        [JsonProperty("prize_id")]
        public long PrizeId { get; set; }
    }

    public class DepositStakeAutomatedPrize : Prize
    {
        [JsonProperty("currency")]
        public byte Currency { get; set; }
        
        [JsonProperty("value")]
        public ulong Value { get; set; }
        
        [JsonProperty("percentage")]
        public byte? Percentage { get; set; }
    }

    public class GppAutomatedPrize : Prize
    {
        [JsonProperty("currency")]
        public byte Currency { get; set; }
        
        [JsonProperty("value")]
        public ulong Value { get; set; }
        
        [JsonProperty("percentage")]
        public byte? Percentage { get; set; }
    }

    public static class PrizeUtils
    {
        public static long CalculatePrize(List<Prize> prizes, long depositStake)
        {
            if (prizes == null)
                return 0;

            long prizeValue = 0;
            foreach (var prize in prizes)
            {
                switch (prize.Type)
                {
                    case PrizeType.GppPoolShare:
                        var gppSharePrize = (GppAutomatedPrize)prize;
                        if (gppSharePrize.Percentage != null)
                            prizeValue += (long)((decimal)gppSharePrize.Percentage * (decimal)depositStake / 100m);
                        break;
                    case PrizeType.DepositStakePoolShare:
                        var depositSharePrize = (DepositStakeAutomatedPrize)prize;
                        if (depositSharePrize.Percentage != null)
                            prizeValue += (long)((decimal)depositSharePrize.Percentage * (decimal)depositStake / 100m);
                        break;
                    case PrizeType.DepositStakePerPlayer:
                        var depositPlayerPrize = (DepositStakeAutomatedPrize)prize;
                        prizeValue += (long)depositPlayerPrize.Value;
                        break;
                    case PrizeType.GppPerPlayer:
                        var gppPlayerPrize = (GppAutomatedPrize)prize;
                        prizeValue += (long)gppPlayerPrize.Value;
                        break;
                }
            }

            return prizeValue;
        }
    }
}