using System;

namespace FunticoGamesSDK
{
	public interface IErrorHandler
	{
		public const string DefaultErrorTitle = "Oops";
		public const string DefaultButtonText = "Ok";

		public void ShowError(string errorMessage, string errorTitle = DefaultErrorTitle, string buttonText = DefaultButtonText, Action additionalActionOnCloseClick = null);
	}
}