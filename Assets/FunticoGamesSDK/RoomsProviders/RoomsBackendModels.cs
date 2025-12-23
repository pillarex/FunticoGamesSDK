// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBeProtected.Global

using System.Collections.Generic;
using System.Linq;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.APIModels.PrizesResponses;

namespace FunticoGamesSDK.RoomsProviders
{
    public class ParticipantsLimit
    {
        public uint min { get; set; }
        public uint max { get; set; }
    }

    public class Registration
    {
        public ulong open_at { get; set; } // DateTime
        public long registration_duration { get; set; }
    }

    public class RoomEvent
    {
        public Repeat repeat { get; set; }
        public StartWhen startWhen { get; set; }
        
        // RegularRoomEvent
        public uint? min { get; set; }
        public uint? max { get; set; }
        
        // TimedEvent
        public long duration { get; set; } // assuming timestamp in seconds or milliseconds
        
        // AfterRegistration
        public ulong open_at { get; set; } // DateTime
        public long registration_duration { get; set; }
        
        // AfterStartAt
        public ulong starts_at { get; set; } // DateTime
    }

    public class DepositStakeAutomatedPrize
    {
        public PrizeType type { get; set; }
        public EditableCurrency currency { get; set; }
        
        // DepositStakePerPlayerPrize
        public ulong value { get; set; }
        
        // DepositStakePoolSharePrize
        public byte percentage { get; set; } // 1-100
    }

    public class Game
    {
        public int game_id { get; set; }
    }

    public class Leader
    {
        public ulong id { get; set; }
        public long score { get; set; }
        public ulong place { get; set; }
    }

    public class Score
    {
        public ulong score { get; set; }
    }

    public class LeadersSelector
    {
        public byte from { get; set; }
        public byte to { get; set; }
        public SortField sort_by { get; set; }
        public ValueType from_type { get; set; }
        public ValueType to_type { get; set; }
        public byte claim_limit { get; set; }
        
        // PrizeDistributionDone
        public PrizeDistributionDoneStatus status { get; set; }
        public ulong at { get; set; } // DateTime
    }

    public class DistributionManualPrize
    {
        public ulong id { get; set; }
        public double? value_usd { get; set; }
        
        public ManualPrize Config { get; set; }
    }

    public class Item
    {
        public ulong id { get; set; }
        public string image { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
    }

    public class JoinRoomResponse
    {
        public string id { get; set; }
        public string game_session_id_or_match_id { get; set; }
    }

    public class EnterRoomResponse
    {
        public string id { get; set; }
    }

    public class RoomLeadersResponseWithNames : RoomLeadersResponse
    {
        public new LeaderWithName[] leaders { get; set; }

        public RoomLeadersResponseWithNames(RoomLeadersResponse roomLeadersResponse)
        {
            state = roomLeadersResponse.state;
            items = roomLeadersResponse.items;
            match_id = roomLeadersResponse.match_id;

            leaders = roomLeadersResponse.leaders.Select(x => new LeaderWithName
            {
                id = x.id,
                score = x.score,
                place = x.place,
                prizes = x.prizes,
                finish_reason = x.FinishReason,
                name = null
            }).ToArray();
        }
    }

    public class LeaderWithName : Leader
    {
        public string name { get; set; } // assigned at a separate request
        public string avatar { get; set; } // assigned at a separate request
        public string border { get; set; } // assigned at a separate request
        public List<Prize> prizes { get; set; } // assigned at a separate request
        public RoomFinishReason finish_reason { get; set; }
    }
    
    public class UsersInfoResponse
    {
        public DataItem[] data { get; set; }
    }

    public class DataItem
    {
        public ulong id { get; set; }
        public string name { get; set; }
        public Avatar avatar { get; set; }
        public Border border { get; set; } 
    }

    public class Avatar
    {
        public string url { get; set; }
        public string type { get; set; }
    }

    public class Border
    {
        public string url { get; set; }
        public string type { get; set; }
    }
}