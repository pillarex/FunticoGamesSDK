using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace FunticoGamesSDK.SessionsManagement
{
	public interface IServerSessionManager
	{
		public UniTask<bool> UserHasUnfinishedSession_Server(int userId);

		public UniTask<string> ReconnectToUnfishedSession_Server(int userId);

		public UniTask<bool> CreateSession_Server(int userId, string json);

		public UniTask<bool> UpdateSession_Server(int userId, string json);

		public void CloseCurrentSession_Server(int userId);

		public List<string> GetCurrentSessionEvents_Server(int userId);

		public void RecordEvent_Server(int userId, string eventInfo);
	}
}