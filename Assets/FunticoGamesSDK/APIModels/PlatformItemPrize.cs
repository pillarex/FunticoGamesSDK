using Newtonsoft.Json;

namespace FunticoGamesSDK.APIModels
{
    public class PlatformItemPrize
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}