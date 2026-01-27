using System;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels.Matchmaking;

namespace FunticoGamesSDK.MatchmakingProviders
{
	public interface IMatchmakingService : IDisposable
	{
		event Action<string> OnMatchStatus;
		event Action<MatchResult> OnMatchFound;
		
		UniTask JoinQueue(string region);
		UniTask LeaveQueue();
	}
}
