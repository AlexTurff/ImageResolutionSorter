using System.Drawing;
using System.IO;
using ImageSorter.Helpers;
using ImageSorter.Models.ModelInterfaces;

namespace ImageSorter.Models
{
    public class ImageMetaData : IImage
    {
        public ImageMetaData(FileInfo fileinfo, Image image)
        {
            FileInfo = fileinfo;
            Height = image.Height;
            Width = image.Width;
            ContentHash = Hasher.Sha256OfImage(image);
            FileByteSize = fileinfo.Length;
            FileExtension = fileinfo.Extension;
        }

        private FileInfo FileInfo { get; set; }
        public string FilePath {
            get { return FileInfo?.FullName ?? string.Empty; }
            set
            {
                var newInfo = new FileInfo(value);

                if (newInfo.Exists)
                {
                    FileInfo = newInfo;
                }
            }
        }

        public string FileExtension { get; set; }

        public long FileByteSize { get; }
        public int Height { get; }
        public int Width { get; }
        public byte[] ContentHash { get; set; }
    }
}