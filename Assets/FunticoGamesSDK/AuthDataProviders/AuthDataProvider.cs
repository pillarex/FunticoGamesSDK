using System;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.NetworkUtils;

namespace FunticoGamesSDK.AuthDataProviders
{
	public class AuthDataProvider : IAuthDataProvider
	{
		private LoginResponse _loginResponse;
		private string _platformToken;

		public async UniTask<LoginResponse> Authentication(string platformToken)
		{
			_platformToken = platformToken;
			var (_, response) = await HTTPClient.Post<LoginResponse>(APIConstants.FUNTICO_LOGIN, new
			{
				Token = platformToken,
			});

			_loginResponse = response;
			return _loginResponse;
		}

		public string GetUserToken() => _loginResponse?.Token;

		public string GetPlatformToken() => _platformToken;
	}
}