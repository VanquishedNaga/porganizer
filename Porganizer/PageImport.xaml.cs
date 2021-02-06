using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Porganizer
{
    public sealed partial class PageImport : Page
    {
        public ObservableCollection<VideoFile> importingList = new ObservableCollection<VideoFile>();
        List<string> paths = new List<string>();

        public PageImport()
        {
            this.InitializeComponent();
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Import videos";
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

        private async void OpenFolder(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder selectedFolder = await folderPicker.PickSingleFolderAsync();

            if (selectedFolder != null)
            {
                // Load content of selected folder.
                LoadVideosFromFolder(selectedFolder);
            }
        }

        private async void LoadVideosFromFolder(StorageFolder selectedFolder)
        {
            if (selectedFolder != null)
            {
                // Clear old files.
                importingList.Clear();

                // Filter to get video files only.
                List<string> fileTypeFilter = new List<string>
                {
                    ".mp4",
                    ".wmv",
                    ".mkv",
                    ".avi"
                };
                QueryOptions queryOptions = new QueryOptions(CommonFileQuery.OrderByName, fileTypeFilter);
                StorageFileQueryResult results = selectedFolder.CreateFileQueryWithOptions(queryOptions);
                IReadOnlyList<StorageFile> fileList = await results.GetFilesAsync();

                // Populate file list.
                foreach (StorageFile file in fileList)
                {
                    importingList.Add(new VideoFile(file));
                }

                StatusText.Text = "Loaded " + importingList.Count + " files.";
            }

            TextFileNum.Text = importingList.Count + " files";
        }
    }
}
