namespace FunticoGamesSDK.APIModels.UserData
{
    public class CompletePracticeRequest
    {
        public int Score { get; set; }
    }

    public class CompletePracticeRequestEncoded
    {
        public string Data { get; set; }
        public string Hash { get; set; }
    }
}