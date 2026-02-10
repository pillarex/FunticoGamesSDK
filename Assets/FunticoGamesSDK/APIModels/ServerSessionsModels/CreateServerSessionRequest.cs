namespace FunticoGamesSDK.APIModels.ServerSessionsModels
{
	public class CreateServerSessionRequest
	{
		public string Url { get; set; }
		public string SessionId { get; set; }
		public string Players;
	}
}