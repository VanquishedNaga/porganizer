using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.IO;
using Windows.Storage.Streams;
using System.Linq;

namespace Porganizer
{
    public sealed partial class PageLibrary : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        List<Performer> performerList = new List<Performer>();
        List<Series> seriesList = new List<Series>();
        Performer rightClickedPerformer;
        Stopwatch stopwatch = new Stopwatch();
        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        StorageItemMostRecentlyUsedList mru = StorageApplicationPermissions.MostRecentlyUsedList;
        VideoFile rightClickedFile;
        VideoFile selectedFile;
        ObservableCollection<VideoFile> displayedFileList = new ObservableCollection<VideoFile>();
        ObservableCollection<Performer> filePerformerList = new ObservableCollection<Performer>();

        public PageLibrary()
        {
            this.InitializeComponent();

            DataAccess.InitializeDatabase();
            LoadFilesFromDatabase();
            //Initialization = LoadFolderFromPreviousSession();

            performerList = DataAccess.GetPerformerList().ToList();
            seriesList = DataAccess.GetSeriesList().ToList();
        }

        private void DisplayStatusMessage(string message)
        {
            StatusText.Text = message;
        }

        // Get video files from database.
        private void LoadFilesFromDatabase()
        {
            displayedFileList.Clear();

            foreach (VideoFile videoFile in App.databaseVideoFiles)
            {
                displayedFileList.Add(videoFile);
            }

            DisplayStatusMessage("Loaded from database");
            TextFileNum.Text = displayedFileList.Count + " files";
        }

        private async Task LoadFolderFromPreviousSession()
        {
            // Load folder from previous session if available.
            if (localSettings.Values["PreviousFolder"] is string token)
            {
                var tempFolder = await mru.GetItemAsync(token);
                LoadFolder(tempFolder as StorageFolder);
            }
        }

        // Called when user selects a new folder.
        private async void AddFolder(object sender, RoutedEventArgs e)
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
                LoadFolder(selectedFolder);

