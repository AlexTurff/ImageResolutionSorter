using System;
using System.Collections.Concurrent;
using System.IO;
using ImageSorter.Helpers;
using ImageSorter.Models.Enums;
using ImageSorter.Models.ModelInterfaces;

namespace ImageSorter.Models
{
    public class ImageSizeFilter : IImageFilter
    {
        private int _long;
        private int _short;
        private DirectoryInfo _destDir;
        private ConcurrentDictionary<string, bool> CurrentRunsHashes { get; set; }
        private Action<DirectoryInfo, IImage, int, int> _opOverride;
        private IImageFilter _nextFilter;

        public ImageSizeFilter(int side1, int side2, IImageFilter nextFilter, DirectoryInfo destinationDirectory, Action<DirectoryInfo, IImage, int, int> operationOverride = null)
        {
            _nextFilter = nextFilter;
            _opOverride = operationOverride;
            _destDir = destinationDirectory;
            _long = Math.Max(side1, side2);
            _short = Math.Min(side1, side2);
            CurrentRunsHashes = new ConcurrentDictionary<string, bool>();
        }

        public void FilterImage(IImage image)
        {
            if (_opOverride != null)
            {
                _opOverride(_destDir, image, _long, _short);
                return;
            }

            if (image.Height > image.Width)
            {
                if (image.Height >= _long && image.Width >= _short)
                {
                    if (CurrentRunsHashes.TryAdd(StaticHelpers.GetStringFromBytes(image.ContentHash), false))
                    {
                        Directory.CreateDirectory(_destDir.FullName + $"\\P{_short}x{_long}\\");
                        File.Copy(image.FilePath, _destDir.FullName + $"\\P{_short}x{_long}\\{StaticHelpers.MakeStringFileSystemSafe(StaticHelpers.GetStringFromBytes(image.ContentHash))}.{image.FileExtension}");
                    }
                    else
                    {
                        CurrentRunsHashes.TryUpdate(StaticHelpers.GetStringFromBytes(image.ContentHash), true, false);
                    }

                }
                else
                {
                    _nextFilter.FilterImage(image);
                }
            }
            else if (image.Width > image.Height)
            {
                if (image.Height >= _short && image.Width >= _long)
                {
                    if (CurrentRunsHashes.TryAdd(StaticHelpers.GetStringFromBytes(image.ContentHash), false))
                    {
                        Directory.CreateDirectory(_destDir.FullName + $"\\L{_long}x{_short}\\");
                        File.Copy(image.FilePath, _destDir.FullName + $"\\L{_long}x{_short}\\{StaticHelpers.MakeStringFileSystemSafe(StaticHelpers.GetStringFromBytes(image.ContentHash))}.{image.FileExtension}");
                    }
                    else
                    {
                        CurrentRunsHashes.TryUpdate(StaticHelpers.GetStringFromBytes(image.ContentHash), true, false);
                    }
                }
                else
                {
                    _nextFilter.FilterImage(image);
                }
            }
            else
            {
                if (image.Height >= _short && image.Width >= _long)
                {
                    if (CurrentRunsHashes.TryAdd(StaticHelpers.GetStringFromBytes(image.ContentHash), false))
                    {
                        Directory.CreateDirectory(_destDir.FullName + $"\\L{_long}x{_short}\\");
                        File.Copy(image.FilePath, _destDir.FullName + $"\\S{_long}x{_short}\\{StaticHelpers.MakeStringFileSystemSafe(StaticHelpers.GetStringFromBytes(image.ContentHash))}.{image.FileExtension}");
                    }
                    else
                    {
                        CurrentRunsHashes.TryUpdate(StaticHelpers.GetStringFromBytes(image.ContentHash), true, false);
                    }
                }
                else
                {
                    _nextFilter.FilterImage(image);
                }
            }
        }
    }
}