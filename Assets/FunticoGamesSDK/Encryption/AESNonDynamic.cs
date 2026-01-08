using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FunticoGamesSDK.Encryption
{
    public class AESNonDynamic
    {
        public static string Encrypt(string plainText, string key)
        {
            byte[] encrypted;
            key = key.Length switch
            {
                > 16 => key.Substring(0, 16),
                < 16 => key.PadRight(16, key.First()),
                _ => key
            };
            byte[] iv = Encoding.UTF8.GetBytes("1234567890123456");

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    // Do not write IV to the stream since it is fixed

                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }

                    encrypted = ms.ToArray();
                }
            }
            return Convert.ToBase64String(encrypted);
        }
    
        public static string Decrypt(string encryptedText, string key)
        {
            byte[] cipherText = Convert.FromBase64String(encryptedText);
            key = key.Length switch
            {
                > 16 => key.Substring(0, 16),
                < 16 => key.PadRight(16, key.First()),
                _ => key
            };
            byte[] iv = Encoding.UTF8.GetBytes("1234567890123456");

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}