using System;
using System.Collections.Generic;

namespace FunticoGamesSDK.ViewModels
{
    [Serializable]
    public class HOFViewModel
    {
        public List<HOFDistributionItemViewModel> Items { get; set; } = new();
    }

    [Serializable]
    public class HOFDistributionItemViewModel
    {
        public int Place { get; set; }
        public int? EndPlace { get; set; }
        public string Range { get; set; }
        public List<RoomPrizeViewModel> Prizes { get; set; } = new();
    }
}
