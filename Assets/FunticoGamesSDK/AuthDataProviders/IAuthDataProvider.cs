using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;

namespace FunticoGamesSDK.AuthDataProviders
{
	public interface IAuthDataProvider
	{
		public UniTask<LoginResponse> Authentication(string platformToken);
		public string GetUserToken();
		public string GetPlatformToken();
	}
}