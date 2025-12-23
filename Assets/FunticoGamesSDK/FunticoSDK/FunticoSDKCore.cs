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
			APIConstants.SetupEnvironment(env);
			_userToken = userToken;
			_errorHandler = errorHandler;
			_publicGameKey = publicGameKey;
			_privateGameKey = privateGameKey;
			SetupAuthDataService();
			SetupUserDataService();
			SetupRoomsProvider();
			HTTPClient.Setup(publicGameKey, _authDataProvider, errorHandler);
			await WarmupServices();
		}

		private async UniTask WarmupServices()
		{
			await _authDataProvider.Authentication(_userToken);
			await UniTask.WhenAll(_userDataService.GetBalance(false), _userDataService.GetUserData(false));
		}
	}
}