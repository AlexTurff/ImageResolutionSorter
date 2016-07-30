using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace ImageSorter.Models.ModelInterfaces
{
    public interface ISelectedDirectory
    {
        string DirectoryPath { get; set; } 
        bool IncludeSubDirectories { get; set; }
        int NumberOfImageFiles { get; }
        long NumberOfImageBytes { get; }
        int NumberOfImagesFiltered { get; }

        ConcurrentBag<ISelectedDirectory> SubDirectories { get; }

        List<Action> CreateFilterTasks(IImageFilter imageFilter);
        List<Action> CreateDiscoveryTasks(out int fileCount);
        Tuple<int, long> CountImagesAndBytes();
        int CountNumberOfFilteredImages();
        void FinishUp();
    }
}