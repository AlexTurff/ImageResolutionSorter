using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Documents;
using ImageSorter.Models.ModelInterfaces;
using ImageSorter.ViewModels;
using log4net;

namespace ImageSorter.Models
{
    public class SelectedDirectory : NotifyPropertyChanged, ISelectedDirectory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SelectedDirectory));

        public DirectoryInfo DirectoryInfo { get; private set; }

        public string DirectoryPath
        {
            get { return DirectoryInfo?.FullName ?? string.Empty; }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    var newInfo = new DirectoryInfo(value);

                    DirectoryInfo = newInfo;
                }
            }
        }

        private bool _includeSubDirectories = false;
        public bool IncludeSubDirectories
        {
            get { return _includeSubDirectories; }
            set
            {
                _includeSubDirectories = value;
            }
        }

        public int NumberOfImageFiles { get; private set; }
        public long NumberOfImageBytes { get; private set; }
        public int NumberOfImagesFiltered { get; private set; }
        public bool Finished = true;

        public ConcurrentBag<ISelectedDirectory> SubDirectories { get; private set; }
        private ConcurrentBag<IImage> ImageMetaList { get; set; }

        public SelectedDirectory()
        {
            DirectoryInfo = null;
            IncludeSubDirectories = false;
            NumberOfImageBytes = 0;
            NumberOfImageFiles = 0;
            SubDirectories = null;
        }


        public List<Action> CreateDiscoveryTasks(out int fileCount)
        {
            Finished = false;
            fileCount = 0;
            NumberOfImageBytes = 0;
            NumberOfImageFiles = 0;
            NumberOfImagesFiltered = 0;

            RaisePropertyChangedEvent("NumberOfImageBytes");
            RaisePropertyChangedEvent("NumberOfImageFiles");
            RaisePropertyChangedEvent("NumberOfImagesFiltered");


            if (DirectoryInfo == null || !DirectoryInfo.Exists)
            {
                return null;
            }

            var taskList = new List<Action>();
            ImageMetaList = new ConcurrentBag<IImage>();
            SubDirectories = new ConcurrentBag<ISelectedDirectory>();
            var allFiles = DirectoryInfo.GetFiles();
            int counter = 0;

            if (IncludeSubDirectories)
            {
                var allDirectories = DirectoryInfo.GetDirectories();
                foreach (var directory in allDirectories)
                {
                    var selectedSubDirectory = new SelectedDirectory();
                    selectedSubDirectory.IncludeSubDirectories = IncludeSubDirectories;
                    selectedSubDirectory.DirectoryPath = directory.FullName;
                    selectedSubDirectory.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == "NumberOfImageFiles")
                        {
                            RaisePropertyChangedEvent("NumberOfImageFiles");
                        }
                        if (args.PropertyName == "NumberOfImageBytes")
                        {
                            RaisePropertyChangedEvent("NumberOfImageBytes");
                        }
                        if (args.PropertyName == "NumberOfImagesFiltered")
                        {
                            RaisePropertyChangedEvent("NumberOfImagesFiltered");
                        }
                    };
                    SubDirectories.Add(selectedSubDirectory);
                    int count;
                    taskList.AddRange(selectedSubDirectory.CreateDiscoveryTasks(out count));
                    fileCount += count;
                }
            }

            List<List<FileInfo>> fileTaskGroups = new List<List<FileInfo>>();
            fileTaskGroups.Add(new List<FileInfo>());

            foreach (var file in allFiles)
            {
                fileTaskGroups[fileTaskGroups.Count - 1].Add(file);

                counter++;

                if (counter == ImageMetaList.Count - 1 || counter >= 5 && counter % 5 == 0)
                {
                    List<FileInfo> files = fileTaskGroups[fileTaskGroups.Count - 1];
                    taskList.Add(new Action(() =>
                    {
                        try
                        {
                            foreach (var taskFile in files)
                            {
                                var image = Image.FromFile(taskFile.FullName);
                                ImageMetaList.Add(new ImageMetaData(taskFile, image));
                                NumberOfImageFiles++;
                                NumberOfImageBytes += taskFile.Length;
                                image.Dispose();
                            }

                            RaisePropertyChangedEvent("NumberOfImageFiles");
                            RaisePropertyChangedEvent("NumberOfImageBytes");

                        }
                        catch (Exception e)
                        {
                            Log.ErrorFormat("File {0} is not valid or is an unsupported image format. This file has been ignored.", file.FullName);
                        }
                    }));
                    fileTaskGroups.Add(new List<FileInfo>());
                }


            }

            fileCount += allFiles.Length;
            return taskList;
        }

        public List<Action> CreateFilterTasks(IImageFilter imageFilter)
        {
            var taskList = new List<Action>();

            if (IncludeSubDirectories)
            {
                foreach (var subDir in SubDirectories)
                {
                    taskList.AddRange(subDir.CreateFilterTasks(imageFilter));
                }
            }

            int counter = 0;
            List<List<IImage>> allImages = new List<List<IImage>>();
            allImages.Add(new List<IImage>());

            foreach (var imageMeta in ImageMetaList)
            {
                allImages[allImages.Count - 1].Add(imageMeta);
                counter++;

                if (counter == ImageMetaList.Count - 1 || counter >= 10 && counter % 10 == 0)
                {
                    List<IImage> images = allImages[allImages.Count - 1];

                    taskList.Add(new Action(() =>
                    {
                        try
                        {
                            foreach (var image in images)
                            {
                                imageFilter.FilterImage(image);
                                NumberOfImagesFiltered++;
                            }

                            RaisePropertyChangedEvent("NumberOfImagesFiltered");

                        }
                        catch (Exception e)
                        {
                            Log.ErrorFormat($"Failed to Filter Image at '{imageMeta.FilePath}' due to error {e}");
                        }
                    }));
                    allImages.Add(new List<IImage>());
                }
            }

            return taskList;
        }

        public Tuple<int, long> CountImagesAndBytes()
        {
            int images = 0;
            long bytes = 0;
            if (IncludeSubDirectories && SubDirectories != null)
            {
                foreach (var directory in SubDirectories)
                {
                    var subcount = directory.CountImagesAndBytes();
                    images += subcount.Item1;
                    bytes += subcount.Item2;
                }
            }

            images += NumberOfImageFiles;
            bytes += NumberOfImageBytes;

            return new Tuple<int, long>(images, bytes);
        }

        public int CountNumberOfFilteredImages()
        {
            int images = 0;

            if (IncludeSubDirectories && SubDirectories != null)
            {
                foreach (var directory in SubDirectories)
                {
                    var subcount = directory.CountNumberOfFilteredImages();
                    images += subcount;
                }
            }

            images += NumberOfImagesFiltered;

            return images;
        }

        public void FinishUp()
        {
            RaisePropertyChangedEvent("Finished");
        }
    }
}