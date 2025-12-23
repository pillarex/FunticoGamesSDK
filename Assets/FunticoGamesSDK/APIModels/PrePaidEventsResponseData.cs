using FunticoGamesSDK.APIModels.Converters;
using Newtonsoft.Json;

namespace FunticoGamesSDK.APIModels
{
    public class GetPrePaidEventsResponse
    {
        public PrePaidEventsResponseData[] data { get; set; }
    }
    
    public class PrePaidEventsResponseData
    {
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("attempts_left")]
        public int AttemptsLeft { get; set; }
        [JsonProperty("participation_id")]
        public string ParticipationId { get; set; }
        [JsonProperty("game_session_id")]
        public string GameSessionId { get; set; }
        [JsonProperty("community_id")]
        public string CommunityId { get; set; }
        [JsonProperty("voucher_used")]
        [JsonConverter(typeof(FlexibleBoolConverter))]
        public bool VoucherUsed { get; set; }
    }
}