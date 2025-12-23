using System.Collections.Generic;

namespace FunticoGamesSDK.APIModels
{
    public enum LeadersState {
        placed = 0,
        distributed = 1,
        to_be_returned = 3,
        returned = 4,
    }

    public class RoomLeadersResponse
    {
        public int state { get; set; }
        public Leader[] leaders { get; set; }
        public Item[] items { get; set; }
        public string match_id { get; set; }
    }

    public class Leader
    {
        public ulong id { get; set; }
        public long score { get; set; }
        public ulong place { get; set; }
        public List<PrizesResponses.Prize> prizes { get; set; }
        public long finish_reason { get; set; }

        public RoomFinishReason FinishReason => (RoomFinishReason) finish_reason;
    }

    public class Item
    {
        public ulong id { get; set; }
        public string image { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public int amount { get; set; } = 0;
    }

    public class PrizesLeaders
    {
        public long? id { get; set; }
        public int? type { get; set; }
        public string? image_url { get; set; }
        public int? value_usd { get; set; }
    }
}