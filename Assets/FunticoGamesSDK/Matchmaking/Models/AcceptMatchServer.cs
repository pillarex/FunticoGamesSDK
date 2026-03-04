using System;

namespace FunticoGamesSDK.APIModels.Matchmaking
{
	public class AcceptMatchServer
	{
		public Guid MatchId { get; set; }
		public int TimeoutSeconds { get; set; }
	}
}