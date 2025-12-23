using System.Collections.Generic;
using FunticoGamesSDK.APIModels;
using UnityEngine;

namespace FunticoGamesSDK.ViewModels
{
    public class RoomLeaderboardViewModel
    {
        public LeadersState State { get; set; }
        public bool ClosedUnsuccessfully => State is LeadersState.returned or LeadersState.to_be_returned;
        public bool IsPending => State == LeadersState.placed;
        public RoomTierEnum RoomTier { get; set; }
        public string RoomName { get; set; }
        public Ticket YourFee { get; set; }
        public Ticket OriginalFee { get; set; }
        public Sprite FeeIcon { get; set; }
        public bool IsFree => YourFee.IsFree();
        public long YourTicoEarnings { get; set; }
        public float Multiplier => YourFee.Type == TicketType.Currency && YourFee.CurrencyAmount.HasValue && YourFee.CurrencyAmount.Value != 0 
            ? (float) YourTicoEarnings / YourFee.CurrencyAmount.Value 
            : 0f;
        public string UserPlace { get; set; }
        public List<RoomPrizeViewModel> YourRewardsFirstRow { get; set; }
        public List<RoomPrizeViewModel> YourRewardsSecondRow { get; set; }
        public List<LeaderboardItemViewModel> LeaderboardItems { get; set; }
    }
}