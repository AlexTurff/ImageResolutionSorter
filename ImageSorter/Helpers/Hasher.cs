using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;

namespace ImageSorter.Helpers
{
    public class Hasher
    {
        public static byte[] Sha256(FileInfo filePath)
        {
            //todo stop multiple access crash
            using (FileStream stream = new FileStream(filePath.FullName, FileMode.Open))
            {
                return Sha256(stream);
            }
        }

        public static byte[] Sha256(Stream fileContents)
        {
            fileContents.Position = 0;
            return SHA256.Create().ComputeHash(fileContents);
        }

        public static byte[] Sha256(string input)
        {
            return SHA256.Create().ComputeHash(StaticHelpers.GetBytesFromString(input));
        }

        public static byte[] Sha256OfImage(Image image)
        {
            using (var output = new MemoryStream())
            {
                image.Save(output, ImageFormat.Png);
                output.Position = 0;
                return Sha256(output);
            }
        }
    }
}