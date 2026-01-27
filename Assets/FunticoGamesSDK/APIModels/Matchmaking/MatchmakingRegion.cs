namespace FunticoGamesSDK.APIModels.Matchmaking
{
	public enum MatchmakingRegion
	{
		Europe,
		Asia,
		NorthAmerica,
		SouthAmerica,
		MiddleEast
	}

	public static class MatchmakingRegionExtensions
	{
		public static string ToShortString(this MatchmakingRegion region) => region switch
		{
			MatchmakingRegion.Europe => "EU",
			MatchmakingRegion.Asia => "AS",
			MatchmakingRegion.NorthAmerica => "NA",
			MatchmakingRegion.SouthAmerica => "SA",
			MatchmakingRegion.MiddleEast => "ME",
			_ => "EU"
		};

		public static MatchmakingRegion ParseShortString(string serverString) => serverString switch
		{
			"EU" => MatchmakingRegion.Europe,
			"AS" => MatchmakingRegion.Asia,
			"NA" => MatchmakingRegion.NorthAmerica,
			"SA" => MatchmakingRegion.SouthAmerica,
			"ME" => MatchmakingRegion.MiddleEast,
			_ => MatchmakingRegion.Europe
		};
	}
}
