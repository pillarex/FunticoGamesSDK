using Newtonsoft.Json;

namespace FunticoGamesSDK.APIModels.UserData
{
    public class BalanceResponse
    {
        // [JsonProperty("Coins")]
        // public double Coins { get; set; }
        [JsonProperty("Diamonds")]
        public double Tico { get; set; }
        public uint FinalTickets { get; set; }
        public uint SemiFinalTickets { get; set; }
        public uint PrivateTickets { get; set; }
        public bool KYCVerified { get; set; }
    }   
}