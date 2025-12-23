using System.Collections.Generic;
using Newtonsoft.Json;

namespace FunticoGamesSDK.APIModels.UserData
{
    public class VoucherStaticData
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public int Id { get; set; }
        [JsonProperty("tier")]
        public RoomTierEnum RoomTierEnum { get; set; }
    }

    public class VoucherStaticDataResponse
    {
        public List<VoucherStaticData> Data { get; set; }
    }

    public class VoucherData
    {
        [JsonProperty("item_id")]
        public int ItemId;

        [JsonProperty("item_name")]
        public string ItemName;

        [JsonProperty("item_image")]
        public string ItemImage;

        [JsonProperty("tier")]
        public int Tier;

        [JsonProperty("play_count")]
        public int PlayCount; // for slider 1/N

        [JsonProperty("plays_required_to_activate")]
        public int PlaysRequiredToActivate; // How many time player need to play to receive 1 voucher

        [JsonProperty("count")]
        public int Count; //amount of vouchers
    }

    public class VoucherResponse
    {
        public List<VoucherData> Data { get; set; }
    }
}