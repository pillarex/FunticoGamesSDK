using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.GhostProviders;

namespace FunticoGamesSDK
{
	public partial class FunticoSDK : IGhostProvider
	{
		private IGhostProvider _ghostProvider;

		private void SetupGhostProvider()
		{
			_ghostProvider = new GhostProvider(_signer);
		}

		public UniTask<GhostPoolsResponse> GetGhostPools() => _ghostProvider.GetGhostPools();

		public UniTask<GhostRunResponse> GetGhostRun(string pool) => _ghostProvider.GetGhostRun(pool);

		public UniTask<GhostRunResponse> StartGhostRun(string pool) => _ghostProvider.StartGhostRun(pool);

		public UniTask<GhostRunResponse> AbandonGhostRun(string pool) => _ghostProvider.AbandonGhostRun(pool);

		public UniTask<GhostFindResponse> FindGhostOpponent(string pool, Dictionary<string, int> attributes,
			string mySnapshotJson = null, IEnumerable<Guid> exclude = null, bool allowBots = true) =>
			_ghostProvider.FindGhostOpponent(pool, attributes, mySnapshotJson, exclude, allowBots);

		public UniTask<GhostUploadResponse> UploadGhostSnapshot(string pool, Dictionary<string, int> attributes,
			string snapshotJson) =>
			_ghostProvider.UploadGhostSnapshot(pool, attributes, snapshotJson);

		public UniTask<GhostReportResponse> ReportGhostResult(Guid matchId, GhostOutcome outcome, string fightHash = null) =>
			_ghostProvider.ReportGhostResult(matchId, outcome, fightHash);

		public UniTask<GhostReportSummaryResponse> GetGhostReportSummary(string pool) =>
			_ghostProvider.GetGhostReportSummary(pool);

		public UniTask<GhostHistoryResponse> GetGhostHistory(string pool, int page = 1, int limit = 20) =>
			_ghostProvider.GetGhostHistory(pool, page, limit);

		public UniTask<GhostLeaderboardResponse> GetGhostLeaderboard(string pool, int page = 1, int limit = 20, string partition = null) =>
			_ghostProvider.GetGhostLeaderboard(pool, page, limit, partition);

		public GhostErrorResponse GetLastGhostError() => _ghostProvider.GetLastGhostError();
	}
}
