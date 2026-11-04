using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;

namespace FunticoGamesSDK.SessionsManagement
{
	public interface IClientSessionManager
	{
		public UniTask<UnfinishedSessionsResponse> UserHasUnfinishedSession_Client();

		public UniTask<string> ReconnectToUnfinishedSession_Client(string id);

		public UniTask<SavedSessionResponse> CreateSession_Client(string json, GameTypeEnum gameType, string eventId, string saveSessionId);

		public UniTask<bool> UpdateSession_Client(string json);

		public void CloseCurrentSession_Client();

		public List<string> GetCurrentSessionEvents_Client();

		/// <summary>Clears only the recorded event log; the session itself (if any) stays open.</summary>
		public void ClearSessionEvents_Client();

		public void RecordEvent_Client(string eventInfo);

		/// <summary>
		/// Counts an item the player collected during the current run, such as a chest. The counts are
		/// sent with the result by FinishRoomSession_Client and ResultTournament_Client, also when the
		/// run was lost, and are saved with every UpdateSession_Client checkpoint.
		/// </summary>
		/// <param name="key">The reward key agreed for your game, for example "chest_1".</param>
		/// <param name="count">How many to add.</param>
		public void RecordCollected_Client(string key, int count = 1);

		/// <summary>Returns a copy of what the current run has collected so far, by reward key.</summary>
		public Dictionary<string, int> GetCurrentSessionCollected_Client();

		/// <summary>Clears only the collected counts; the session itself (if any) stays open.</summary>
		public void ClearSessionCollected_Client();
	}
}