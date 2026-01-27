using System;
using System.Collections.Generic;

namespace FunticoGamesSDK.APIModels.Matchmaking
{
	public class MatchResult
	{
		public Guid MatchId { get; set; }
		public List<OpponentData> Opponents { get; set; } = new();
		public string ServerUrl { get; set; }
	}
}