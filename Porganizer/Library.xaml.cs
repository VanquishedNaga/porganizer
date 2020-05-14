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
using Windows.Storage.FileProperties;

using System.Diagnostics;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.IO;
using System.Security.Cryptography;

using DataAccessLibrary;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.Foundation;

namespace Porganizer
{
    public sealed partial class Library : Page
    {
        public ObservableCollection<VideoFile> thumbFileList = new ObservableCollection<VideoFile>();
        VideoFile rightClickedFile;
        VideoFile selectedFile;
        int selectedIndex;
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        public Task Initialization { get; private set; }
        Stopwatch stopwatch = new Stopwatch();
        StorageItemMostRecentlyUsedList mru = StorageApplicationPermissions.MostRecentlyUsedList;

        string ListThumbnailPlaceholderPath = "ms-appx:///Assets/StoreLogo.scale-400.png";

        List<DatabaseVideoFile> databaseVideoFiles = new List<DatabaseVideoFile>();

        public Library()
        {
            this.InitializeComponent();
            AddLog("Ready.");
            Initialization = LoadFolderFromPreviousSession();

            DataAccess.InitializeDatabase();

            // Initialization = LoadFromDatabase();
        }

        // Get video files from database.
        private async Task LoadFromDatabase()
        {
            databaseVideoFiles = DataAccess.GetVideoList();
            BitmapImage image = new BitmapImage(new Uri(ListThumbnailPlaceholderPath));

            foreach (DatabaseVideoFile videoFile in databaseVideoFiles)
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(videoFile.path);
                thumbFileList.Add(new VideoFile(file));
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
                thumbFileList.Clear();

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
                BitmapImage image = new BitmapImage(new Uri(ListThumbnailPlaceholderPath));
                foreach (StorageFile file in fileList)
                {
                    // thumbFileList.Add(new VideoFile { File = file, Thumbnail = image });
                    thumbFileList.Add(new VideoFile(file));
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
                if (await Windows.System.Launcher.LaunchFileAsync(temp.File))
                {
                    AddLog("Video launched.");
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

                // Display video details.
                TextFileSize.Text = ((await temp.File.GetBasicPropertiesAsync()).Size / 1024 / 1024).ToString() + " MB";
                TextFileLength.Text = (await temp.File.Properties.GetVideoPropertiesAsync()).Duration.Minutes.ToString() + " min";

                if (temp.Gif != null)
                {
                    bitmap.Source = temp.Gif;
                }
                else
                {
                    bitmap.Source = temp.Thumbnail;
                }

                // Set screenshot image.
                if (temp.Screen != null)
                {
                    BitmapImage screen = new BitmapImage();
                    screen.SetSource(await temp.Screen.GetThumbnailAsync(ThumbnailMode.SingleItem));
                    ImgScreenshot.Source = screen;
                    AddLog("Screens.");
                }
                else
                {
                    ImgScreenshot.Source = null;
                    AddLog("No screens.");
                }

                Match series = Regex.Match(Path.GetFileNameWithoutExtension(temp.File.Name), @"^\[(.+?)\]\s*(.+?)\s*\((.+?)\)$");
                if (series.Success)
                {
                    TextFileSeries.Text = series.Groups[1].Value;
                    TextFileActress.Text = series.Groups[2].Value;
                    TextFileTitle.Text = series.Groups[3].Value;
                }
                else
                {
                    TextFileSeries.Text = Path.GetFileNameWithoutExtension(temp.File.Name);
                    TextFileActress.Text = "";
                    TextFileTitle.Text = "";
                }
            }
        }

        private async void Play_Clicked(object sender, RoutedEventArgs e)
        {
            AddLog("Launching video...");
            if (await Windows.System.Launcher.LaunchFileAsync(rightClickedFile.File))
            {
                AddLog("Video launched.");
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
            AddLog("Opening folder...");
            StorageFolder folder = await rightClickedFile.File.GetParentAsync();
            var success = await Windows.System.Launcher.LaunchFolderAsync(folder);
            if (success)
            {
                AddLog("Folder opened.");
            }
        }

        // Display log in debug window.
        void AddLog(string log)
        {
            DebugOutput.Text = log + '\n' + DebugOutput.Text;
        }
    }
}
