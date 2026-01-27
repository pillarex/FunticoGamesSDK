using System;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels.Matchmaking;
using FunticoGamesSDK.AuthDataProviders;
using FunticoGamesSDK.MatchmakingProviders;

namespace FunticoGamesSDK
{
	public partial class FunticoSDK : IMatchmakingService
	{
		private IMatchmakingService _matchmakingService;

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

		private void SetupMatchmakingService(string publicGameKey, string sessionId, IAuthDataProvider authDataProvider)
		{
			_matchmakingService = new MatchmakingService(authDataProvider, publicGameKey, sessionId);
		}

		public UniTask JoinQueue(string region) => _matchmakingService.JoinQueue(region);

		public UniTask LeaveQueue() => _matchmakingService.LeaveQueue();

		public void Dispose() => _matchmakingService.Dispose();
	}
}
