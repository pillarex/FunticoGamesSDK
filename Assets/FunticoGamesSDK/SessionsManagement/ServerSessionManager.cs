using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels.ServerSessionsModels;
using FunticoGamesSDK.Logging;
using FunticoGamesSDK.NetworkUtils;
using Newtonsoft.Json;

namespace FunticoGamesSDK.SessionsManagement
{
	public class ServerSessionManager : IServerSessionManager
	{
		private readonly Dictionary<int, List<string>> _sessionLogs = new Dictionary<int, List<string>>();
		private string _currentSessionId;
		private List<ServerUserData> _currentSessionPlayers;

		public async UniTask<bool> CreateSession_Server(string serverUrl, string sessionId, List<ServerUserData> playersInfo)
		{
			var data = new CreateServerSessionRequest()
			{
				Url = serverUrl,
				SessionId = sessionId,
				Players = JsonConvert.SerializeObject(playersInfo)
			};
			var url = APIConstants.WithQuery(APIConstants.SERVER_CREATE_SESSION);
			var (success, id) = await HTTPClient.Post<CreateServerSessionResponse>(url, data);
			if (success)
			{
				_currentSessionPlayers = playersInfo;
				_currentSessionPlayers.ForEach(player => _sessionLogs[player.FunticoUserId] = new List<string>());
				_currentSessionId = id.Id;
			}

			return success;
		}

		public async UniTask<bool> UserLeaveSession_Server(int platformUserId)
		{
			var url = APIConstants.WithQuery(APIConstants.SERVER_USER_LEAVE_SESSION, $"id={_currentSessionId}", $"userId={platformUserId}");
			var success = await HTTPClient.Get_Short(url);
			return success;
		}

		public async UniTask<bool> CloseCurrentSession_Server()
		{
			var url = APIConstants.WithQuery(APIConstants.SERVER_CLOSE_SESSION, $"id={_currentSessionId}");
			var success = await HTTPClient.Get_Short(url);
			if (success)
			{
				_currentSessionPlayers.ForEach(player => _sessionLogs.Remove(player.FunticoUserId));
				_currentSessionPlayers = null;
				_currentSessionId = null;
			}

			return success;
		}

		public List<string> GetCurrentSessionEvents_Server(int platformUserId) => _sessionLogs.GetValueOrDefault(platformUserId);

		public void RecordEvent_Server(int platformUserId, string eventInfo)
		{
			if (!_sessionLogs.TryGetValue(platformUserId, out var logs))
			{
				Logger.LogError($"Can't log event. Session data for user is with id = {platformUserId} is not found");
				return;
			}

			if (logs == null)
			{
				Logger.LogError($"Can't log event. Session data for user is with id = {platformUserId} is null");
				return;
			}

			logs.Add(eventInfo);
		}
	}
}