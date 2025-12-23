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

		public UniTask<LoginResponse> Authentication(string platformToken)
		{
			throw new System.NotImplementedException();
		}
		public string GetUserToken()
		{
			throw new System.NotImplementedException();
		}
		public string GetPlatformToken()
		{
			throw new System.NotImplementedException();
		}
	}
}