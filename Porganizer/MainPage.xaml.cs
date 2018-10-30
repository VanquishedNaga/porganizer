using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
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

namespace Porganizer
{
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<VideoFile> thumbFileList = new ObservableCollection<VideoFile>();
        VideoFile rightClickedFile;
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

            // Load content of selected folder.
            LoadFolder(selectedFolder);

            // Save folder in MRU.
            string mruToken = mru.Add(selectedFolder);
            localSettings.Values["PreviousFolder"] = mruToken;
        }

        private async void LoadFolder(StorageFolder selectedFolder)
        {
            if (selectedFolder != null)
            {
                // Start timer
                stopwatch.Start();

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

                // Get thumbnails.
                int count = 0;
                foreach (StorageFile file in fileList)
                {
                    BitmapImage image = new BitmapImage();
                    var temp = await file.GetThumbnailAsync(ThumbnailMode.VideosView);
                    if (temp != null)
                    {
                        await image.SetSourceAsync(temp);
                    }
                    thumbFileList.Add(new VideoFile { File = file, Thumbnail = image });
                    count++;
                    Progress.Value = (count * 100) / fileList.Count;
                }
                // Operation done.
                stopwatch.Stop();
                TextFileNum.Text = String.Format("{0} files", fileList.Count);
                StatusText.Text = String.Format("Ready. {0} secs.", stopwatch.ElapsedMilliseconds / 1000);

                // Set detail pane.
                listView1.SelectedItem = thumbFileList[0];
            }
            else
            {
                StatusText.Text = "Operation cancelled.";
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
            StatusText.Text = "File selected.";
            VideoFile temp = listView1.SelectedItem as VideoFile;
            if (temp != null)
            {
                // Display video details.
                TextFileName.Text = temp.File.Name;
                TextFileSize.Text = ((await temp.File.GetBasicPropertiesAsync()).Size / 1024 / 1024).ToString() + " MB";
                TextFileLength.Text = (await temp.File.Properties.GetVideoPropertiesAsync()).Duration.Minutes.ToString() + " min";

                // Set preview image.
                BitmapImage image = new BitmapImage();
                image.SetSource(await temp.File.GetThumbnailAsync(ThumbnailMode.SingleItem));
                bitmap.Source = image;

                // Set screenshot image.


                Match series = Regex.Match(temp.File.Name, @"^\[(.*)\]\s?(.+)(\s+(\S+))?(\s+\(\d+\))?\.\w+$");
                if (series.Success)
                {
                    TextFileSeries.Text = series.Groups[1].Value;
                    TextFileActress.Text = series.Groups[4].Value;
                    TextFileTitle.Text = series.Groups[2].Value;
                }
                else
                {
                    TextFileSeries.Text = "";
                    TextFileActress.Text = "";
                    TextFileTitle.Text = "";
                }
            }
        }

        private void ListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ListView listView = (ListView)sender;
            videoMenuFlyout.ShowAt(listView, e.GetPosition(listView));
            rightClickedFile = ((FrameworkElement)e.OriginalSource).DataContext as VideoFile;
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
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }


}
