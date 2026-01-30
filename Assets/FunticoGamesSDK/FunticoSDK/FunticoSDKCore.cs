using System;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.AssetsProvider;
using FunticoGamesSDK.NetworkUtils;

namespace FunticoGamesSDK
{
	public partial class FunticoSDK
	{
		public enum Environment
		{
			STAGING,
			PROD
		}

		private string _privateGameKey;
		private string _publicGameKey;
		private string _userToken;
		private string _sessionId;
		private IErrorHandler _errorHandler;
		
		// Public accessors for optional modules
		public string PublicGameKey => _publicGameKey;
		public string SessionId => _sessionId;

		private static FunticoSDK _instance;
		public static FunticoSDK Instance => _instance ??= new FunticoSDK(); 
		
		private FunticoSDK() {}

		public async UniTask Initialize(Environment env, string privateGameKey, string publicGameKey, string userToken, IErrorHandler errorHandler)
		{
			_userToken = userToken;
			_errorHandler = errorHandler;
			_publicGameKey = publicGameKey;
			_privateGameKey = privateGameKey;
			_sessionId = Guid.NewGuid().ToString();
			APIConstants.SetupEnvironment(env);
			AssetsLoader.Instance.Warmup();
			SetupAuthDataService();
			SetupUserDataService();
			SetupServerSessionManager();
			SetupClientSessionManager(_userDataService, _privateGameKey);
			SetupRoomsProvider(privateGameKey, _userDataService, _authDataProvider, _clientSessionManager, _serverSessionManager, _errorHandler);
			HTTPClient.Setup(publicGameKey, privateGameKey, _sessionId, _authDataProvider, errorHandler);
			await WarmupServices();
		}

		private async UniTask WarmupServices()
		{
#if !SERVER && !UNITY_SERVER
			await _authDataProvider.Authentication(_userToken);
			await UniTask.WhenAll(_userDataService.GetBalance(false), _userDataService.GetUserData(false));
#endif
		}
	}
}