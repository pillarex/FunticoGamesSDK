using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FunticoGamesSDK.APIModels
{
	public class SessionModel
	{
		public string Data { get; set; }
		public List<string> EventsList { get; set; }

		// Saved with the checkpoint: a resumed run keeps what it had collected by then, and what it
		// collected afterwards goes away together with the progress it has to replay.
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public Dictionary<string, int> Collected { get; set; }
	}

	public class SavedSessionResponse
	{
		public string SessionId { get; set; }
		public string SaveSessionId { get; set; }
		public string Id { get; set; }
		public string Data { get; set; }
		public float ReconnectTime { get; set; }
		public int GameType { get; set; }
		public string Hash { get; set; }
	}

	public class UnfinishedSessionsResponse
	{
		public List<SavedSessionResponse> SavedSessions { get; set; }
	}
}