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
    public sealed partial class MainPage : Page
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

        // The cryptographic service provider.
        private SHA256 Sha256 = SHA256.Create();

        public MainPage()
        {
            this.InitializeComponent();
            AddLog("Ready.");
            Initialization = LoadFolderFromPreviousSession();

            DataAccess.InitializeDatabase();
        }

        private void AddData(object sender, RoutedEventArgs e)
        {
            DataAccess.AddData(Input_Box.Text);

            Output.ItemsSource = DataAccess.GetData();
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
                BitmapImage image = new BitmapImage(new Uri("ms-appx:///Assets/StoreLogo.scale-400.png"));
                foreach (StorageFile file in fileList)
                {
                    thumbFileList.Add(new VideoFile { File = file, Thumbnail = image });
                }

                TextFileNum.Text = String.Format("{0} files", fileList.Count);

                // Generate thumbnails.
                GetThumbnailsForAllFiles();
                FindScreensForAllFiles(selectedFolder);
                CheckDB();
            }
        }

        private async void GetThumbnailsForAllFiles()
        {
            AddLog("Getting thumbnails...");
            if (thumbFileList.Count > 0)
            {
                List<Task> thumbnailOperations = new List<Task>();

                // Start timer
                stopwatch.Restart();

                foreach (VideoFile video in thumbFileList)
                {
                    thumbnailOperations.Add(GetThumbnailsForOneFile(video));
                }

                await Task.WhenAll(thumbnailOperations);

                // Operation done.
                stopwatch.Stop();
                AddLog(String.Format("Got all thumbnails in {0} secs.", stopwatch.ElapsedMilliseconds / 1000));
            }
            else
            {
                AddLog("No video files.");
            }
        }

        async Task GetThumbnailsForOneFile(VideoFile video)
        {
            BitmapImage image = new BitmapImage();
            var temp = await video.File.GetThumbnailAsync(ThumbnailMode.VideosView);
            if (temp == null)
            {
                AddLog("No thumbnail found for " + video.File.Name + '.');
            }
            else
            {
                await image.SetSourceAsync(temp);
                video.Thumbnail = image;
            }            
        }

        private void FindScreensForAllFiles(StorageFolder selectedFolder)
        {
            foreach (VideoFile video in thumbFileList)
            {
                FindScreens(video);
            }
        }

        private async void FindScreens(VideoFile video)
        {
            List<string> fileTypeFilter = new List<string>
            {
                ".jpg"
            };

            var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, fileTypeFilter)
            {
                ApplicationSearchFilter = "System.FileName:*\"" + Path.GetFileNameWithoutExtension(video.File.Name) + "\"*"
            };

            StorageFolder folder = await video.File.GetParentAsync();
            StorageFileQueryResult queryResult = folder.CreateFileQueryWithOptions(queryOptions);

            var files = await queryResult.GetFilesAsync();
            if (files.Count > 0)
            {
                video.Screen = files[0];
            }
        }

        private void CheckDB()
        {

        }

        private async void Screens_Clicked(object sender, RoutedEventArgs e)
        {
            if (selectedFile.Screen != null)
            {
                await Windows.System.Launcher.LaunchFileAsync(selectedFile.Screen);
            }
        }

        private async void Rename(object sender, RoutedEventArgs e)
        {
            // Remove whitespaces from start and end.
            TextFileSeries.Text = TextFileSeries.Text.Trim();
            TextFileActress.Text = TextFileActress.Text.Trim();
            TextFileTitle.Text = TextFileTitle.Text.Trim();

            String newName = "[" + TextFileSeries.Text + "]";
            // Only include if string is not empty.
            if (TextFileActress.Text != "")
            {
                newName += " " + TextFileActress.Text;
            }
            if (TextFileTitle.Text != "")
            {
                newName += " (" + TextFileTitle.Text + ")";
            }

            try
            {
                await selectedFile.File.RenameAsync(newName + selectedFile.File.FileType);

                thumbFileList[selectedIndex].OnPropertyChanged("File");

                if (selectedFile.Screen != null)
                {
                    await selectedFile.Screen.RenameAsync(selectedFile.File.Name + selectedFile.Screen.FileType);
                }
            }
            catch (Exception)
            {
            }
        }

        private async void LaunchVideo(object sender, DoubleTappedRoutedEventArgs e)
        {
            AddLog("Launching video...");
            if (listView1.SelectedItem is VideoFile temp)
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
            if (listView1.SelectedItem is VideoFile temp)
            {
                selectedFile = temp;
                selectedIndex = listView1.SelectedIndex;

                // Display video details.
                TextFileSize.Text = ((await temp.File.GetBasicPropertiesAsync()).Size / 1024 / 1024).ToString() + " MB";
                TextFileLength.Text = (await temp.File.Properties.GetVideoPropertiesAsync()).Duration.Minutes.ToString() + " min";

                bitmap.Source = temp.Thumbnail;

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

        private void ListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ListView listView = (ListView)sender;
            rightClickedFile = ((FrameworkElement)e.OriginalSource).DataContext as VideoFile;

            if (rightClickedFile.Screen == null)
            {
                videoMenuFlyout.Items[1].Visibility = Visibility.Collapsed;
            }

            videoMenuFlyout.ShowAt(listView, e.GetPosition(listView));
            AddLog(rightClickedFile.File.Name);

            //FrameworkElement element = sender as FrameworkElement;
            //if (element != null) FlyoutBase.ShowAttachedFlyout(element);
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

        /*private async Task HashAsync(StorageFile file)
        {
            HashAlgorithmProvider alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var stream = await file.OpenStreamForReadAsync();
            var inputStream = stream.AsInputStream();
            uint capacity = 100000000;
            Windows.Storage.Streams.Buffer buffer = new Windows.Storage.Streams.Buffer(capacity);
            var hash = alg.CreateHash();

            while (true)
            {
                await inputStream.ReadAsync(buffer, capacity, InputStreamOptions.None);
                if (buffer.Length > 0)
                    hash.Append(buffer);
                else
                    break;
            }

            string hashText = CryptographicBuffer.EncodeToHexString(hash.GetValueAndReset()).ToUpper();

            inputStream.Dispose();
            stream.Dispose();

            return hashText;
        }*/

        // Compute the file's hash.
        private byte[] GetHashSha256(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))
            {
                return Sha256.ComputeHash(stream);
            }
        }

        // Display log in debug window.
        void AddLog(string log)
        {
            DebugOutput.Text = log + '\n' + DebugOutput.Text;
        }
    }

    public class VideoFile : INotifyPropertyChanged
    {
        private StorageFile file;
        private StorageFile screen;
        private BitmapImage thumbnail;
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public StorageFile File
        {
            get
            {
                return this.file;
            }
            set
            {
                this.file = value;
                this.OnPropertyChanged();
            }
        }
        public StorageFile Screen
        {
            get
            {
                return this.screen;
            }
            set
            {
                this.screen = value;
                this.OnPropertyChanged();
            }
        }
        public BitmapImage Thumbnail
        {
            get
            {
                return this.thumbnail;
            }
            set
            {
                this.thumbnail = value;
                this.OnPropertyChanged();
            }
        }
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }


}
