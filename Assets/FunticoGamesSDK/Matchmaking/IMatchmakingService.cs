using System;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels.Matchmaking;

namespace FunticoGamesSDK.MatchmakingProviders
{
	public interface IMatchmakingService : IDisposable
	{
		event Action<string> OnMatchStatus;
		event Action<MatchResult> OnMatchFound;

		/// <summary>
		/// Joins the matchmaking queue.
		/// </summary>
		/// <param name="region">The region to search for matches.</param>
		/// <param name="size">Number of players required for a match. Valid range: 2-16.</param>
		UniTask JoinQueue(MatchmakingRegion region, int size = 2);
		UniTask LeaveQueue();
	}
}
