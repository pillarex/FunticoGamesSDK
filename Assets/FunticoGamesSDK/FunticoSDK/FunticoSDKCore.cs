using Cysharp.Threading.Tasks;
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
		private IErrorHandler _errorHandler;

		private static FunticoSDK _instance;
		public static FunticoSDK Instance => _instance ??= new FunticoSDK(); 
		
		private FunticoSDK() {}

		public async UniTask Initialize(Environment env, string privateGameKey, string publicGameKey, string userToken, IErrorHandler errorHandler)
		{
			_userToken = userToken;
			_errorHandler = errorHandler;
			_publicGameKey = publicGameKey;
			_privateGameKey = privateGameKey;
			APIConstants.SetupEnvironment(env);
			SetupAuthDataService();
			SetupUserDataService();
			SetupServerSessionManager();
			SetupClientSessionManager(privateGameKey, _userDataService);
			SetupRoomsProvider(privateGameKey, _userDataService, _authDataProvider, _clientSessionManager, _serverSessionManager, _errorHandler);
			HTTPClient.Setup(publicGameKey, privateGameKey, _authDataProvider, errorHandler);
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