using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels.Matchmaking;
using FunticoGamesSDK.AuthDataProviders;
using Microsoft.AspNetCore.SignalR.Client;
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
		
		private HubConnection _connection;
		private CancellationTokenSource _cts;

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
				await _connection.InvokeAsync("FindMatch", region.ToShortString(), size, _cts.Token);
			}
			catch (Exception ex)
			{
				Logger.LogError($"MatchmakingService JoinQueue failed: {ex.Message}");
				await DisposeConnection();
			}
		}

		public async UniTask LeaveQueue()
		{
			if (_connection == null || _connection.State != HubConnectionState.Connected)
				return;

			try
			{
				await _connection.InvokeAsync("LeaveQueue", _cts.Token);
			}
			catch (Exception ex)
			{
				Logger.LogError($"MatchmakingService LeaveQueue failed: {ex.Message}");
			}
			finally
			{
				await DisposeConnection();
			}
		}

		private async UniTask CreateConnection()
		{
			await DisposeConnection();
			
			_cts = new CancellationTokenSource();

			var hubUrl = APIConstants.MATCHMAKER_HUB;
			var userToken = _authDataProvider.GetUserToken();

			_connection = new HubConnectionBuilder()
				.WithUrl(hubUrl, options =>
				{
					options.AccessTokenProvider = () => Task.FromResult(userToken);
					options.Headers.Add("X-User-Key", _publicGameKey);
					options.Headers.Add("X-Client-Session-Id", _clientSessionId);
				})
				.WithAutomaticReconnect(new[]
				{
					TimeSpan.Zero,
					TimeSpan.FromSeconds(1),
					TimeSpan.FromSeconds(3),
					TimeSpan.FromSeconds(10),
				})
				.Build();

			_connection.On<string>("MatchStatus", status =>
			{
				Logger.Log($"MatchStatus: {status}");
				OnMatchStatus?.Invoke(status);
			});

			_connection.On<MatchResult>("MatchFound", result =>
			{
				Logger.Log($"MatchFound: MatchId={result.MatchId} ServerUrl={result.ServerUrl} Opponents={string.Join(", ", result.Opponents.Select(user => user.UserName))}");
				OnMatchFound?.Invoke(result);
				DisposeConnection().Forget();
			});

			_connection.Reconnecting += error =>
			{
				Logger.Log($"MatchmakingService Reconnecting: {error?.Message}");
				return Task.CompletedTask;
			};

			_connection.Reconnected += newConnectionId =>
			{
				Logger.Log($"MatchmakingService Reconnected: {newConnectionId}");
				return Task.CompletedTask;
			};

			_connection.Closed += error =>
			{
				Logger.Log($"MatchmakingService Closed: {error?.Message}");
				return Task.CompletedTask;
			};

			await _connection.StartAsync(_cts.Token);
			Logger.Log($"MatchmakingService Connected: {_connection.ConnectionId}");
		}

		private async UniTask DisposeConnection()
		{
			_cts?.Cancel();
			_cts?.Dispose();
			_cts = null;

			if (_connection != null)
			{
				try
				{
					await _connection.DisposeAsync();
				}
				catch (Exception ex)
				{
					Logger.LogError($"MatchmakingService DisposeConnection failed: {ex.Message}");
				}
				_connection = null;
			}
		}

		public void Dispose()
		{
			DisposeConnection().Forget();
		}
	}
}