                // Save folder in MRU.
                string mruToken = mru.Add(selectedFolder);
                localSettings.Values["PreviousFolder"] = mruToken;
            }
        }

        private async void LoadFolder(StorageFolder selectedFolder)
        {
            if (selectedFolder != null)
            {
                // Clear old files.
                displayedFileList.Clear();

                // Application now has read/write access to all contents in the picked folder (including other sub-folder contents).
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", selectedFolder);

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
                    displayedFileList.Add(new VideoFile(file));
                }
            }
        }

        private async void Screens_Clicked(object sender, RoutedEventArgs e)
        {
            if (selectedFile.Screen != null)
            {
                await Windows.System.Launcher.LaunchFileAsync(selectedFile.Screen);
            }
        }
        
        // Called when left click on list member to select.
        private async void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridView1.SelectedItem is VideoFile temp)
            {
                ClearDetailsPane();

                selectedFile = temp;

                if (selectedFile.File != null)
                {
                    // Display video details.
                    Match series = Regex.Match(Path.GetFileNameWithoutExtension(selectedFile.File.Name), @"^\[(.+?)\]\s*(.+?)\s*\((.+?)\)$");
                    if (series.Success)
                    {
                        TextFileSeries.Text = series.Groups[1].Value;
                        TextFileTitle.Text = series.Groups[3].Value;
                    }
                    else
                    {
                        TextFileSeries.Text = Path.GetFileNameWithoutExtension(selectedFile.File.Name);
                    }

                    // Always display GIF in details view.
                    if (selectedFile.Gif != null)
                    {
                        ImgThumbnail.Source = selectedFile.Gif;
                    }
                    else
                    {
                        ImgThumbnail.Source = selectedFile.Thumbnail;
                    }
                }

                // Set screenshot image.
                if ((selectedFile.Screen != null) && (selectedFile.ScreenImage == null))
                {
                    IRandomAccessStream fileStream = await selectedFile.Screen.OpenAsync(FileAccessMode.Read);
                    BitmapImage image = new BitmapImage();
                    image.SetSource(fileStream);
                    selectedFile.ScreenImage = image;
                }

                RefreshFilePerformers();

                Series fileSeries = DataAccess.GetSeriesById(selectedFile.FileId);
                SelectedFileSeries.Text = fileSeries.Name;
            }
        }

        private void ClearDetailsPane()
        {
            TextFileSeries.Text = "";
            TextFileTitle.Text = "";

            ImgThumbnail.Source = null;

            performerComboBox.SelectedIndex = -1;
            filePerformerList.Clear();
        }

        private void RefreshFilePerformers()
        {
            // Set performers list
            ObservableCollection<Performer> tempFilePerformerList = DataAccess.GetFilePerformers(selectedFile.FileId);
            filePerformerList.Clear();
            foreach (Performer performer in tempFilePerformerList)
            {
                filePerformerList.Add(performer);
            }
        }

        private async void Play_Clicked(object sender, RoutedEventArgs e)
        {
            if (rightClickedFile.File != null)
            {
                if (await Windows.System.Launcher.LaunchFileAsync(rightClickedFile.File))
                {
                    DisplayStatusMessage("Video opened.");
                }
            }
        }

        private async void Menu_Screens_Clicked(object sender, RoutedEventArgs e)
        {
            if (rightClickedFile.Screen != null)
            {
                await Windows.System.Launcher.LaunchFileAsync(rightClickedFile.Screen);
            }
        }

        private async void OpenFileLocation_Clicked(object sender, RoutedEventArgs e)
        {
            if (rightClickedFile.File != null)
            {
                StorageFolder folder = await rightClickedFile.File.GetParentAsync();
                var success = await Windows.System.Launcher.LaunchFolderAsync(folder);
            }
        }

        private void DisplayGif(object sender, PointerRoutedEventArgs e)
        {
            if ((((FrameworkElement)e.OriginalSource).DataContext is VideoFile pointedFile) && (pointedFile.Gif != null))
            {
                pointedFile.DisplayedImage = pointedFile.Gif;
            }
        }

        private void DisplayThumbnail(object sender, PointerRoutedEventArgs e)
        {
            if ((((FrameworkElement)e.OriginalSource).DataContext is VideoFile pointedFile) && (pointedFile.Thumbnail != null))
            {
                pointedFile.DisplayedImage = pointedFile.Thumbnail;
            }
        }

        private void ReloadDatabase(object sender, RoutedEventArgs e)
        {
            LoadFilesFromDatabase();
        }

        private void DeleteFile(object sender, RoutedEventArgs e)
        {
            if (selectedFile != null)
            {
                StatusText.Text = "Deleting " + selectedFile.File.Name + "...";
                DataAccess.DeleteFile(selectedFile.FileId);
                StatusText.Text = "Deleted " + selectedFile.File.Name + ".";

                // Refresh list after deleting.
                LoadFilesFromDatabase();
            }
        }

        private void EditFile(object sender, RoutedEventArgs e)
        {
            if (selectedFile != null)
            {
                this.Frame.Navigate(typeof(FormEditFile), selectedFile);
            }
        }

        private void GridView1_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            GridView gridView = (GridView)sender;

            // Single or multiple items selected?
            if (gridView1.SelectedItems.Count > 1)
            {
                multiFileSelectedFlyout.ShowAt(gridView, e.GetPosition(gridView));
            }
            else
            {
                // Right click takes effect on right clicked file, not selected file.
                rightClickedFile = ((FrameworkElement)e.OriginalSource).DataContext as VideoFile;

                if (rightClickedFile.Screen == null)
                {
                    videoMenuFlyout.Items[1].Visibility = Visibility.Collapsed;
                }

                videoMenuFlyout.ShowAt(gridView, e.GetPosition(gridView));
            }
        }

        private void AddPerformerButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if ((selectedFile != null) && (performerComboBox.SelectedItem is Performer performer))
            {
                DataAccess.AddPerformerToFile(selectedFile.FileId, performer.PerformerId);
                RefreshFilePerformers();
            }
        }

        private void performersListBox_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ListBox performersListBox = (ListBox)sender;
            rightClickedPerformer = ((FrameworkElement)e.OriginalSource).DataContext as Performer;

            performersListBoxMenuFlyout.ShowAt(performersListBox, e.GetPosition(performersListBox));
        }

        private void DeleteFilePerformer_Clicked(object sender, RoutedEventArgs e)
        {
            DataAccess.DeletePerformerFromFile(selectedFile.FileId, rightClickedPerformer.PerformerId);
            RefreshFilePerformers();
        }

        private void PerformerSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.QueryText == "")
            {
                LoadFilesFromDatabase();
            }
            else
            {
                DisplayStatusMessage(string.Format("Searching for files by \"{0}\"...", args.QueryText));
                List<VideoFile> tempList = DataAccess.GetVideoListByPerformerName(args.QueryText);

                displayedFileList.Clear();
                foreach (VideoFile videoFile in tempList)
                {
                    displayedFileList.Add(videoFile);
                }

                DisplayStatusMessage("Search completed.");
            }
        }

        private void PerformerSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
        }

        private void VideoNameSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.QueryText == "")
            {
                LoadFilesFromDatabase();
            }
            else
            {
                DisplayStatusMessage(string.Format("Searching for files containing \"{0}\"...", args.QueryText));
                List<VideoFile> tempList = DataAccess.GetVideoListByFileName(args.QueryText);

                displayedFileList.Clear();
                foreach (VideoFile videoFile in tempList)
                {
                    displayedFileList.Add(videoFile);
                }

                DisplayStatusMessage("Search completed.");
            }
        }

        private void ApplySeriesTag_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
