namespace FunticoGamesSDK.APIModels.UserData
{
    public class BalanceResponse
    {
        public double Coins { get; set; }
        public double Diamonds { get; set; }
        public uint FinalTickets { get; set; }
        public uint SemiFinalTickets { get; set; }
        public uint PrivateTickets { get; set; }
        public bool KYCVerified { get; set; }
    }   
}