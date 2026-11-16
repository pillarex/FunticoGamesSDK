using System.Globalization;
using Cysharp.Threading.Tasks;

namespace FunticoGamesSDK.Encryption
{
    /// <summary>
    /// Signs on the client with the game's private key. The SDK uses it when <c>Initialize</c> gets a
    /// private key. It is also the reference for every other <see cref="IPayloadSigner"/>: a signer must
    /// return exactly what this one returns.
    /// </summary>
    public sealed class LocalPayloadSigner : IPayloadSigner
    {
        /// <summary>The shortest key that can sign: 8 characters for the encryption key, the rest for the hash.</summary>
        public const int MinKeyLength = 9;

        private readonly string _privateKey;

        public LocalPayloadSigner(string privateKey)
        {
            _privateKey = privateKey;
        }

        /// <summary>True when the private key is long enough to sign with.</summary>
        public bool HasKey => !string.IsNullOrEmpty(_privateKey) && _privateKey.Length >= MinKeyLength;

        public UniTask<SignedPayload> Sign(PayloadKind kind, string saltId, string payloadJson)
        {
            var encrypted = AESNonDynamic.Encrypt(payloadJson, Salt(kind, saltId) + _privateKey.Substring(0, 8));
            return UniTask.FromResult(new SignedPayload
            {
                Data = encrypted,
                Hash = HashUtils.HashString(encrypted, _privateKey.Substring(8))
            });
        }

        public UniTask<string> EncryptSession(string userId, string sessionJson) =>
            UniTask.FromResult(AESNonDynamic.Encrypt(sessionJson, SessionKey(userId)));

        public UniTask<string> DecryptSession(string userId, string encryptedData) =>
            UniTask.FromResult(AESNonDynamic.Decrypt(encryptedData, SessionKey(userId)));

        /// <summary>
        /// The salt in front of the key: the first 8 characters of the id. A practice id is the player's
        /// UserId, padded on the left with zeros to 8 digits first.
        /// </summary>
        public static string Salt(PayloadKind kind, string saltId) =>
            kind == PayloadKind.Practice ? saltId.PadLeft(8, '0').Substring(0, 8) : saltId.Substring(0, 8);

        /// <summary>The first 8 characters of the player's id as a GUID, then the last 8 of the private key.</summary>
        private string SessionKey(string userId)
        {
            var userGuid = IntToGuidHelper.IntToGuid(int.Parse(userId, NumberStyles.Integer, CultureInfo.InvariantCulture));
            return userGuid.ToString().Substring(0, 8) + _privateKey.Substring(_privateKey.Length - 8);
        }
    }
}
