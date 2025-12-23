using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FunticoGamesSDK.APIModels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RoomType
    {
        singleplayer = 0,
        multiplayer = 1,
        external = 2
    }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LeaderboardType
    {
        local = 0,
        global = 1
    }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SortField
    {
        place = 0,
        score = 1
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ValueType
    {
        number = 0,
        percentage = 1
    }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Fn
    {
        min = 0,
        max = 1,
        sum = 2,
        avg = 3,
        median = 4
    }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GameScoringSystemField
    {
        points = 0,
        time = 1,
        distance = 2
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum GamePlacementOrder
    {
        ascending = 0, // Small score wins
        descending = 1 // Big score wins
    }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PrizeDistributionDoneStatus
    {
        idle = PrizeDistributedStatus.idle,
        distributed = PrizeDistributedStatus.distributed,
        canceled = PrizeDistributedStatus.canceled,
        partial = 3
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PrizePoolType
    {
        gpp = 0,
        platform_fee = 1
    }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EditableCurrency
    {
        TICO = 1,
        aTICO = 2
    }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StartWhen
    {
        has_enough_participants = 0b0001,
        after_registration = 0b0010,
        after_start_at = 0b0100
    }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Repeat
    {
        regular = 0,
        once = 1
    }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PrizeDistributedStatus
    {
        idle = 0,
        distributed = 1,
        canceled = 2
    }
}