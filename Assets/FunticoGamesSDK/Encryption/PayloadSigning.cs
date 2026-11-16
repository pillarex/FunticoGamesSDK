using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Logger = FunticoGamesSDK.Logging.Logger;

namespace FunticoGamesSDK.Encryption
{
    internal static class PayloadSigning
    {
        /// <summary>
        /// Serialises the payload and signs it with the game's signer. Null when the signer gave no usable
        /// answer; the caller then sends nothing and fails as on a network error.
        /// </summary>
        public static async UniTask<SignedPayload> Sign(IPayloadSigner signer, PayloadKind kind, string saltId, object payload)
        {
            var signed = await signer.Sign(kind, saltId, JsonConvert.SerializeObject(payload));
            if (signed != null && !string.IsNullOrEmpty(signed.Data) && !string.IsNullOrEmpty(signed.Hash))
                return signed;

            Logger.LogError($"The {kind} result could not be signed, so it was not sent.");
            return null;
        }
    }
}
