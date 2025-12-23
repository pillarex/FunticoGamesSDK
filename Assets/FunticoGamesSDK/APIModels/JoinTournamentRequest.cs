#nullable enable
using System;

namespace FunticoGamesSDK.APIModels
{
    public class JoinTournamentRequest
    {
        public Guid TournamentId { get; set; }
        public Guid? CommunityId { get; set; }
        public string? Password { get; set; } = null;
        public string? ForExistedLobbyId { get; set; } = null;
        public bool UseVoucher { get; set; } = false;
    }
}