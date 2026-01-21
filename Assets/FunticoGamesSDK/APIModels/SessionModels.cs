using System;
using System.Collections.Generic;

namespace FunticoGamesSDK.APIModels
{
	public class SessionModel
	{
		public string Data { get; set; }
		public List<string> EventsList { get; set; }
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