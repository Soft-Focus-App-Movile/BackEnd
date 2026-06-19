using System.Text;

namespace AgoraIO.Media
{
    /// <summary>
    /// Vendored from AgoraIO/Tools (MemoryStreamExtensions). UTF-8 helpers used by AccessToken2.
    /// </summary>
    public static class StringExtensions
    {
        public static byte[] getBytes(this string obj)
        {
            return Encoding.UTF8.GetBytes(obj);
        }

        public static string getString(this byte[] obj)
        {
            return Encoding.UTF8.GetString(obj);
        }
    }
}
