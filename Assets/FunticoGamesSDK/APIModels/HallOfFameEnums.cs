using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FunticoGamesSDK.APIModels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HallOfFameGameMode
    {
        SpecialTournament = 0,
        Rooms = 1,
        Practice = 2,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum HallOfFamePeriodMode
    {
        Global = 0,
        Monthly = 1,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum HallOfFameGameModeFilter
    {
        All = 0,
        SpecialTournament = 1,
        Rooms = 2,
        Practice = 3,
    }
}
