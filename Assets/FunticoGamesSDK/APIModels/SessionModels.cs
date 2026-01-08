using System.Collections.Generic;

namespace FunticoGamesSDK.APIModels
{
	public class SessionModel
	{
		public string Data { get; set; }
		public List<string> EventsList { get; set; }
	}

	public class SessionModelEncrypted
	{
		public string EncryptedData { get; set; }
	}
}