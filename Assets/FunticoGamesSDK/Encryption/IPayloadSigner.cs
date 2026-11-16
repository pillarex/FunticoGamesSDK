using Cysharp.Threading.Tasks;

namespace FunticoGamesSDK.Encryption
{
    /// <summary>
    /// Which result is being signed. Each kind takes its key salt from a different id; see
    /// <see cref="IPayloadSigner.Sign"/>.
    /// </summary>
    public enum PayloadKind
    {
        /// <summary>CompletePractice. Salt id: the player's UserId.</summary>
        Practice = 0,

        /// <summary>FinishRoomSession_Client. Salt id: the room session id.</summary>
        Room = 1,

        /// <summary>ResultTournament_Client. Salt id: the tournament's saveScoreId.</summary>
        Tournament = 2,

        /// <summary>CompleteCampaignLevel. Salt id: the attempt id.</summary>
        Campaign = 3,

        /// <summary>ReportGhostResult. Salt id: the match id.</summary>
        Ghost = 4,

        /// <summary>CompleteGoalAttempt. Salt id: the attempt id.</summary>
        CommunityGoal = 5,
    }

    /// <summary>A signed result, as the API expects it: the encrypted payload and its hash.</summary>
    public sealed class SignedPayload
    {
        public string Data;
        public string Hash;
    }

    /// <summary>
    /// Signs the results the SDK sends and encrypts the saved session. When the game passes its private
    /// key to <c>Initialize</c>, the SDK uses <see cref="LocalPayloadSigner"/>. A game that keeps the key
    /// off the client passes its own signer to <c>Initialize</c> instead, typically one that forwards the
    /// call to the game's backend, where the key lives.
    /// </summary>
    /// <remarks>
    /// Any signer must return exactly what <see cref="LocalPayloadSigner"/> returns for the same input and
    /// key; the API cannot tell them apart.
    /// <para>
    /// Return null when the call fails. The SDK then handles it like a network error: nothing is sent,
    /// the call returns false or null, and the state a retry needs is kept (the practice run id, the
    /// campaign attempt, the collected counts). An exception thrown by a signer reaches the game's call
    /// unchanged.
    /// </para>
    /// </remarks>
    public interface IPayloadSigner
    {
        /// <summary>Encrypts and hashes a result.</summary>
        /// <param name="kind">Which result this is.</param>
        /// <param name="saltId">
        /// The id the key salt comes from, unshortened: the player's UserId for
        /// <see cref="PayloadKind.Practice"/>, otherwise the id named on the <see cref="PayloadKind"/> value.
        /// </param>
        /// <param name="payloadJson">The exact JSON to encrypt. Encrypt it as it is; never re-serialise it.</param>
        UniTask<SignedPayload> Sign(PayloadKind kind, string saltId, string payloadJson);

        /// <summary>Encrypts a session checkpoint (CreateSession_Client, UpdateSession_Client).</summary>
        /// <param name="userId">The player's UserId.</param>
        /// <param name="sessionJson">The exact JSON to encrypt.</param>
        /// <returns>The encrypted session, or null on failure.</returns>
        UniTask<string> EncryptSession(string userId, string sessionJson);

        /// <summary>Decrypts a saved session (ReconnectToUnfinishedSession_Client).</summary>
        /// <param name="userId">The player's UserId.</param>
        /// <param name="encryptedData">The session as the API returned it.</param>
        /// <returns>The session JSON, or null on failure.</returns>
        UniTask<string> DecryptSession(string userId, string encryptedData);
    }
}
