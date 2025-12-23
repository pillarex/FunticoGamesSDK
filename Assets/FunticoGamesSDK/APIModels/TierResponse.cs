using System.Collections.Generic;
using Newtonsoft.Json;

namespace FunticoGamesSDK.APIModels
{
    public class TierData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("tier")]
        public int Tier { get; set; }

        [JsonProperty("lowerBoundEntryFee")]
        public int LowerBoundEntryFee { get; set; }

        [JsonProperty("upperBoundEntryFee")]
        public int UpperBoundEntryFee { get; set; }
        
        [JsonProperty("hidden")]
        public bool Hidden { get; set; }
    }

    public class TierResponse
    {
        [JsonProperty("data")]
        public List<TierData> Data { get; set; }
    }
}