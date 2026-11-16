using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.CommunityGoalProviders;

namespace FunticoGamesSDK
{
	public partial class FunticoSDK : ICommunityGoalProvider
	{
		private ICommunityGoalProvider _communityGoalProvider;

		private void SetupCommunityGoalProvider()
		{
			_communityGoalProvider = new CommunityGoalProvider(_signer);
		}

		public UniTask<CommunityGoalResponse> GetCommunityGoal(string chainKey) =>
			_communityGoalProvider.GetCommunityGoal(chainKey);

		public UniTask<CommunityGoalResponse> GetCommunityGoalById(Guid goalId) =>
			_communityGoalProvider.GetCommunityGoalById(goalId);

		public UniTask<CommunityGoalLiveResponse> GetCommunityGoalLive(Guid goalId) =>
			_communityGoalProvider.GetCommunityGoalLive(goalId);

		public UniTask<StartGoalAttemptResponse> StartGoalAttempt(Guid goalId) =>
			_communityGoalProvider.StartGoalAttempt(goalId);

		public UniTask<CompleteGoalAttemptResponse> CompleteGoalAttempt(GoalAttemptDto attempt, long contribution,
			double? clientDurationSeconds = null, IDictionary<string, double> metrics = null, string fightHash = null) =>
			_communityGoalProvider.CompleteGoalAttempt(attempt, contribution, clientDurationSeconds, metrics, fightHash);

		public UniTask<CommunityGoalLeaderboardResponse> GetCommunityGoalLeaderboard(Guid goalId, int page = 1, int limit = 20) =>
			_communityGoalProvider.GetCommunityGoalLeaderboard(goalId, page, limit);

		public UniTask<CommunityGoalHistoryResponse> GetCommunityGoalHistory(string chainKey, int page = 1, int limit = 10) =>
			_communityGoalProvider.GetCommunityGoalHistory(chainKey, page, limit);

		public CommunityGoalWatch WatchCommunityGoal(Guid goalId, Action<CommunityGoalLiveResponse> onChange,
			Action<CommunityGoalErrorResponse> onError = null, int minIntervalSeconds = 2, bool pollInBackground = false) =>
			_communityGoalProvider.WatchCommunityGoal(goalId, onChange, onError, minIntervalSeconds, pollInBackground);

		public CommunityGoalErrorResponse GetLastCommunityGoalError() => _communityGoalProvider.GetLastCommunityGoalError();
	}
}
