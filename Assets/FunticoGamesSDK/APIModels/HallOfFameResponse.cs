using System.Collections.Generic;
using FunticoGamesSDK.APIModels.PrizesResponses;
using Newtonsoft.Json;

namespace FunticoGamesSDK.APIModels
{
    public class HallOfFameResponse
    {
        [JsonProperty("Leaderboard")]
        public List<HallOfFamePlayerEntry> Leaderboard { get; set; }

        [JsonProperty("CurrentPlayer")]
        public HallOfFamePlayerEntry CurrentPlayer { get; set; }

        [JsonProperty("TotalPlayers")]
        public int TotalPlayers { get; set; }

        [JsonProperty("Page")]
        public int Page { get; set; }

        [JsonProperty("Limit")]
        public int Limit { get; set; }
    }

    public class HallOfFamePlayerEntry
    {
        [JsonProperty("UserId")]
        public int UserId { get; set; }

        [JsonProperty("FunticoId")]
        public int FunticoId { get; set; }

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("AvatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonProperty("AvatarType")]
        public string AvatarType { get; set; }

        [JsonProperty("AvatarBorder")]
        public string AvatarBorder { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }

        [JsonProperty("Rank")]
        public int Rank { get; set; }

        [JsonProperty("TotalPoints")]
        public long TotalPoints { get; set; }

        [JsonProperty("GamesPlayed")]
        public int GamesPlayed { get; set; }

        [JsonProperty("PlayedAt")]
        public string PlayedAt { get; set; }
    }

    public class HOFDistributionDto
    {
        [JsonProperty("Items")]
        public List<HOFDistributionItemDto> Items { get; set; }
    }

    public class HOFDistributionItemDto
    {
        [JsonProperty("Place")]
        public int Place { get; set; }

        [JsonProperty("EndPlace")]
        public int? EndPlace { get; set; }

        [JsonProperty("Range")]
        public string Range { get; set; }

        [JsonProperty("Prizes")]
        public List<Prize> Prizes { get; set; }
    }
}
