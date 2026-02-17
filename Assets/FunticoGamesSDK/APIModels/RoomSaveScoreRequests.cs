using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FunticoGamesSDK.APIModels
{
	public class RoomSaveScoreRequest
	{
		public long? Score { get; set; }
		public int UserId { get; set; }
		public string? Ip { get; set; }
		public List<string> GameEvents { get; set; }
	}

	public class RoomSaveScoreRequestEncrypted
	{
		public string EncryptedData { get; set; }
		public Guid TournamentId { get; set; }
		public string Hash { get; set; }
	}

	public class FinishedUser
	{
		public int Score { get; set; }
		public int UserId { get; set; }
		public int FunticoUserId { get; set; }
		public string UserIp { get; set; }
		public string AdditionalData { get; set; }
		public List<string> GameEvents { get; set; } = null;
	}

	public class RoomEndRequestAllUsers
	{
		[JsonProperty("scores")]
		public List<UserScore> Scores;
	}

	public class UserScore
	{
		[JsonProperty("game_session_id_or_match_id")]
		public string GameSessionIdOrMatchId;

		[JsonProperty("score")]
		public int Score;

		[JsonProperty("is_suspected_cheater")]
		public bool IsSuspectedCheater;

		[JsonProperty("user_ip_end_session")]
		public string UserIpEndSession;

		[JsonProperty("user_id")]
		public long UserId;
	}
}