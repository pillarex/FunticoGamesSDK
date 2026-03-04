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
		public event Action<AcceptMatchServer> OnAcceptMatch;
		public event Action<string> OnMatchCancelled;
		public event Action<string> OnMatchError;
		public event Action OnConnectionStarted;
		public event Action OnConnectionClosed;

		private readonly IAuthDataProvider _authDataProvider;
		private readonly string _publicGameKey;
		private readonly string _clientSessionId;

		private SignalR _connection;
		private UniTaskCompletionSource<bool> _connectTcs;
		private bool _isConnected;
		private string _matchIdToAccept;

		public MatchmakingService(IAuthDataProvider authDataProvider, string publicGameKey, string sessionId)
		{
			_authDataProvider = authDataProvider;
			_publicGameKey = publicGameKey;
			_clientSessionId = sessionId;
		}

		public async UniTask JoinQueue(string tournamentId, MatchmakingRegion region, int size)
		{
			try
			{
				await CreateConnection();
				_connection.Invoke("FindMatch", region.ToShortString(), size.ToString(), tournamentId);
			}
			catch (Exception ex)
			{
				Logger.LogError($"MatchmakingService JoinQueue failed: {ex.Message}");
				DisposeConnection();
			}
		}

		public void AcceptMatch()
		{
			if (_connection == null || !_isConnected)
				return;

			try
			{
				var matchId = _matchIdToAccept;
				_connection.Invoke("AcceptMatch", matchId);
			}
			catch (Exception ex)
			{
				Logger.LogError($"MatchmakingService AcceptMatch failed: {ex.Message}");
				DisposeConnectionWithDelay().Forget();
			}
		}

		public void DeclineMatch()
		{
			if (_connection == null || !_isConnected)
				return;

			try
			{
				var matchId = _matchIdToAccept;
				_connection.Invoke("DeclineMatch", matchId);
			}
			catch (Exception ex)
			{
				Logger.LogError($"MatchmakingService DeclineMatch failed: {ex.Message}");
			}
			finally
			{
				DisposeConnectionWithDelay().Forget();
			}
		}

		public void LeaveQueue()
		{
			if (_connection == null || !_isConnected)
				return;

			try
			{
				_connection.Invoke("LeaveQueue");
			}
			catch (Exception ex)
			{
				Logger.LogError($"MatchmakingService LeaveQueue failed: {ex.Message}");
			}
			finally
			{
				DisposeConnectionWithDelay().Forget();
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

			_connection.ConnectionStarted += ConnectionStarted;
			_connection.ConnectionClosed += ConnectionClosed;

			_connection.Init(hubUrl, userToken, headers);

			SubscribeToServerEvents();

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

		private void SubscribeToServerEvents()
		{
			_connection.On<string>("MatchStatus", status =>
			{
				Logger.Log($"MatchStatus: {status}");
				OnMatchStatus?.Invoke(status);
			});

			_connection.On<string>("MatchCancelled", reason =>
			{
				Logger.Log($"MatchCancelled: {reason}");
				OnMatchCancelled?.Invoke(reason);
			});


			_connection.On<string>("MatchError", errorMsg => {
				Logger.Log($"MatchError: {errorMsg}");
				OnMatchError?.Invoke(errorMsg);
			});

			_connection.On<string>("MatchFound", result =>
			{
				var resultParsed = JsonConvert.DeserializeObject<MatchResult>(result);
				Logger.Log(
					$"MatchFound: MatchId={resultParsed.MatchId} ServerUrl={resultParsed.ServerUrl} Opponents={string.Join(", ", resultParsed.Opponents.Select(user => user.UserName))}");
				OnMatchFound?.Invoke(resultParsed);
				DisposeConnectionWithDelay().Forget();
			});

			_connection.On<string>("AcceptMatch", result =>
			{
				var resultParsed = JsonConvert.DeserializeObject<AcceptMatchServer>(result);
				_matchIdToAccept = resultParsed.MatchId.ToString();
				Logger.Log(
					$"AcceptMatchServer: MatchId={_matchIdToAccept} TimeoutSec={resultParsed.TimeoutSeconds}");
				OnAcceptMatch?.Invoke(resultParsed);
			});
		}

		private void ConnectionStarted(object sender, ConnectionEventArgs e)
		{
			_isConnected = true;
			_connectTcs?.TrySetResult(true);
			OnConnectionStarted?.Invoke();
			Logger.Log($"MatchmakingService Connected: {e.ConnectionId}");
		}

		private void ConnectionClosed(object sender, ConnectionEventArgs e)
		{
			_isConnected = false;
			OnConnectionClosed?.Invoke();
			Logger.Log($"MatchmakingService Closed: {e.ConnectionId}");
		}

		private async UniTask DisposeConnectionWithDelay(int delay = 1000)
		{
			await UniTask.Delay(delay);
			DisposeConnection();
		}

		private void DisposeConnection()
		{
			if (_connection != null)
			{
				try
				{
					_connection.Stop();
					_connection.ConnectionStarted -= ConnectionStarted;
					_connection.ConnectionClosed -= ConnectionClosed;
					Logger.Log($"MatchmakingService Connection closed");
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