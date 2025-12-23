using System.Collections.Generic;

namespace FunticoGamesSDK.ViewModels
{
    public class PrizePoolDistibutionViewModel
    {
        public List<PrizePlaceViewModel> PrizePlaces { get; set; }
        public long TotalTicoAccumulated { get; set; }
        public int TotalPlayers { get; set; }
        public long TicoPerPlayer => TotalTicoAccumulated / TotalPlayers;
        public float PlatformFeePercentage { get; set; }
        public long PlatformFee { get; set; }
        public long TotalTicoDistributedToPlayers => TotalTicoAccumulated - PlatformFee;
    }
}