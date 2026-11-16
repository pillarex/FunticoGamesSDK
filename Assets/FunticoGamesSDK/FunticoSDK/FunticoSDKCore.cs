using System;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.AssetsProvider;
using FunticoGamesSDK.Encryption;
using FunticoGamesSDK.NetworkUtils;
using FunticoGamesSDK.HallOfFameProviders;

namespace FunticoGamesSDK
{
	public partial class FunticoSDK
	{
		public enum Environment
		{
			STAGING,
			PROD
		}

		private string _publicGameKey;
		private string _userToken;
		private string _sessionId;
		private IPayloadSigner _signer;
		private IErrorHandler _errorHandler;
		private IHallOfFameProvider _hallOfFameProvider;
		
		// Public accessors for optional modules
		public string PublicGameKey => _publicGameKey;
		public string SessionId => _sessionId;

		private static FunticoSDK _instance;
		public static FunticoSDK Instance => _instance ??= new FunticoSDK(); 
		
		private FunticoSDK() {}

		/// <summary>Initializes the SDK; results are signed on the client with <paramref name="privateGameKey"/>.</summary>
		public UniTask Initialize(Environment env, string privateGameKey, string publicGameKey, string userToken, IErrorHandler errorHandler) =>
			InitializeCore(env, privateGameKey, new LocalPayloadSigner(privateGameKey), publicGameKey, userToken, errorHandler);

		/// <summary>
		/// Initializes the SDK without the private key: <paramref name="signer"/> signs every result and
		/// encrypts the saved session, so the key can stay off the client.
		/// </summary>
		public UniTask Initialize(Environment env, IPayloadSigner signer, string publicGameKey, string userToken, IErrorHandler errorHandler)
		{
			if (signer == null)
				throw new ArgumentNullException(nameof(signer), "Pass a signer, or use the Initialize overload that takes the private key.");
			return InitializeCore(env, null, signer, publicGameKey, userToken, errorHandler);
		}

		private async UniTask InitializeCore(Environment env, string privateGameKey, IPayloadSigner signer, string publicGameKey, string userToken, IErrorHandler errorHandler)
		{
			_userToken = userToken;
			_errorHandler = errorHandler;
			_publicGameKey = publicGameKey;
			_signer = signer;
			_sessionId = Guid.NewGuid().ToString();
			APIConstants.SetupEnvironment(env);
			AssetsLoader.Instance.Warmup();
			SetupAuthDataService();
			SetupUserDataService();
			SetupServerSessionManager();
			SetupClientSessionManager(_userDataService, _signer);
			SetupRoomsProvider(_signer, _userDataService, _authDataProvider, _clientSessionManager, _serverSessionManager, _errorHandler);
			SetupTournamentsProvider();
			SetupHallOfFameProvider();
			SetupCampaignProvider();
			SetupInventoryProvider();
			SetupGhostProvider();
			SetupCommunityGoalProvider();
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