using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels.ServerSessionsModels;

namespace FunticoGamesSDK.SessionsManagement
{
	public interface IServerSessionManager
	{
		public UniTask<bool> CreateSession_Server(string serverUrl, string sessionId, List<ServerUserData> playersInfo);

		public UniTask<bool> UserLeaveSession_Server(int userId);

		public UniTask<bool> CloseCurrentSession_Server();

		public List<string> GetCurrentSessionEvents_Server(int userId);

		public void RecordEvent_Server(int userId, string eventInfo);
	}
}