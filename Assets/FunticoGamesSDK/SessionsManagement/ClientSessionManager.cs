using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.Encryption;
using FunticoGamesSDK.NetworkUtils;
using FunticoGamesSDK.UserDataProviders;
using Newtonsoft.Json;

namespace FunticoGamesSDK.SessionsManagement
{
	public class ClientSessionManager : IClientSessionManager
	{
		private readonly IUserDataService _userDataService;
		private readonly string _privateGameKey;
		private List<string> _sessionLogs = new List<string>();

		public ClientSessionManager(string privateGameKey, IUserDataService userDataService)
		{
			_privateGameKey = privateGameKey;
			_userDataService = userDataService;
		}

		public async UniTask<bool> UserHasUnfinishedSession_Client()
		{
			var (success, _) = await HTTPClient.Get<SessionModelEncrypted>(APIConstants.UNFINISHED_SESSION);
			return success;
		}

		public async UniTask<string> ReconnectToUnfishedSession_Client()
		{
			var (success, data) = await HTTPClient.Get<SessionModelEncrypted>(APIConstants.UNFINISHED_SESSION);
			if (!success)
				return null;

			var key = Utils.RandomString(16, GetEncryptionKey());
			var decryptedData = AESNonDynamic.Decrypt(data.EncryptedData, key);
			var sessionModel = JsonConvert.DeserializeObject<SessionModel>(decryptedData);
			if (sessionModel == null)
				return null;

			_sessionLogs = sessionModel.EventsList ?? new List<string>(); 
			return sessionModel.Data;
		}

		public async UniTask<bool> CreateSession_Client(string json)
		{
			var encryptedData = EncryptCurrentSessionData(json);
			var success = await HTTPClient.Post_Short(APIConstants.CREATE_SESSION, encryptedData);
			return success;
		}

		public async UniTask<bool> UpdateSession_Client(string json)
		{
			var encryptedData = EncryptCurrentSessionData(json);
			var success = await HTTPClient.Post_Short(APIConstants.UPDATE_SESSION, encryptedData);
			return success;
		}

		public void CloseCurrentSession_Client() => _sessionLogs.Clear();

		public List<string> GetCurrentSessionEvents_Client() => _sessionLogs;

		public void RecordEvent_Client(string eventInfo) => _sessionLogs.Add(eventInfo);

		private int GetEncryptionKey() => _userDataService.GetCachedUserData().PlatformId * _privateGameKey.GetHashCode();

		private SessionModelEncrypted EncryptCurrentSessionData(string json)
		{
			var data = new SessionModel()
			{
				Data = json,
				EventsList = _sessionLogs
			};

			var key = Utils.RandomString(16, GetEncryptionKey());
			var encryptedData = new SessionModelEncrypted()
			{
				EncryptedData = AESNonDynamic.Encrypt(JsonConvert.SerializeObject(data), key)
			};
			return encryptedData;
		}
	}
}