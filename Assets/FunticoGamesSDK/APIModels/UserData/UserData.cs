namespace FunticoGamesSDK.APIModels.UserData
{
    public class UserData
    {
        public string Name { get; set; }
        public double Coins { get; set; }
        public double Diamonds { get; set; }
        public int Rating { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int ExperienceToLevel { get; set; }
        public float SoundsValue { get; set; }
        public float MusicValue { get; set; }
        public uint SemiFinalTickets { get; set; }
        public uint FinalTickets { get; set; }
        public int PlatformId { get; set; }
        public int UserId { get; set; }
        public uint PrivateTickets { get; set; }
        public bool KYCVerified { get; set; }
    }   
}