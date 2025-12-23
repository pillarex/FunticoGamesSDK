using System.Collections.Generic;

namespace FunticoGamesSDK.ViewModels
{
    public class LeaderboardItemViewModel
    {
        public string Place { get; set; }
        public string Avatar { get; set; }
        public string Border { get; set; }
        public string Name { get; set; }
        public string Score { get; set; }
        public bool IsMe { get; set; }
        public bool IsEmpty { get; set; }
        public List<RoomPrizeViewModel> Prizes { get; set; }

        public static LeaderboardItemViewModel CreateEmpty()
        {
            var result = new LeaderboardItemViewModel
            {
                Avatar = null,
                Border = null,
                Name = "Player Needed",
                Score = "<voffset=-0.2em><size=28>TBD",
                IsEmpty = true
            };

            return result;
        }
    }
}