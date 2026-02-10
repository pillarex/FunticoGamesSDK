using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels.ServerSessionsModels;
using FunticoGamesSDK.Logging;
using FunticoGamesSDK.SessionsManagement;

namespace FunticoGamesSDK
{
	public partial class FunticoSDK : IServerSessionManager
	{
		private IServerSessionManager _serverSessionManager;

		private void SetupServerSessionManager()
		{
			_serverSessionManager = new ServerSessionManager();
		}

		public UniTask<bool> CreateSession_Server(string serverUrl, string sessionId, List<ServerUserData> playersInfo) =>
			_serverSessionManager.CreateSession_Server(serverUrl, sessionId, playersInfo);

		public UniTask<bool> UserLeaveSession_Server(int userId) =>
			_serverSessionManager.UserLeaveSession_Server(userId);

		public UniTask<bool> CloseCurrentSession_Server()
		{
			Logger.LogWarning("You are not supposed to call CloseCurrentSession manually");
			return UniTask.FromResult(false);
		}

		public List<string> GetCurrentSessionEvents_Server(int userId) =>
			_serverSessionManager.GetCurrentSessionEvents_Server(userId);

		public void RecordEvent_Server(int userId, string eventInfo) =>
			_serverSessionManager.RecordEvent_Server(userId, eventInfo);
	}
}