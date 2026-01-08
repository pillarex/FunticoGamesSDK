using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.Encryption;
using FunticoGamesSDK.Logging;
using FunticoGamesSDK.NetworkUtils;
using FunticoGamesSDK.UserDataProviders;
using Newtonsoft.Json;

namespace FunticoGamesSDK.SessionsManagement
{
	public class ServerSessionManager : IServerSessionManager
	{
		private readonly Dictionary<int, List<string>> _sessionLogs = new Dictionary<int, List<string>>();

		public async UniTask<bool> UserHasUnfinishedSession_Server(int userId)
		{
			var url = APIConstants.WithQuery(APIConstants.UNFINISHED_SESSION, $"userId={userId}");
			var (success, _) = await HTTPClient.Get<SessionModel>(url);
			return success;
		}

		public async UniTask<string> ReconnectToUnfishedSession_Server(int userId)
		{
			var url = APIConstants.WithQuery(APIConstants.UNFINISHED_SESSION, $"userId={userId}");
			var (success, data) = await HTTPClient.Get<SessionModel>(url);
			if (!success || data == null)
				return null;

			_sessionLogs[userId] = data.EventsList; 
			return data.Data;
		}

		public async UniTask<bool> CreateSession_Server(int userId, string json)
		{
			var data = GetUserSessionModel(userId, json);
			var url = APIConstants.WithQuery(APIConstants.CREATE_SESSION, $"userId={userId}");
			var success = await HTTPClient.Post_Short(url, data);
			if (success) _sessionLogs[userId] = new List<string>();
			return success;
		}

		public async UniTask<bool> UpdateSession_Server(int userId, string json)
		{
			var data = GetUserSessionModel(userId, json);
			var url = APIConstants.WithQuery(APIConstants.UPDATE_SESSION, $"userId={userId}");
			var success = await HTTPClient.Post_Short(url, data);
			return success;
		}

		public void CloseCurrentSession_Server(int userId) => _sessionLogs.Remove(userId);

		public List<string> GetCurrentSessionEvents_Server(int userId) => _sessionLogs.GetValueOrDefault(userId);

		public void RecordEvent_Server(int userId, string eventInfo)
		{
			if (!_sessionLogs.TryGetValue(userId, out var logs))
			{
				Logger.LogError($"Can't log event. Session data for user is with id = {userId} is not found");
				return;
			}

			if (logs == null)
			{
				Logger.LogError($"Can't log event. Session data for user is with id = {userId} is null");
				return;
			}

			logs.Add(eventInfo);
		}

		private SessionModel GetUserSessionModel(int userId, string json)
		{
			var sessionLogs = _sessionLogs.GetValueOrDefault(userId);
			var data = new SessionModel()
			{
				Data = json,
				EventsList = sessionLogs
			};
			return data;
		}
	}
}