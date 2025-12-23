using UnityEngine.Networking;

namespace FunticoGamesSDK.NetworkUtils
{
    public class CertificateWhore : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}