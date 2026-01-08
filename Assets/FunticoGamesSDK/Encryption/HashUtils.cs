using System;
using System.Security.Cryptography;
using System.Text;

namespace FunticoGamesSDK.Encryption
{
    public static class HashUtils
    {
        public static string HashString(string input, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = hmac.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
