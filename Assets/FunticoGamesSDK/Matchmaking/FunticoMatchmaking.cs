using System;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels.Matchmaking;
using FunticoGamesSDK.MatchmakingProviders;

namespace FunticoGamesSDK
{
	/// <summary>
	/// Matchmaking service for FunticoSDK.
	/// Requires USE_FUNTICO_MATCHMAKING scripting define symbol.
	/// </summary>
	public class FunticoMatchmaking : IMatchmakingService
	{
		private static FunticoMatchmaking _instance;
		public static FunticoMatchmaking Instance => _instance ??= new FunticoMatchmaking();

		private readonly IMatchmakingService _matchmakingService;

		public event Action<string> OnMatchStatus
		{
			add => _matchmakingService.OnMatchStatus += value;
			remove => _matchmakingService.OnMatchStatus -= value;
		}

		public event Action<MatchResult> OnMatchFound
		{
			add => _matchmakingService.OnMatchFound += value;
			remove => _matchmakingService.OnMatchFound -= value;
		}

		private FunticoMatchmaking()
		{
			var sdk = FunticoSDK.Instance;
			_matchmakingService = new MatchmakingService(sdk, sdk.PublicGameKey, sdk.SessionId);
		}

		public UniTask JoinQueue(MatchmakingRegion region, int size) => _matchmakingService.JoinQueue(region, size);

		public UniTask LeaveQueue() => _matchmakingService.LeaveQueue();

		public void Dispose() => _matchmakingService.Dispose();
	}
}
