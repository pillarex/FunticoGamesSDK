using System.Collections.Generic;
using FunticoGamesSDK.APIModels.ServerSessionsModels;
using FunticoGamesSDK.Matchmaking.Models;

namespace FunticoGamesSDK.APIModels.Matchmaking
{
	public class ServerSetupData
	{
		public string RoomSettings { get; set; }
		public string MatchId { get; set; }
		public Dictionary<string, OpponentData> Players { get; set; }
	}
}