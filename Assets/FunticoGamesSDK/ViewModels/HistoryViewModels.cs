namespace FunticoGamesSDK.ViewModels
{
    public class HistoryViewModel
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public bool IsEnded { get; set; }
        public string Score { get; set; }
        public string Place { get; set; }
        public string TicoWon { get; set; }
    }

    public class RoomsHistoryViewModel : HistoryViewModel
    {
        public string EventId { get; set; }
        public string SessionId { get; set; }
        public string MatchId { get; set; }
    }

// public class PvPHistoryViewModel : HistoryViewModel
// {
//     public string EventId { get; set; }
//     public string SessionId { get; set; }
//     public string MatchId { get; set; }
// }
//
// public class TournamentsHistoryViewModel : HistoryViewModel
// {
//     public string Entries { get; set; }
//     public string PlatformGuid { get; set; }
//
//     public TournamentsHistoryViewModel() { }
//
//     public TournamentsHistoryViewModel(TournamentWithPlayersResponse data) : this()
//     {
//         IsEnded = data.StartDate + TimeSpan.FromSeconds(data.DurationInSeconds) < DateTime.UtcNow;
//         Name = data.Name;
//         Entries = data.CurrentPlayer.TimesPlayed.ToString();
//         Place = (!IsEnded) ? ("TBA") : data.CurrentPlayer.Place.ToString();
//         Score = data.ScoreType == "points"
//             ? data.CurrentPlayer.Points.ToString()
//             : TimeSpan.FromMilliseconds(data.CurrentPlayer.BestTime).ToString(@"mm\:ss\:ff");
//         PlatformGuid = data.Plaftorfm_Guid;
//         TicoWon = (!IsEnded) ? "?" : "0";
//     }
// }
}