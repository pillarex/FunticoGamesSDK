using System.Collections.Generic;

namespace FunticoGamesSDK.APIModels
{
    public class RoomsHolderResponse
    {
        public List<RoomConfig> Data { get; set; }
        public string Cursor { get; set; }
    }
}