using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ImageSorter.Helpers;
using ImageSorter.Models.ModelInterfaces;


namespace ImageSorter.Models
{
    public class TaskManager:NotifyPropertyChanged
    {
        private ISelectedDirectory SourceDirectory { get; set; }
        public DirectoryInfo DestinationDirectory { get; set; }
        public int MaxThreads { get; set; }
        private CancellationToken CancellationToken { get; set; }
        private CancellationTokenSource CancelSource { get; set; }
        public int TotalFiles { get; private set; }

        public TaskManager(ISelectedDirectory sourceDirectory)
        {
            SourceDirectory = sourceDirectory;
            CancelSource = new CancellationTokenSource();
            CancellationToken = CancelSource.Token;
        }

        public void Start()
        {
            TotalFiles = 0;
            RaisePropertyChangedEvent("TotalFiles");
            int fileCount;
            var discoveryTasks = SourceDirectory.CreateDiscoveryTasks(out fileCount);
            TotalFiles = fileCount;
            RaisePropertyChangedEvent("TotalFiles");

            Parallel.Invoke(
                new ParallelOptions() {CancellationToken = CancellationToken, MaxDegreeOfParallelism = MaxThreads},
                discoveryTasks.ToArray()); //this should block

            var filterTasks = SourceDirectory.CreateFilterTasks(CreateFilter());

            Parallel.Invoke(
                new ParallelOptions() {CancellationToken = CancellationToken, MaxDegreeOfParallelism = MaxThreads},
                filterTasks.ToArray()); //this should block
        }

        public void Stop()
        {
            CancelSource.Cancel();
        }

        private IImageFilter CreateFilter()
        {
            IImageFilter dumpFilter = new ImageSizeFilter(0,0,null,DestinationDirectory,(destDir, image, arg3, arg4) => 
            { Directory.CreateDirectory(destDir.FullName + $"\\ReallySmallImages\\");
                File.Copy(image.FilePath,destDir.FullName +$"\\ReallySmallImages\\{StaticHelpers.MakeStringFileSystemSafe(StaticHelpers.GetStringFromBytes(image.ContentHash))}.{image.FileExtension}");
            });
            IImageFilter nHD = new ImageSizeFilter(640, 360, dumpFilter, DestinationDirectory);
            IImageFilter p488 = new ImageSizeFilter(640, 480, nHD, DestinationDirectory);
            IImageFilter SVGA = new ImageSizeFilter(800, 600, p488, DestinationDirectory);
            IImageFilter qHD = new ImageSizeFilter(960, 540, SVGA, DestinationDirectory);
            IImageFilter HD = new ImageSizeFilter(1280, 720, qHD, DestinationDirectory);
            IImageFilter WXGA = new ImageSizeFilter(1360, 768, HD, DestinationDirectory);
            IImageFilter WXGAPlus = new ImageSizeFilter(1440, 900, WXGA, DestinationDirectory);
            IImageFilter HDPlus = new ImageSizeFilter(1600, 900, WXGAPlus, DestinationDirectory);
            IImageFilter WSXGAPlus = new ImageSizeFilter(1680, 1050, HDPlus, DestinationDirectory);
            IImageFilter FHD = new ImageSizeFilter(1920, 1080, WSXGAPlus, DestinationDirectory);
            IImageFilter WQHD = new ImageSizeFilter(2560, 1440, FHD, DestinationDirectory);
            IImageFilter WQXGA = new ImageSizeFilter(2560, 1600, WQHD, DestinationDirectory);
            IImageFilter QHDPlus = new ImageSizeFilter(3200, 1800, WQXGA, DestinationDirectory);
            IImageFilter K4 = new ImageSizeFilter(3840, 2160, QHDPlus, DestinationDirectory);
            IImageFilter K5 = new ImageSizeFilter(5120, 2880, K4, DestinationDirectory);
            IImageFilter K8 = new ImageSizeFilter(7680, 4320, K5, DestinationDirectory);

            return K8;
        }
    }
}