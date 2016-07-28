using System;
using System.Linq;

namespace ImageSorter.Helpers
{
    public class StaticHelpers
    {

        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static string ByteArrayToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty); ;
        }

        /// <summary>
        /// from http://stackoverflow.com/a/10380166
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] GetBytesFromString(string str)
        {
            return Convert.FromBase64String(str);
        }

        /// <summary>
        /// from http://stackoverflow.com/a/10380166
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetStringFromBytes(byte[] bytes)
        {
            
            return Convert.ToBase64String(bytes);
        }

        public static string MakeStringFileSystemSafe(string inputString)
        {
            return System.IO.Path.GetInvalidFileNameChars().Aggregate(inputString, (current, badChar) => current.Replace($"{badChar}", ""));
        }
    }
}

