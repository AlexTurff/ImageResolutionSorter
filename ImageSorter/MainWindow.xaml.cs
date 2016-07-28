using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageSorter.ViewModels;
using log4net;
using Microsoft.Win32;

namespace ImageSorter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MainWindow));
        public ImageSorterViewModel ViewModel;

        public MainWindow()
        {
            InitializeComponent();
            log.Info("Entering application.");
            ViewModel = new ImageSorterViewModel();
            DataContext = ViewModel;
        }

        private void SourceBrowseButton_OnClick(object sender, RoutedEventArgs e)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog {ShowNewFolderButton = false};
            folderDialog.ShowDialog();
            SourceTextBox.Text = folderDialog.SelectedPath;
        }

        private void DestinationBrowseButton_OnClick(object sender, RoutedEventArgs e)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog { ShowNewFolderButton = true };
            folderDialog.ShowDialog();
            DestinationTextBox.Text = folderDialog.SelectedPath;
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Start();
        }

        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Stop();
        }
    }
}
