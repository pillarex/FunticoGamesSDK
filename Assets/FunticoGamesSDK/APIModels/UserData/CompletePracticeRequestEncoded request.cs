using System.Collections.Generic;
using Newtonsoft.Json;

namespace FunticoGamesSDK.APIModels.UserData
{
    public class CompletePracticeRequest
    {
        public int Score { get; set; }

        /// <summary>Identifies this practice run, so a resent result is not counted twice.</summary>
        public string RunId { get; set; }

        /// <summary>What the run collected (RecordCollected_Client); omitted when nothing was.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, int> Collected { get; set; }
    }

    public class CompletePracticeRequestEncoded
    {
        public string Data { get; set; }
        public string Hash { get; set; }
    }
}