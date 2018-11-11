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

namespace Porganizer
{
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<VideoFile> thumbFileList = new ObservableCollection<VideoFile>();
        VideoFile rightClickedFile;
        VideoFile selectedFile;
        int selectedIndex;
        List<Task<StorageItemThumbnail>> thumbnailOperations;
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        public Task Initialization { get; private set; }
        Stopwatch stopwatch = new Stopwatch();
        StorageItemMostRecentlyUsedList mru = StorageApplicationPermissions.MostRecentlyUsedList;


        public MainPage()
        {
            this.InitializeComponent();
            StatusText.Text = "Ready.";
            Initialization = InitAsync();
        }

        private async Task InitAsync()
        {
            // Load folder from previous session if available.
            StatusText.Text = "Looking for previous folder...";
            string token = localSettings.Values["PreviousFolder"] as string;
            StatusText.Text = string.Format("token: {0}", token);
            if (token != null)
            {
                var tempFolder = await mru.GetItemAsync(token);
                LoadFolder(tempFolder as StorageFolder);
                StatusText.Text = "Folder loaded.";
            }
            else
            {
                StatusText.Text = "No previous folder found.";
            }
        }

        private async void AddFolder(object sender, RoutedEventArgs e)
        {

            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
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

                StatusText.Text = "Loading files...";
                // Application now has read/write access to all contents in the picked folder (including other sub-folder contents).
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", selectedFolder);

                // Filter to get video files only.
                List<string> fileTypeFilter = new List<string>();
                fileTypeFilter.Add(".mp4");
                fileTypeFilter.Add(".wmv");
                fileTypeFilter.Add(".mkv");
                fileTypeFilter.Add(".avi");
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
                getThumbnails();
                findScreens(selectedFolder);
            }
            else
            {
                StatusText.Text = "Operation cancelled.";
            }
        }

        private async void getThumbnails()
        {
            // Start timer
            stopwatch.Start();

            int count = 0;

            foreach (VideoFile video in thumbFileList)
            {
                BitmapImage image = new BitmapImage();
                var temp = await video.File.GetThumbnailAsync(ThumbnailMode.VideosView);
                if (temp != null)
                {
                    await image.SetSourceAsync(temp);
                }
                video.Thumbnail = image;
                count++;
                Progress.Value = count == 0 ? 0 : (count * 100) / thumbFileList.Count;
            }

            // Operation done.
            stopwatch.Stop();
            StatusText.Text = String.Format("Ready. {0} secs.", stopwatch.ElapsedMilliseconds / 1000);
        }

        private async void findScreens(StorageFolder selectedFolder)
        {
            List<string> fileTypeFilter = new List<string>();
            fileTypeFilter.Add(".jpg");

            foreach (VideoFile video in thumbFileList)
            {
                StorageFolder folder = await video.File.GetParentAsync();
                var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, fileTypeFilter);
                queryOptions.ApplicationSearchFilter = "System.FileName:*\"" + Path.GetFileNameWithoutExtension(video.File.Name) + "\"*";

                StorageFileQueryResult queryResult = folder.CreateFileQueryWithOptions(queryOptions);

                var files = await queryResult.GetFilesAsync();
                if (files.Count > 0)
                {
                    video.Screen = files[0];
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
            StatusText.Text = "Launching video...";
            var temp = listView1.SelectedItem as VideoFile;
            if (temp != null)
            {
                if (await Windows.System.Launcher.LaunchFileAsync(temp.File))
                {
                    StatusText.Text = "Video launched.";
                }
            }
        }

        // Called when left click on list member to select.
        private async void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VideoFile temp = listView1.SelectedItem as VideoFile;
            if (temp != null)
            {
                selectedFile = temp;
                selectedIndex = listView1.SelectedIndex;

                // Display video details.
                TextFileSize.Text = ((await temp.File.GetBasicPropertiesAsync()).Size / 1024 / 1024).ToString() + " MB";
                TextFileLength.Text = (await temp.File.Properties.GetVideoPropertiesAsync()).Duration.Minutes.ToString() + " min";

                // Set preview image.
                // BitmapImage image = new BitmapImage();
                // image.SetSource(await temp.File.GetThumbnailAsync(ThumbnailMode.SingleItem));
                // bitmap.Source = image;

                bitmap.Source = temp.Thumbnail;

                // Set screenshot image.
                if (temp.Screen != null)
                {
                    BitmapImage screen = new BitmapImage();
                    screen.SetSource(await temp.Screen.GetThumbnailAsync(ThumbnailMode.SingleItem));
                    ImgScreenshot.Source = screen;
                    StatusText.Text = "Screens.";
                }
                else
                {
                    ImgScreenshot.Source = null;
                    StatusText.Text = "No screens.";
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
            StatusText.Text = rightClickedFile.File.Name;

            //FrameworkElement element = sender as FrameworkElement;
            //if (element != null) FlyoutBase.ShowAttachedFlyout(element);
        }

        private async void Play_Clicked(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Launching video...";
            if (await Windows.System.Launcher.LaunchFileAsync(rightClickedFile.File))
            {
                StatusText.Text = "Video launched.";
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
            StatusText.Text = "Opening folder...";
            StorageFolder folder = await rightClickedFile.File.GetParentAsync();
            var success = await Windows.System.Launcher.LaunchFolderAsync(folder);
            if (success)
            {
                StatusText.Text = "Folder opened.";
            }
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
