using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace FunticoGamesSDK.SessionsManagement
{
	public interface IClientSessionManager
	{
		public UniTask<bool> UserHasUnfinishedSession_Client();

		public UniTask<string> ReconnectToUnfishedSession_Client();

		public UniTask<bool> CreateSession_Client(string json);

		public UniTask<bool> UpdateSession_Client(string json);

		public void CloseCurrentSession_Client();

		public List<string> GetCurrentSessionEvents_Client();

		public void RecordEvent_Client(string eventInfo);
	}
}