using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

		public UniTask<bool> UserHasUnfinishedSession_Server(int userId) => _serverSessionManager.UserHasUnfinishedSession_Server(userId);

		public UniTask<string> ReconnectToUnfishedSession_Server(int userId) => _serverSessionManager.ReconnectToUnfishedSession_Server(userId);

		public UniTask<bool> CreateSession_Server(int userId, string json) => _serverSessionManager.CreateSession_Server(userId, json);

		public UniTask<bool> UpdateSession_Server(int userId, string json) => _serverSessionManager.UpdateSession_Server(userId, json);

		public void CloseCurrentSession_Server(int userId) => Logger.LogWarning("You are not supposed to call CloseCurrentSession manually");

		public List<string> GetCurrentSessionEvents_Server(int userId) => _serverSessionManager.GetCurrentSessionEvents_Server(userId);

		public void RecordEvent_Server(int userId, string eventInfo) => _serverSessionManager.RecordEvent_Server(userId, eventInfo);
	}
}