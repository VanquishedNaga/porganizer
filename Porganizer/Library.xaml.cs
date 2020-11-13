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
    public sealed partial class Library : Page
    {
        public ObservableCollection<VideoFile> displayedFileList = new ObservableCollection<VideoFile>();
        VideoFile rightClickedFile;
        VideoFile selectedFile;
        int selectedIndex;
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        public Task Initialization { get; private set; }
        Stopwatch stopwatch = new Stopwatch();
        StorageItemMostRecentlyUsedList mru = StorageApplicationPermissions.MostRecentlyUsedList;

        List<VideoFile> databaseVideoFiles = new List<VideoFile>();

        List<Performer> performerList = new List<Performer>();

        public Library()
        {
            this.InitializeComponent();
            AddLog("Ready.");

            DataAccess.InitializeDatabase();
            LoadFilesFromDatabase();
            //Initialization = LoadFolderFromPreviousSession();

            performerList = DataAccess.GetPerformerList().ToList();
        }

        // Get video files from database.
        private void LoadFilesFromDatabase()
        {
            databaseVideoFiles = DataAccess.GetVideoList();

            foreach (VideoFile videoFile in databaseVideoFiles)
            {
                displayedFileList.Add(videoFile);
            }
        }

        private async Task LoadFolderFromPreviousSession()
        {
            // Load folder from previous session if available.
            AddLog("Looking for previous folder...");
            if (localSettings.Values["PreviousFolder"] is string token)
            {
                AddLog(string.Format("token: {0}", token));
                var tempFolder = await mru.GetItemAsync(token);
                LoadFolder(tempFolder as StorageFolder);
                AddLog("Folder loaded.");
            }
            else
            {
                AddLog("No previous folder found.");
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

                AddLog("Loading files...");
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

        private async void LaunchVideo(object sender, DoubleTappedRoutedEventArgs e)
        {
            AddLog("Launching video...");
            if (gridView1.SelectedItem is VideoFile temp)
            {
                if (temp.File != null)
                {
                    if (await Windows.System.Launcher.LaunchFileAsync(temp.File))
                    {
                        AddLog("Video launched.");
                    }
                }
            }
        }
        
        // Called when left click on list member to select.
        private async void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridView1.SelectedItem is VideoFile temp)
            {
                selectedFile = temp;
                selectedIndex = gridView1.SelectedIndex;

                if (selectedFile.File != null)
                {
                    // Display video details.
                    TextFileSize.Text = ((await selectedFile.File.GetBasicPropertiesAsync()).Size / 1024 / 1024).ToString() + " MB";
                    TextFileLength.Text = (await selectedFile.File.Properties.GetVideoPropertiesAsync()).Duration.Minutes.ToString() + " min";

                    Match series = Regex.Match(Path.GetFileNameWithoutExtension(selectedFile.File.Name), @"^\[(.+?)\]\s*(.+?)\s*\((.+?)\)$");
                    if (series.Success)
                    {
                        TextFileSeries.Text = series.Groups[1].Value;
                        TextFileActress.Text = series.Groups[2].Value;
                        TextFileTitle.Text = series.Groups[3].Value;
                    }
                    else
                    {
                        TextFileSeries.Text = Path.GetFileNameWithoutExtension(selectedFile.File.Name);
                        TextFileActress.Text = "";
                        TextFileTitle.Text = "";
                    }

                    // Always display GIF in details view.
                    if (selectedFile.Gif != null)
                    {
                        bitmap.Source = selectedFile.Gif;
                    }
                    else
                    {
                        bitmap.Source = selectedFile.Thumbnail;
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
            }
        }

        private async void Play_Clicked(object sender, RoutedEventArgs e)
        {
            if (rightClickedFile.File != null)
            {
                AddLog("Launching video...");
                if (await Windows.System.Launcher.LaunchFileAsync(rightClickedFile.File))
                {
                    AddLog("Video launched.");
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
                AddLog("Opening folder...");
                StorageFolder folder = await rightClickedFile.File.GetParentAsync();
                var success = await Windows.System.Launcher.LaunchFolderAsync(folder);
                if (success)
                {
                    AddLog("Folder opened.");
                }
            }
        }

        // Display log in debug window.
        void AddLog(string log)
        {
            DebugOutput.Text = log + '\n' + DebugOutput.Text;
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
            displayedFileList.Clear();
            LoadFilesFromDatabase();
        }

        private void DeleteFile(object sender, RoutedEventArgs e)
        {
            if (selectedFile != null)
            {
                StatusText.Text = "Deleting " + selectedFile.File.Name + "...";
                DataAccess.RemoveFileFromDatabase(selectedFile.File.Path);
                StatusText.Text = "Deleted " + selectedFile.File.Name + ".";

                // Refresh list after deleting.
                displayedFileList.Clear();
                LoadFilesFromDatabase();
            }
        }

        private void GridView1_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            GridView gridView = (GridView)sender;
            rightClickedFile = ((FrameworkElement)e.OriginalSource).DataContext as VideoFile;

            if (rightClickedFile.Screen == null)
            {
                videoMenuFlyout.Items[1].Visibility = Visibility.Collapsed;
            }

            videoMenuFlyout.ShowAt(gridView, e.GetPosition(gridView));
        }

        private void AddPerformerButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (selectedFile != null)
            {
                //DataAccess.AddPerformerToFile(selectedFile.File.Path);
            }
        }
    }
}
