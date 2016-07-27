using System.IO;

namespace ImageSorter.Models.ModelInterfaces
{
    public interface IFile
    {
        string FilePath { get; set; }
        string FileExtension { get; set; }
        long FileByteSize { get; }
    }
}