using System.Drawing;

namespace ImageSorter.Models.ModelInterfaces
{
    public interface IImage : IFile
    {
        int Height { get;}
        int Width { get; }
        byte[] ContentHash { get; set; }
    }
}