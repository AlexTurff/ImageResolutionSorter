using System;
using System.ComponentModel;
using ImageSorter.Models;

namespace ImageSorter.ViewModels
{
    public class ImageSorterViewModel
    {
        public SelectedDirectory SelectedDirectory { get; set; }
        public TaskManager TaskManager { get; set; }

        //private string _folderPath = "";

        //public string DirectoryPath
        //{
        //    get
        //    {
        //        return _folderPath;
        //    }
        //    set
        //    {
        //        _folderPath = value;
        //        SelectedDirectory.DirectoryPath = value;
        //    }
        //}

        //private bool _includeSubDirectories = false;

        //public bool IncludeSubDirectories
        //{
        //    get
        //    {
        //        return _includeSubDirectories;
        //    }
        //    set
        //    {
        //        _includeSubDirectories = value;
        //        SelectedDirectory.IncludeSubDirectories = value;
        //    }
        //}

        //public int NumberOfImages { get; set; }
        //public long SizeOfImages { get; set; }

        public ImageSorterViewModel()
        {
            SelectedDirectory = new SelectedDirectory();
            TaskManager = new TaskManager(SelectedDirectory);
            //SelectedDirectory.PropertyChanged += (sender, args) =>
            //{
            //    NumberOfImages = SelectedDirectory.NumberOfImageFiles;
            //    SizeOfImages = SelectedDirectory.NumberOfImageBytes;
            //};
        }


    }
}