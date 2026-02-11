using System;
using System.Collections.Generic;

namespace FunticoGamesSDK.APIModels
{
    public class RoomSaveScoreRequest
    {
        public long? Score { get; set; }
        public int UserId { get; set; }
        public string? Ip { get; set; }
        public List<string> GameEvents { get; set; }
    }

    public class RoomSaveScoreRequestEncrypted
    {
        public string EncryptedData { get; set; }
        public Guid TournamentId { get; set; }
        public string Hash { get; set; }
    }

    public class FinishedUser 
    {
        public int Score { get; set; }
        public int UserId { get; set; }
        public int FunticoUserId { get; set; }
        public string UserIp { get; set; }
        public string AdditionalData { get; set; }
        public List<string> GameEvents { get; set; } = null;
    }
}