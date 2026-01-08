using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.Logging;
using FunticoGamesSDK.SessionsManagement;
using FunticoGamesSDK.UserDataProviders;

namespace FunticoGamesSDK
{
	public partial class FunticoSDK : IClientSessionManager
	{
		private IClientSessionManager _clientSessionManager;

		private void SetupClientSessionManager(string privateGameKey, IUserDataService userDataService)
		{
			_clientSessionManager = new ClientSessionManager(privateGameKey, userDataService);
		}

		public UniTask<bool> UserHasUnfinishedSession_Client() => _clientSessionManager.UserHasUnfinishedSession_Client();

		public UniTask<string> ReconnectToUnfishedSession_Client() => _clientSessionManager.ReconnectToUnfishedSession_Client();

		public UniTask<bool> CreateSession_Client(string json) => _clientSessionManager.CreateSession_Client(json);

		public UniTask<bool> UpdateSession_Client(string json) => _clientSessionManager.UpdateSession_Client(json);

		public void CloseCurrentSession_Client() => Logger.LogWarning("You are not supposed to call CloseCurrentSession manually");

		public List<string> GetCurrentSessionEvents_Client() => _clientSessionManager.GetCurrentSessionEvents_Client();

		public void RecordEvent_Client(string eventInfo) => _clientSessionManager.RecordEvent_Client(eventInfo);
	}
}