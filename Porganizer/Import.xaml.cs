using DataAccessLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Porganizer
{
    public sealed partial class Import : Page
    {
        public ObservableCollection<VideoFile> importingList = new ObservableCollection<VideoFile>();
        List<string> paths = new List<string>();

        public Import()
        {
            this.InitializeComponent();
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    foreach (StorageFile file in items)
                    {
                        // If it is video
                        if (VideoFile.IsSupportedVideoType(file))
                        {
                            if (!paths.Contains(file.Path))
                            {
                                importingList.Add(new VideoFile(file));
                                paths.Add(file.Path);
                            }
                        }
                    }
                }
            }
        }

        // Display log in debug window.
        void AddLog(string log)
        {
            DebugOutput.Text = log + '\n' + DebugOutput.Text;
        }

        private void ClearList(object sender, TappedRoutedEventArgs e)
        {
            importingList.Clear();
            paths.Clear();
        }

        private async void ImportToDatabaseAsync(object sender, TappedRoutedEventArgs e)
        {
            StatusText.Text = "Importing " + importingList.Count + " files.";

            Progress.Value = 0;
            Progress.Maximum = importingList.Count;

            foreach (VideoFile file in importingList)
            {
                DataAccess.AddFile(file.File.Path, (int)((await file.File.GetBasicPropertiesAsync()).Size / 1024 / 1024));
                Progress.Value++;
            }

            StatusText.Text = "Imported " + importingList.Count + " files.";
        }        
    }
}
