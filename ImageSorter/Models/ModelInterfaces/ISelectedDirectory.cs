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
        ConcurrentBag<ISelectedDirectory> SubDirectories { get; }

        List<Task> CreateFilterTasks();
        List<Task> CreateDiscoveryTasks();
        Tuple<int, long> CountImagesAndBytes();
    }
}