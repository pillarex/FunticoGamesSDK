using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels.Matchmaking;
using FunticoGamesSDK.AuthDataProviders;
using Newtonsoft.Json;
using Logger = FunticoGamesSDK.Logging.Logger;

namespace FunticoGamesSDK.MatchmakingProviders
{
	public class MatchmakingService : IMatchmakingService
	{
		public event Action<string> OnMatchStatus;
		public event Action<MatchResult> OnMatchFound;

		private readonly IAuthDataProvider _authDataProvider;
		private readonly string _publicGameKey;
		private readonly string _clientSessionId;
		
		private SignalR _connection;
		private UniTaskCompletionSource<bool> _connectTcs;
		private bool _isConnected;

		public MatchmakingService(IAuthDataProvider authDataProvider, string publicGameKey, string sessionId)
		{
			_authDataProvider = authDataProvider;
			_publicGameKey = publicGameKey;
			_clientSessionId = sessionId;
		}

		public async UniTask JoinQueue(MatchmakingRegion region, int size)
		{
			try
			{
				await CreateConnection();
				_connection.Invoke("FindMatch", region.ToShortString(), size.ToString());
			}
			catch (Exception ex)
			{
				Logger.LogError($"MatchmakingService JoinQueue failed: {ex.Message}");
				DisposeConnection();
			}
		}

		public async UniTask LeaveQueue()
		{
			if (_connection == null || !_isConnected)
				return;

			try
			{
				_connection.Invoke("LeaveQueue");
				await UniTask.Delay(1000); // Give time for the message to be sent
			}
			catch (Exception ex)
			{
				Logger.LogError($"MatchmakingService LeaveQueue failed: {ex.Message}");
			}
			finally
			{
				DisposeConnection();
			}
		}

		private async UniTask CreateConnection()
		{
			DisposeConnection();

			var hubUrl = APIConstants.WithQuery(APIConstants.MATCHMAKER_HUB, $"gameKey={_publicGameKey}");
			var userToken = _authDataProvider.GetUserToken();
			var headers = new Dictionary<string, string>
			{
				// { "X-User-Key", _publicGameKey },
				{ "X-Client-Session-Id", _clientSessionId }
			};

			_connection = new SignalR();
			_connectTcs = new UniTaskCompletionSource<bool>();

			_connection.ConnectionStarted += OnConnectionStarted;
			_connection.ConnectionClosed += OnConnectionClosed;

			_connection.Init(hubUrl, userToken, headers);
			
			_connection.On<string>("MatchStatus", status =>
			{
				Logger.Log($"MatchStatus: {status}");
				OnMatchStatus?.Invoke(status);
			});

			_connection.On<string>("MatchFound", async result =>
			{
				var resultParsed = JsonConvert.DeserializeObject<MatchResult>(result);
				Logger.Log($"MatchFound: MatchId={resultParsed.MatchId} ServerUrl={resultParsed.ServerUrl} Opponents={string.Join(", ", resultParsed.Opponents.Select(user => user.UserName))}");
				OnMatchFound?.Invoke(resultParsed);
				await UniTask.Delay(1000);
				DisposeConnection();
			});

			_connection.Connect();
			
			// Wait for connection with timeout
			var cts = new System.Threading.CancellationTokenSource(10000);
			try
			{
				await _connectTcs.Task.AttachExternalCancellation(cts.Token);
			}
			catch (OperationCanceledException)
			{
				throw new TimeoutException("Connection timeout");
			}
			
			Logger.Log("MatchmakingService Connected");
		}

		private void OnConnectionStarted(object sender, ConnectionEventArgs e)
		{
			_isConnected = true;
			Logger.Log($"MatchmakingService Connected: {e.ConnectionId}");
			_connectTcs?.TrySetResult(true);
		}

		private void OnConnectionClosed(object sender, ConnectionEventArgs e)
		{
			_isConnected = false;
			Logger.Log($"MatchmakingService Closed: {e.ConnectionId}");
		}

		private void DisposeConnection()
		{
			if (_connection != null)
			{
				_connection.ConnectionStarted -= OnConnectionStarted;
				_connection.ConnectionClosed -= OnConnectionClosed;
				
				try
				{
					_connection.Stop();
				}
				catch (Exception ex)
				{
					Logger.LogError($"MatchmakingService DisposeConnection failed: {ex.Message}");
				}
				_connection = null;
			}
			
			_isConnected = false;
			_connectTcs = null;
		}

		public void Dispose()
		{
			DisposeConnection();
		}
	}
}
