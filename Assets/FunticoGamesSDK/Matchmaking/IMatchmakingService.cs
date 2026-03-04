using System;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels.Matchmaking;
using FunticoGamesSDK.Matchmaking.Models;

namespace FunticoGamesSDK.MatchmakingProviders
{
	public interface IMatchmakingService : IDisposable
	{
		event Action<string> OnMatchStatus;
		event Action<MatchResult> OnMatchFound;
		event Action<AcceptMatchServer> OnAcceptMatch;
		event Action<string> OnMatchCancelled;
		event Action<string> OnMatchError;
		event Action OnConnectionStarted;
		event Action OnConnectionClosed;

		/// <summary>
		/// Joins the matchmaking queue.
		/// </summary>
		/// <param name="eventId">Id of event you're trying to join.</param>
		/// <param name="region">The region to search for matches.</param>
		/// <param name="size">Number of players required for a match. Valid range: 2-16.</param>
		UniTask JoinQueue(string eventId, MatchmakingRegion region, int size = 2);
		void AcceptMatch();
		void DeclineMatch();
		void LeaveQueue();
	}
}
