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

		public void RecordEvent_Client(string eventInfo);
	}
}