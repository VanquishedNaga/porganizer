using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Porganizer
{
    public class VideoFile : BindableBase
    {
        private static List<string> supportedVideoTypes = new List<string>
        {
            ".avi",
            ".mkv",
            ".mov",
            ".mp4",
            ".wmv",
        };

        public static bool IsSupportedVideoType(StorageFile file)
        {
            bool isVideo = false;

            if (supportedVideoTypes.Contains(file.FileType))
            {
                isVideo = true;
            }

            return isVideo;
        }

        public VideoFile()
        {
        }

        public VideoFile(StorageFile inputFile)
        {
            File = inputFile;

            GetThumbnail();
            FindScreens();
            FindGif();
        }

        public VideoFile(string filePath)
        {
            // Can't make constructor async, so need this function.
            InitFromFilePath(filePath);
        }

        private async void InitFromFilePath(string filePath)
        {
            StorageFile temp = null;
            try
            {
                temp = await StorageFile.GetFileFromPathAsync(filePath);
            }
            catch (Exception ex)
            {
            }

            if (temp != null)
            {
                File = temp;
                FileName = File.Name;

                GetThumbnail();
                FindScreens();
                FindGif();
            }
            else
            {
                string ListThumbnailPlaceholderPath = "ms-appx:///Assets/StoreLogo.scale-400.png";
                BitmapImage image = new BitmapImage(new Uri(ListThumbnailPlaceholderPath));
                FileName = Path.GetFileNameWithoutExtension(filePath);
                Thumbnail = image;
                DisplayedImage = image;
            }
        }

        private async void GetThumbnail()
        {
            BitmapImage image = new BitmapImage();

            var temp = await file.GetThumbnailAsync(ThumbnailMode.VideosView);
            if (temp != null)
            {
                await image.SetSourceAsync(temp);
                Thumbnail = image;
                DisplayedImage = image;
            }
        }

        private async void FindScreens()
        {
            List<string> fileTypeFilter = new List<string>
            {
                ".jpg"
            };

            var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, fileTypeFilter)
            {
                ApplicationSearchFilter = "System.FileName:*\"" + Path.GetFileNameWithoutExtension(File.Name) + "\"*"
            };

            StorageFolder folder = await File.GetParentAsync();
            StorageFileQueryResult queryResult = folder.CreateFileQueryWithOptions(queryOptions);

            var files = await queryResult.GetFilesAsync();
            if (files.Count > 0)
            {
                Screen = files[0];
            }
        }

        private async void FindGif()
        {
            List<string> fileTypeFilter = new List<string>
            {
                ".gif"
            };

            var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, fileTypeFilter)
            {
                ApplicationSearchFilter = "System.FileName:*\"" + Path.GetFileNameWithoutExtension(File.Name) + "\"*"
            };

            StorageFolder folder = await File.GetParentAsync();
            StorageFileQueryResult queryResult = folder.CreateFileQueryWithOptions(queryOptions);

            var files = await queryResult.GetFilesAsync();
            if (files.Count > 0)
            {
                IRandomAccessStream fileStream = await files[0].OpenAsync(FileAccessMode.Read);
                BitmapImage image = new BitmapImage();
                image.SetSource(fileStream);
                Gif = image;
            }
        }

        // Attributes.

        private int fileId;
        public int FileId
        {
            get { return this.fileId; }
            set { this.SetProperty(ref this.fileId, value); }
        }

        private StorageFile file;
        public StorageFile File
        {
            get { return this.file; }
            set { this.SetProperty(ref this.file, value); }
        }

        private string fileName;
        public string FileName
        {
            get { return this.fileName; }
            set { this.SetProperty(ref this.fileName, value); }
        }

        private StorageFile screen;
        public StorageFile Screen
        {
            get { return this.screen; }
            set { this.SetProperty(ref this.screen, value); }
        }

        private BitmapImage thumbnail;
        public BitmapImage Thumbnail
        {
            get { return this.thumbnail; }
            set { this.SetProperty(ref this.thumbnail, value); }
        }

        private BitmapImage gif;
        public BitmapImage Gif
        {
            get { return this.gif; }
            set { this.SetProperty(ref this.gif, value); }
        }

        // Could either display a thumbnail or a GIF.
        private BitmapImage displayedImage;
        public BitmapImage DisplayedImage
        {
            get { return this.displayedImage; }
            set { this.SetProperty(ref this.displayedImage, value); }
        }

        private BitmapImage screenImage;
        public BitmapImage ScreenImage
        {
            get { return this.screenImage; }
            set { this.SetProperty(ref this.screenImage, value); }
        }
    }
}
