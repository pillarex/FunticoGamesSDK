using System;
using System.Collections.Generic;
using FunticoGamesSDK.APIModels.ServerSessionsModels;
using FunticoGamesSDK.Matchmaking.Models;

namespace FunticoGamesSDK.APIModels.Matchmaking
{
	public class MatchResult
	{
		public Guid MatchId { get; set; }
		public List<OpponentData> Opponents { get; set; } = new();
		public string ServerUrl { get; set; }
		public string JoinKey { get; set; }
	}
}