namespace FunticoGamesSDK.APIModels
{
	public static class Extensions
	{
		public static string GetString(this EntryFeeType feeType)
		{
			return feeType switch {
				EntryFeeType.SemifinalsTickets => "Semifinal Tickets",
				_ => feeType.ToStringWithSpaces()
			};
		}
	}
}