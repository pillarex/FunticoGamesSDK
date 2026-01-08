using System;
using System.Linq;

namespace FunticoGamesSDK
{
	public class Utils
	{
		public static string RandomString(int length, int? hashCode = null)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			var random = hashCode != null ? new Random(hashCode.Value) : new Random();
			return new string(Enumerable
				.Range(0, length)
				.Select(_ => chars[random.Next(chars.Length)])
				.ToArray());
		}
	}
}