using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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


        public List<Action> CreateDiscoveryTasks()
        {
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
                    SubDirectories.Add(selectedSubDirectory);
                    taskList.AddRange(selectedSubDirectory.CreateDiscoveryTasks());
                }
            }

            List<List<FileInfo>> fileTaskGroups = new List<List<FileInfo>>();
            fileTaskGroups.Add(new List<FileInfo>());

            foreach (var file in allFiles)
            {
                if (counter >=5 && counter % 5 == 0)
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
                                NumberOfImageFiles ++;
                                NumberOfImageBytes += taskFile.Length;
                                image.Dispose();
                            }
                            
                        }
                        catch (Exception e)
                        {
                            Log.ErrorFormat("File {0} is not valid or is an unsupported image format. This file has been ignored.", file.FullName);
                        }
                    }));
                    fileTaskGroups.Add(new List<FileInfo>());
                }

                fileTaskGroups[fileTaskGroups.Count - 1].Add(file);

                counter++;
            }

            return taskList;
        }

        public List<Action> CreateFilterTasks() {
            //todo
            var taskList = new List<Action>();

            foreach (var subDir in SubDirectories)
            {
                taskList.AddRange(subDir.CreateFilterTasks());
            }

            foreach (var imageMeta in ImageMetaList)
            {
                
            }
        } 

        public Tuple<int,long> CountImagesAndBytes()
        {
            int images = 0;
            long bytes = 0;
            foreach (var directory in SubDirectories)
            {
                var subcount = directory.CountImagesAndBytes();
                images += subcount.Item1;
                bytes += subcount.Item2;
            }

            foreach (var image in ImageMetaList)
            {
                images++;
                bytes += image.FileByteSize;
            }

            NumberOfImageBytes = bytes;
            NumberOfImageFiles = images;

            RaisePropertyChangedEvent("NumberOfImageFiles");
            RaisePropertyChangedEvent("NumberOfImageBytes");

            return new Tuple<int, long>(images,bytes);
        }
    }
}