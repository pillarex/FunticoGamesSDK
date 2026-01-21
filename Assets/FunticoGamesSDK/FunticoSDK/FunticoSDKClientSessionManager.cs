using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.Logging;
using FunticoGamesSDK.SessionsManagement;
using FunticoGamesSDK.UserDataProviders;

namespace FunticoGamesSDK
{
	public partial class FunticoSDK : IClientSessionManager
	{
		private IClientSessionManager _clientSessionManager;

		private void SetupClientSessionManager(IUserDataService userDataService)
		{
			_clientSessionManager = new ClientSessionManager(userDataService);
		}

		public UniTask<UnfinishedSessionsResponse> UserHasUnfinishedSession_Client() =>
			_clientSessionManager.UserHasUnfinishedSession_Client();

		public UniTask<string> ReconnectToUnfinishedSession_Client(string id) =>
			_clientSessionManager.ReconnectToUnfinishedSession_Client(id);

		public UniTask<SavedSessionResponse> CreateSession_Client(string json, GameTypeEnum gameType, string eventId, string saveSessionId) =>
			_clientSessionManager.CreateSession_Client(json, gameType, eventId, saveSessionId);

		public UniTask<bool> UpdateSession_Client(string json) =>
			_clientSessionManager.UpdateSession_Client(json);

		public void CloseCurrentSession_Client() =>
			Logger.LogWarning("You are not supposed to call CloseCurrentSession manually");

		public List<string> GetCurrentSessionEvents_Client() => _clientSessionManager.GetCurrentSessionEvents_Client();

		public void RecordEvent_Client(string eventInfo) => _clientSessionManager.RecordEvent_Client(eventInfo);
	}
}