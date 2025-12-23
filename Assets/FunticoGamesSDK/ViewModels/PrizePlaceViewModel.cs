using System.Collections.Generic;

namespace FunticoGamesSDK.ViewModels
{
    public class PrizePlaceViewModel
    {
        public int Place { get; set; }
        public int PlatformExp { get; set; }
        public int PlatformExpKyc { get; set; }
        public float RewardFeeRatio { get; set; }
        public List<RoomPrizeViewModel> Prizes { get; set; }
    }
}