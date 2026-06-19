using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace AgoraIO.Media
{
    /// <summary>
    /// Vendored from AgoraIO/Tools, trimmed to what AccessToken2 needs. The original relied on
    /// ICSharpCode.SharpZipLib for zlib (de)compression; this port uses the .NET built-in
    /// <see cref="ZLibStream"/> (RFC 1950, same wire format) to avoid an extra NuGet dependency.
    /// </summary>
    public class Utils
    {
        public static int getTimestamp()
        {
            return (int)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }

        public static int randomInt()
        {
            return new Random().Next();
        }

        public static string base64Encode(byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        public static byte[] base64Decode(string data)
        {
            return Convert.FromBase64String(data);
        }

        public static bool isUUID(string uuid)
        {
            if (uuid.Length != 32)
            {
                return false;
            }

            Regex regex = new Regex("^[0-9a-fA-F]{32}$");
            return regex.IsMatch(uuid);
        }

        public static byte[] compress(byte[] data)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (var zlibStream = new ZLibStream(outputStream, CompressionMode.Compress, leaveOpen: true))
                {
                    zlibStream.Write(data, 0, data.Length);
                }
                return outputStream.ToArray();
            }
        }

        public static byte[] decompress(byte[] data)
        {
            using (MemoryStream inputStream = new MemoryStream(data))
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (var zlibStream = new ZLibStream(inputStream, CompressionMode.Decompress))
                {
                    zlibStream.CopyTo(outputStream);
                }
                return outputStream.ToArray();
            }
        }
    }
}
