using System;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels.Matchmaking;
using FunticoGamesSDK.MatchmakingProviders;
using System.Collections.Generic;
using FunticoGamesSDK.Logging;
using FunticoGamesSDK.Matchmaking.Models;
using Newtonsoft.Json;

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

		public event Action<AcceptMatchServer> OnAcceptMatch
		{
			add => _matchmakingService.OnAcceptMatch += value;
			remove => _matchmakingService.OnAcceptMatch -= value;
		}

		public event Action<string> OnMatchCancelled
		{
			add => _matchmakingService.OnMatchCancelled += value;
			remove => _matchmakingService.OnMatchCancelled -= value;
		}

		public event Action<string> OnMatchError
		{
			add => _matchmakingService.OnMatchError += value;
			remove => _matchmakingService.OnMatchError -= value;
		}

		public event Action OnConnectionStarted
		{
			add => _matchmakingService.OnConnectionStarted += value;
			remove => _matchmakingService.OnConnectionStarted -= value;
		}

		public event Action OnConnectionClosed
		{
			add => _matchmakingService.OnConnectionClosed += value;
			remove => _matchmakingService.OnConnectionClosed -= value;
		}

		private FunticoMatchmaking()
		{
			var sdk = FunticoSDK.Instance;
			_matchmakingService = new MatchmakingService(sdk, sdk.PublicGameKey, sdk.SessionId);
		}

		public UniTask JoinQueue(string eventId, MatchmakingRegion region, int size) => _matchmakingService.JoinQueue(eventId, region, size);

		public void AcceptMatch() => _matchmakingService.AcceptMatch();

		public void DeclineMatch() => _matchmakingService.DeclineMatch();

		public void LeaveQueue() => _matchmakingService.LeaveQueue();
			
		public ServerSetupData GetServerSetupData()
		{
#if !SERVER && !UNITY_SERVER
			Logger.LogError("GetServerSetupData should be called on server");
			return null;
#endif
			var userKeys = Environment.GetEnvironmentVariable("User_Keys");
			var matchId = Environment.GetEnvironmentVariable("MatchId");
			if (userKeys == null)
			{
				return new ServerSetupData()
				{
					MatchId = matchId
				};
			}

			var userKeysParsed = JsonConvert.DeserializeObject<Dictionary<string, OpponentData>>(userKeys);
			return new ServerSetupData()
			{
				MatchId = matchId,
				Players = userKeysParsed
			};
		}

		public void Dispose() => _matchmakingService.Dispose();
	}
}
