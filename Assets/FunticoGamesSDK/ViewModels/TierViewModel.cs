using System;
using System.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using UnityEngine;

namespace FunticoGamesSDK.ViewModels
{
    [Serializable]
    public class TierViewModel
    {
        public string TierName { get; set; }
        public Task<Sprite> TierImage { get; set; }
        public RoomTierEnum Tier { get; set; }
        public int EntryFeeLowerBound { get; set; }
        public int EntryFeeUpperBound { get; set; }
        public bool Hidden { get; set; }
    }
}