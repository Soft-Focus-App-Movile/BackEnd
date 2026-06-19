using System;
using System.Security.Cryptography;
using System.Text;

namespace AgoraIO.Media
{
    /// <summary>
    /// Vendored from AgoraIO/Tools. HMAC helper used to sign the AccessToken2 payload.
    /// </summary>
    public class DynamicKeyUtil
    {
        public static byte[] encodeHMAC(string key, byte[] message, string alg = "SHA1")
        {
            return encodeHMAC(Encoding.UTF8.GetBytes(key), message, alg);
        }

        public static byte[] encodeHMAC(byte[] keyBytes, byte[] textBytes, string alg = "SHA1")
        {
            KeyedHashAlgorithm hash;
            switch (alg)
            {
                case "MD5":
                    hash = new HMACMD5(keyBytes);
                    break;
                case "SHA256":
                    hash = new HMACSHA256(keyBytes);
                    break;
                case "SHA384":
                    hash = new HMACSHA384(keyBytes);
                    break;
                case "SHA512":
                    hash = new HMACSHA512(keyBytes);
                    break;
                case "SHA1":
                default:
                    hash = new HMACSHA1(keyBytes);
                    break;
            }

            byte[] hashBytes = hash.ComputeHash(textBytes);
            return hashBytes;
        }

        public static string bytesToHex(byte[] inData)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte b in inData)
            {
                builder.Append(b.ToString("X2"));
            }
            return builder.ToString().ToLower();
        }
    }
}
