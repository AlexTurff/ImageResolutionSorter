using System;
using System.IO;
using ImageSorter.Helpers;
using ImageSorter.Models.Enums;
using ImageSorter.Models.ModelInterfaces;

namespace ImageSorter.Models
{
    public class ImageSizeFilter
    {
        private int _width;
        private int _height;
        private Orientations _orientation;
        private DirectoryInfo _destDir;
        private Action<DirectoryInfo, IImage> _opOverride;
        private ImageSizeFilter _nextFilter;

        public ImageSizeFilter(int side1, int side2, Orientations orientation, ImageSizeFilter nextFilter, DirectoryInfo destinationDirectory, Action<DirectoryInfo,IImage> operationOverride = null)
        {
            _orientation = orientation;
            _nextFilter = nextFilter;
            _opOverride = operationOverride;
            _destDir = destinationDirectory;

            switch (orientation)
            {
                case Orientations.Landscape:
                    _width = Math.Max(side1, side2);
                    _height = Math.Min(side1, side2);
                    break;
                case Orientations.Portrait:
                    _width = Math.Min(side1, side2);
                    _height = Math.Max(side1, side2);
                    break;
                case Orientations.Square:
                    if (side1 == side2)
                    {
                        _height = side1;
                        _width = side1;
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null);
            }
        }

        public void FilterImage(IImage image)
        {
            if (_opOverride != null)
            {
                _opOverride(_destDir, image);
                return;
            }

            if (image.Height >= _height && image.Width >= _width)
            {
                File.Copy(image.FilePath,_destDir.FullName + $"\\{_orientation}{_width}x{_height}\\{StaticHelpers.MakeStringFileSystemSafe(StaticHelpers.GetStringFromBytes(image.ContentHash))}.{image.FileExtension}");
            }
            else
            {
                _nextFilter.FilterImage(image);
            }
        }
    }
}