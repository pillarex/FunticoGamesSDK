using System;
using Newtonsoft.Json;

namespace FunticoGamesSDK.APIModels
{
    public class JoinRoomResponse
    {
        public string id { get; set; }
        public string game_session_id_or_match_id { get; set; }
        public LobbyQuote? lobby_quote { get; set; } = null;
    }

    public class LobbyQuote
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("exp")]
        public DateTime ExpDate { get; set; }
    }
}