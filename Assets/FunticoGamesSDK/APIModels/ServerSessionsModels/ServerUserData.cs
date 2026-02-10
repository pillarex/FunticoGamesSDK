namespace FunticoGamesSDK.APIModels.ServerSessionsModels
{
	public class ServerUserData
	{
		public string JoinKey { get; set; }
		public int FunticoUserId { get; set; }
		public int SdkUserId { get; set; }
		public UserGameStatus UserStatus { get; set; }
	}
}