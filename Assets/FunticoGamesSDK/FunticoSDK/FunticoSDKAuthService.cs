using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.AuthDataProviders;

namespace FunticoGamesSDK
{
	public partial class FunticoSDK : IAuthDataProvider
	{
		private IAuthDataProvider _authDataProvider;

		private void SetupAuthDataService()
		{
			_authDataProvider = new AuthDataProvider();
		}

		public UniTask<LoginResponse> Authentication(string platformToken) => _authDataProvider.Authentication(platformToken);

		public string GetUserToken() => _authDataProvider.GetUserToken();

		public string GetPlatformToken() => _authDataProvider.GetPlatformToken();
	}
}