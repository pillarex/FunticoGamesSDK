using System.Collections.Generic;

namespace FunticoGamesSDK.APIModels.Matchmaking
{
	public class ServerSetupData
	{
		public string RoomSettings { get; set; }
		public string MatchId { get; set; }
		public Dictionary<string, OpponentData> Players { get; set; }
	}
}