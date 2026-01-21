using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.Encryption;
using FunticoGamesSDK.Logging;
using FunticoGamesSDK.NetworkUtils;
using FunticoGamesSDK.UserDataProviders;
using Newtonsoft.Json;

namespace FunticoGamesSDK.SessionsManagement
{
	public class ClientSessionManager : IClientSessionManager
	{
		private readonly IUserDataService _userDataService;
		private List<string> _sessionLogs = new List<string>();
		private SavedSessionResponse _currentSessionData;
		private string _privateKey;

		public ClientSessionManager(IUserDataService userDataService, string privateKey)
		{
			_privateKey = privateKey;
			_userDataService = userDataService;
		}

		public async UniTask<UnfinishedSessionsResponse> UserHasUnfinishedSession_Client()
		{
			var (_, response) = await HTTPClient.Get<UnfinishedSessionsResponse>(APIConstants.UNFINISHED_SESSION);
			return response;
		}

		public async UniTask<string> ReconnectToUnfinishedSession_Client(string id)
		{
			var url = APIConstants.WithQuery(APIConstants.RECONNECT_TO_SESSION, $"id={id}");
			var (success, data) = await HTTPClient.Get<SavedSessionResponse>(url);
			if (!success)
				return null;

			_currentSessionData = data;
			var decryptedData = AESNonDynamic.Decrypt(data.Data, GetEncryptionKey());
			var sessionModel = JsonConvert.DeserializeObject<SessionModel>(decryptedData);
			if (sessionModel == null)
				return null;

			_sessionLogs = sessionModel.EventsList ?? new List<string>(); 
			return sessionModel.Data;
		}

		public async UniTask<SavedSessionResponse> CreateSession_Client(string json, GameTypeEnum gameType, string eventId, string saveSessionId)
		{
			var encryptedData = EncryptCurrentSessionData(json);
			encryptedData.GameType = (int) gameType;
			encryptedData.SessionId = eventId;
			encryptedData.SaveSessionId = saveSessionId;
			encryptedData.Hash = HashUtils.HashString(encryptedData.Data, eventId);
			var (success, response) = await HTTPClient.Post<SavedSessionResponse>(APIConstants.CREATE_SESSION, encryptedData);
			_currentSessionData = response;
			return _currentSessionData;
		}

		public async UniTask<bool> UpdateSession_Client(string json)
		{
			if (_currentSessionData == null)
			{
				Logger.LogError("Can't find started session");
				return false;
			}

			var encryptedData = EncryptCurrentSessionData(json);
			encryptedData.Id = _currentSessionData.Id;
			encryptedData.Hash = HashUtils.HashString(encryptedData.Data, _currentSessionData.SessionId);
			var success = await HTTPClient.Post_Short(APIConstants.UPDATE_SESSION, encryptedData);
			return success;
		}

		public void CloseCurrentSession_Client()
		{
			_sessionLogs.Clear();
			_currentSessionData = null;
		}

		public List<string> GetCurrentSessionEvents_Client() => _sessionLogs;

		public void RecordEvent_Client(string eventInfo) => _sessionLogs.Add(eventInfo);

		private string GetEncryptionKey()
		{
			var userGuid = IntToGuidHelper.IntToGuid(_userDataService.GetCachedUserData().UserId).ToString();
			return userGuid.Take(8).ToString() + _privateKey.TakeLast(8);
		}

		private SavedSessionResponse EncryptCurrentSessionData(string json)
		{
			var data = new SessionModel()
			{
				Data = json,
				EventsList = _sessionLogs
			};

			var encryptedData = new SavedSessionResponse()
			{
				Data = AESNonDynamic.Encrypt(JsonConvert.SerializeObject(data), GetEncryptionKey())
			};
			return encryptedData;
		}
	}
}